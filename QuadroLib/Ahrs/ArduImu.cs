using System;
using System.IO.Ports;
using System.Diagnostics;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace QuadroLib.Ahrs {
    public class ArduImu : IAhrs {
        private readonly SerialPort _port;
        private readonly byte[] IMU_buffer = new byte[30];
        private IMUState IMU_step = IMUState.ReadHeader1;
        private byte payload_length = 0;
        private byte payload_counter = 0;
        private byte message_num = 0;

        private readonly Collections.CircularByteBuffer _receiveBuffer = new Collections.CircularByteBuffer(256);
        private readonly byte[] _buf = new byte[256];

        //IMU Checksum
        private byte ck_a = 0;
        private byte ck_b = 0;
        private byte IMU_ck_a = 0;
        private byte IMU_ck_b = 0;
        private int imu_payload_error_count;
        private int imu_checksum_error_count;
        private int imu_messages_received;

        public bool imu_ok;
        public bool imuAnalogs_ok;

        public double roll_sensor;
        public double pitch_sensor;
        public double ground_course;

        public short analog_x;
        public short analog_y;
        public short analog_z;
        public short acc_x;
        public short acc_y;
        public short acc_z;

        public ArduImu(string port) {
            _port = new SerialPort(port, 57600);
            _port.Handshake = Handshake.RequestToSend;
            this._port.ReadTimeout = 0;
            this._port.ErrorReceived += PortErrorReceived;
            this._port.Open();  // If you open the port after you set the event you will endup with problems
            this._port.DiscardInBuffer();
            this._port.DataReceived += PortDataReceived;
        }

        public static void Reset(Cpu.Pin resetPin) {
            OutputPort dtr = new OutputPort(resetPin, false);
            Thread.Sleep(1);
            dtr.Write(true);
            dtr.Dispose();
        }

        private void PortErrorReceived(object sender, SerialErrorReceivedEventArgs e) {
            Debug.Print("Error: " + (e.EventType == SerialError.RXOver ? "RXOver" : e.EventType.ToString() ));
            if (e.EventType == SerialError.RXOver)
                _port.DiscardInBuffer();
        }

        private void PortDataReceived(object sender, SerialDataReceivedEventArgs e) {
            int numc = 0;
            byte data;

            if (_port.BytesToRead > 0) {
                numc = _port.Read(_buf, 0, System.Math.Min(_buf.Length, _port.BytesToRead));
                _receiveBuffer.Put(_buf, 0, numc);
                for (int i = 0; i < numc; i++) {
                    // Process bytes received
                    data = _receiveBuffer.Get();
                    switch (IMU_step) {	 	//Normally we start from zero. This is a state machine
                        case IMUState.ReadHeader1:
                            if (data == 0x44) {
                                IMU_step++; //First byte of data packet header is correct, so jump to the next step
                            }
                            break;

                        case IMUState.ReadHeader2:
                            if (data == 0x49) {
                                IMU_step++;	//Second byte of data packet header is correct
                            }
                            else {
                                IMU_step = IMUState.ReadHeader1;		 //Second byte is not correct so restart to step zero and try again.	
                            }
                            break;

                        case IMUState.ReadHeader3:
                            if (data == 0x59) {
                                IMU_step++;	//Third byte of data packet header is correct
                            }
                            else {
                                IMU_step = IMUState.ReadHeader1;		 //Third byte is not correct so restart to step zero and try again.
                            }
                            break;

                        case IMUState.ReadHeader4:
                            if (data == 0x64) {
                                IMU_step++;	//Fourth byte of data packet header is correct, Header complete
                            }
                            else {
                                IMU_step = IMUState.ReadHeader1;		 //Fourth byte is not correct so restart to step zero and try again.
                            }
                            break;
                        case IMUState.ReadPayloadLength:
                            payload_length = data;
                            checksum(payload_length);
                            IMU_step++;
                            if (payload_length > 28) {
                                IMU_step = IMUState.ReadHeader1;	 //Bad data, so restart to step zero and try again.		 
                                payload_counter = 0;
                                ck_a = 0;
                                ck_b = 0;
                                imu_payload_error_count++;
                            }
                            break;

                        case IMUState.ReadMessageID:
                            message_num = data;
                            checksum(data);
                            IMU_step++;
                            break;

                        case IMUState.ReadPayload:	// Payload data read...
                            // We stay in this state until we reach the payload_length
                            IMU_buffer[payload_counter] = data;
                            checksum(data);
                            payload_counter++;
                            if (payload_counter >= payload_length) {
                                IMU_step++;
                            }
                            break;
                        case IMUState.ReadChecksum1:
                            IMU_ck_a = data;	 // First checksum byte
                            IMU_step++;
                            break;
                        case IMUState.ReadChecksum2:
                            IMU_ck_b = data;	 // Second checksum byte

                            // We end the IMU/GPS read...
                            // Verify the received checksum with the generated checksum.. 
                            if ((ck_a == IMU_ck_a) && (ck_b == IMU_ck_b)) {
                                if (message_num == 0x02) {
                                    IMU_join_data();
                                }
                                else if (message_num == 0x03) {
                                    GPS_join_data();
                                }
                                else if (message_num == 0x04) {
                                    IMU2_join_data();
                                }
                                else if (message_num == 0x05) {
                                    IMUAnalogs_join_data();
                                }
                                else {
                                    Debug.Print("Invalid message number = " + message_num);
                                }
                            }
                            else {
                                Debug.Print("MSG Checksum error"); //bad checksum
                                imu_checksum_error_count++;
                            }
                            // Variable initialization
                            IMU_step = IMUState.ReadHeader1;
                            payload_counter = 0;
                            ck_a = 0;
                            ck_b = 0;
                            break;
                    }
                }// End for...

            }
        }

        private void checksum(byte data) {
            ck_a += data;
            ck_b += ck_a;
        }

        private void IMU_join_data() {
            /*
             * (short)((IMU_buffer[0] << 8) | IMU_buffer[1]); // BigEndian
             * (short)(IMU_buffer[0] | (IMU_buffer[1] << 8)); // LittleEndian
             */
            imu_messages_received++;

            //Storing IMU roll
            roll_sensor = (short)(IMU_buffer[0] | (IMU_buffer[1] << 8)) * 0.01;
            //roll_sensor *= 0.01; // 1/100

            //Storing IMU pitch
            pitch_sensor = ((short)(IMU_buffer[2] | (IMU_buffer[3] << 8))) * 0.01;
            //pitch_sensor *= 0.01;

            //Storing IMU heading (yaw)
            ground_course = ((short)(IMU_buffer[4] | (IMU_buffer[5] << 8))) * 0.01;

            imu_ok = true;
        }

        private void IMUAnalogs_join_data() {
            imu_messages_received++;
            //int j = 0;

            // Analog x
            analog_x = (short)((IMU_buffer[0] << 8) | IMU_buffer[1]);
            // Analog y
            analog_y = (short)((IMU_buffer[2] << 8) | IMU_buffer[3]);
            // Analog z
            analog_z = (short)((IMU_buffer[4] << 8) | IMU_buffer[5]);

            // Acc x
            acc_x = (short)((IMU_buffer[6] << 8) | IMU_buffer[7]);
            // Acc y
            acc_y = (short)((IMU_buffer[8] << 8) | IMU_buffer[9]);
            // Acc z
            acc_z = (short)((IMU_buffer[10] << 8) | IMU_buffer[11]);

            imuAnalogs_ok = true;
        }

        private void IMU2_join_data() {
            throw new NotImplementedException();
        }

        private void GPS_join_data() {
            throw new NotImplementedException();
        }

        private enum IMUState {
            ReadHeader1 = 0,
            ReadHeader2 = 1,
            ReadHeader3 = 2,
            ReadHeader4 = 3,
            ReadPayloadLength = 4,
            ReadMessageID = 5,
            ReadPayload = 6,
            ReadChecksum1 = 7,
            ReadChecksum2 = 8
        }

        void IAhrs.Get(out double roll, out double pitch, out double groundCourse) {
            roll = roll_sensor;
            pitch = pitch_sensor;
            groundCourse = ground_course;
        }

        void IAhrs.Analogs(out double x, out double y, out double z) {
            x = analog_x;
            y = analog_y;
            z = analog_z;
        }

        void IAhrs.Acc(out double x, out double y, out double z) {
            x = acc_x;
            y = acc_y;
            z = acc_z;
        }

        bool IAhrs.Ready { get { return this.imu_ok; } }
    }
}

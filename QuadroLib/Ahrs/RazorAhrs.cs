using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

#if MF
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
#endif

using Extensions;

namespace QuadroLib.Ahrs {
    /// <summary>
    /// Klasse zum Verarbeiten des Seriellem Sparkfun 9DOF Razor AHRS </br>
    /// Bevor aktuelle Daten verarbeitet werden können muss die ParseData() Methode
    /// Aufgerufen werden.
    /// </summary>
    public class RazorAhrs {
        private readonly SerialPort _port;
#if MF
        private readonly Cpu.Pin _dtr;
#endif

        private readonly byte[] _receiveBuffer = new byte[80];
        private readonly char[] _receivedChars = new char[80];
        private readonly char[] _parseBuffer = new char[80];
        private int _receiveIndex = 0;
        private bool _haveToParse = false;
        private ParserState _imuState = ParserState.FindPre;

        private static readonly char[] Pree = { '!', 'A', 'N', 'G', ':' };
        private int _preeIndex = 0;
        private const string AnalogsPree = ",AN:";
        private const string ProductSring = "Sparkfun 9DOF Razor AHRS";

        public bool HasValidData = false;

        public float Roll;
        public float Pitch;
        public float Yaw;

        /// <summary>
        /// AccX, AccY, AccZ, AnX, AnY, AnZ
        /// </summary>
        public int[] Offsets = new int[6];

        public int AccX;
        public int AccY;
        public int AccZ;
        public int AnX;
        public int AnY;
        public int AnZ;
        public int MagX;
        public int MagY;
        public int MagZ;

        private readonly bool _parseAnalogs = false;

        private enum ParserState {
            Start,
            FindOffsets,
            FindPre,
            ParseEuler
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="com">Comport</param>
        /// <param name="parseAnalogs">true, wenn auch die Analogdaten der Gyros ermittelt werden sollen</param>
        public RazorAhrs(string com, bool parseAnalogs) {
            _parseAnalogs = parseAnalogs;
            this._port = new SerialPort(com, 57600, Parity.None, 8, StopBits.One);
        }

#if MF
        public RazorAhrs(string com, bool parseAnalogs, Cpu.Pin dtr)
            : this(com, parseAnalogs) {
            this._dtr = dtr;
        }
#endif
        private bool _applyOffsets = true;

        /// <summary>
        /// Legt fest ob die Offsets gleich verrechnet werden sollen
        /// </summary>
        public bool ApplyOffsets {
            get { return this._applyOffsets; }
            set { this._applyOffsets = value; }
        }

        public void Initialize() {
            this.Initialize(true);
        }

        public void Initialize(bool reset) {
            this._port.ReadTimeout = 0;
            this._port.ErrorReceived += PortErrorReceived;
            if (reset) {
#if MF
                this.Reset();
#else
                this._port.DtrEnable = true;
#endif
            }
            this._port.Open();// If you open the port after you set the event you will endup with problems
            this._port.DiscardInBuffer();
            this._port.DataReceived += this.PortDataReceived;
            Debug.Print("Initialized.");
        }
#if MF
        private void Reset() {
            OutputPort dtr = new OutputPort(this._dtr, false);
            Thread.Sleep(1);
            dtr.Write(true);
            dtr.Dispose();
        }
#endif

        void PortDataReceived(object sender, SerialDataReceivedEventArgs e) {

            int bufferIndex = 0;
            byte offsetIndex = 0;
            int data_received;
            //Thread.Sleep(10); 
            data_received = this._port.Read(this._receiveBuffer, 0, this._receiveBuffer.Length);

            while (bufferIndex < data_received) {
                switch (this._imuState) {
                    case ParserState.Start:
                        if (this._receiveBuffer[bufferIndex] == ProductSring[this._preeIndex]) {
                            this._preeIndex++;
                            if (this._preeIndex >= ProductSring.Length) {
                                this._imuState = ParserState.FindOffsets;
                                Debug.Print("Found Offsets");
                                this._preeIndex = 0;
                            }
                        }
                        break;
                    case ParserState.FindOffsets:
                        this._receivedChars[this._receiveIndex] = (char)this._receiveBuffer[bufferIndex];
                        if (this._receivedChars[this._receiveIndex] == '\r' ||
                            this._receivedChars[this._receiveIndex] == '\n') {
                            // Full line
                            
                            if (this._receiveIndex > 2) { // mehr als nur \r\n ?
                                Debug.Print("X: " + this._receivedChars[this._receiveIndex]);
                                Array.Copy(this._receivedChars, this._parseBuffer, this._receiveIndex);
                                ParseOffset(out Offsets[offsetIndex++], this._parseBuffer);
                                this._receiveIndex = 0;
                                if (offsetIndex >= 6) {
                                    // Alle 6 Offsets erhalten
                                    _imuState = ParserState.FindPre;
                                }
                            }
                        }
                        else {
                            this._receiveIndex++;
                        }
                        break;
                    case ParserState.FindPre:
                        if (this._receiveBuffer[bufferIndex] == Pree[this._preeIndex]) {
                            this._preeIndex++;
                            if (this._preeIndex >= Pree.Length) {
                                this._imuState = ParserState.ParseEuler;
                                this._preeIndex = 0;
                            }
                        }
                        else
                            this._preeIndex = 0;
                        break;
                    case ParserState.ParseEuler:
                        this._receivedChars[this._receiveIndex] = (char)this._receiveBuffer[bufferIndex];
                        if (this._receivedChars[this._receiveIndex] == '\r' ||
                            this._receivedChars[this._receiveIndex] == '\n') {
                            // Full Line
                            if (this._receiveIndex > 10) {
                                // Parse the Data!
                                lock (this._parseBuffer) {
                                    Array.Copy(this._receivedChars, this._parseBuffer, this._receiveIndex);
                                    this.HasValidData = true;
                                }
                                this._haveToParse = true;
                                //this.ParseData();
                            }
                            this._receiveIndex = 0;
                            this._imuState = ParserState.FindPre;

                        }
                        else {
                            this._receiveIndex++;
                            if (this._receiveIndex >= this._receivedChars.Length) {
                                Debug.Print("Sentence is too long!");
                                this._receiveIndex = 0;
                                this._imuState = ParserState.FindPre;
                            }
                        }
                        break;
                }
                bufferIndex++;
            }
        }

        static void PortErrorReceived(object sender, SerialErrorReceivedEventArgs e) {

            Debug.Print("COM Error: " + e.EventType);
        }

        private static void ParseOffset(out int offsetVar, char[] data) {
            int i = 0;
            offsetVar = FastNumberParse.ParseInt(data, ref i);
        }

        /// <summary>
        /// Parst die Empfangenen Daten des AHRS
        /// Vom Mainthread aus aufrufen bevor die Daten verarbeitet werden sollen
        /// </summary>
        public void ParseData() {
            if (!_haveToParse) {
                return;
            }
            lock (this._parseBuffer) {
                _haveToParse = false;
                int i = 0;
                this.Roll = FastNumberParse.ParseFloat(this._parseBuffer, ref i);
                i++;
                this.Pitch = FastNumberParse.ParseFloat(this._parseBuffer, ref i);
                i++;
                this.Yaw = FastNumberParse.ParseFloat(this._parseBuffer, ref i);
                if (this._parseAnalogs) {
                    // + ',AN:'
                    i += AnalogsPree.Length;
                    if (_applyOffsets) {
                        this.AnX = FastNumberParse.ParseInt(this._parseBuffer, ref i) - Offsets[0];
                        i++;
                        this.AnY = FastNumberParse.ParseInt(this._parseBuffer, ref i) - Offsets[1];
                        i++;
                        this.AnZ = FastNumberParse.ParseInt(this._parseBuffer, ref i) - Offsets[2];
                        i++;
                        this.AccX = FastNumberParse.ParseInt(this._parseBuffer, ref i) - Offsets[3];
                        i++;
                        this.AccY = FastNumberParse.ParseInt(this._parseBuffer, ref i) - Offsets[4];
                        i++;
                        this.AccZ = FastNumberParse.ParseInt(this._parseBuffer, ref i) - Offsets[5];
                    }
                    else {
                        this.AnX = FastNumberParse.ParseInt(this._parseBuffer, ref i);
                        i++;
                        this.AnY = FastNumberParse.ParseInt(this._parseBuffer, ref i);
                        i++;
                        this.AnZ = FastNumberParse.ParseInt(this._parseBuffer, ref i);
                        i++;
                        this.AccX = FastNumberParse.ParseInt(this._parseBuffer, ref i);
                        i++;
                        this.AccY = FastNumberParse.ParseInt(this._parseBuffer, ref i);
                        i++;
                        this.AccZ = FastNumberParse.ParseInt(this._parseBuffer, ref i);
                    }
                    i++;
                    this.MagX = FastNumberParse.ParseInt(this._parseBuffer, ref i);
                    i++;
                    this.MagY = FastNumberParse.ParseInt(this._parseBuffer, ref i);
                    i++;
                    this.MagZ = FastNumberParse.ParseInt(this._parseBuffer, ref i);
                }
            }
        }
    }
}
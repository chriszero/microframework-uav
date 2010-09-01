using System;

using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

using Microsoft.SPOT;
using System.Threading;

using Microsoft.SPOT.Hardware;
using QuadroLib;


namespace MFConsoleApplication1 {
    public class Program {
        private static long tstamp = 0;
        public static void Main() {

            InputPort button = new InputPort((Cpu.Pin)FEZ_Pin.Digital.Di13, true, Port.ResistorMode.PullUp);

            AnalogChannel a1 = new AnalogChannel((Cpu.Pin)FEZ_Pin.Interrupt.Di1, 30);

            OutputPort led1 = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di12, false);
            MotorAxis rollAxis = new MotorAxis((PWM.Pin) FEZ_Pin.PWM.Di10, (PWM.Pin) FEZ_Pin.PWM.Di9, Periods.P500Hz);

            RazorAhrs dof = new RazorAhrs("com2", true, (Cpu.Pin)FEZ_Pin.Digital.UEXT10);
            dof.Initialize(true);

            PidControl2 pid = new PidControl2(1, 0.1, 0.001, 0.0003) {SP = 0, CvMax = 100};

            //OldPidControl opid = new OldPidControl(4,0,2);

            Thread.Sleep(3000);
            while (button.Read()) {
                led1.Write(!led1.Read());
                Thread.Sleep(200);
            }
            led1.Write(true);
            Thread.Sleep(2000);
            rollAxis.ArmMotors();

            dof.Offsets[1] = 377;

            StopWatch stw = new StopWatch();
            while (true) {
                double dt = stw.ElapsedMillis();
                stw.Start();

                dof.ParseData();
                //float cv = pid.UpdatePiDangle(0, dof.AccY, (float)dt);
                pid.Update(dof.AccY, dt);

                Debug.Print("In:"+ dof.AccY +"\tout:" + pid.Cv + "\tdt:" + dt);

                rollAxis.SetOffset(Motors.Left, 70 - (int)pid.Cv);
                rollAxis.SetOffset(Motors.Right, 70 + (int)pid.Cv);
                rollAxis.Write();
                
                if (!button.Read()) {
                    Debug.Print("OFF");
                    break;
                }
                
            }
            led1.Write(false);
            rollAxis.LockMotors();
        }

        private static double ScaleXboxHat(short inp, short dead, short max) {
            double scaled = (inp / (double)short.MaxValue) * max;
            if (scaled > -dead && scaled < dead) {
                scaled = 0;
            }
            else if (scaled < -dead) {
                scaled += dead;
            }
            else if (scaled > dead) {
                scaled -= dead;
            }
            //Debug.Print(":" + scaled);
            return scaled;
        }
    }
}
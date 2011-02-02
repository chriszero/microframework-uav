using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using QuadroLib;
using QuadroLib.Ahrs;
using QuadroLib.Control;
using QuadroLib.Acuator;
using GHIElectronics.NETMF.Hardware;
using QuadroLib.Util;

namespace QuadTest {
    public class Program {
        public static void Main() {
            ArduImu.Reset((Cpu.Pin)FEZ_Pin.Digital.UEXT10);
            IAhrs dof = new ArduImu("com2");
            IControl pid = new PidControl2(1, 0.1, 0.001, 0.0003) { SP = 0, CvMax = 100 };
            MotorAxis rollAxis = new MotorAxis((PWM.Pin)FEZ_Pin.PWM.Di10, (PWM.Pin)FEZ_Pin.PWM.Di9, Periods.P50Hz);

            double x,y,z;
            double xOut;
            while (true) {
                if (dof.Ready) {
                    dof.Get(out x, out y, out z);
                    pid.Compute(0, x, out xOut);

                    rollAxis.SetOffset(Motors.Left, 70 - (int)xOut);
                    rollAxis.SetOffset(Motors.Right, 70 + (int)xOut);
                    rollAxis.Write();

                    Debug.Print("X-Achse: " + x.ToString("N2") + "\tX-Out: " + xOut.ToString("N2"));
                }
                Thread.Sleep(20);
            }

            Thread.Sleep(System.Threading.Timeout.Infinite);
        }
    }
}

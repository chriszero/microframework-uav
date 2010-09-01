using GHIElectronics.NETMF.Hardware;
using QuadroLib.Util;
using Extensions;


namespace QuadroLib.Acuator {
    public class BrushlessMotor {
        protected static uint Period;// = 20 * 1000 * 1000;
        protected const uint Max = 20 * 100 * 1000;
        protected const uint Min = 10 * 100 * 1000;

        private const uint Precalc = (Max - Min) / 255;

        private readonly PWM _pwmPin;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pin">PWM Pin</param>
        public BrushlessMotor(PWM.Pin pin)
            : this(pin, Periods.P50Hz) {
        }

        public BrushlessMotor(PWM.Pin pin, Periods period) {
            Period = (uint)period;
            this._pwmPin = new PWM(pin);
            this._pwmPin.SetPulse(Period, Min);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="power 0...255</param>
        public void SetPower(int power) {
            uint highTime = (uint)(Min + (Precalc * power.Constrain(0, 255)));
            //Debug.Print(highTime.ToString());
            this._pwmPin.SetPulse(Period, highTime);
        }

        public void Stop() {
            this._pwmPin.SetPulse(Period, Min);
        }

        public void Full() {
            this._pwmPin.SetPulse(Period, Max);
        }
    }
}
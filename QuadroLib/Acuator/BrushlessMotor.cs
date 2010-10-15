using GHIElectronics.NETMF.Hardware;
using QuadroLib.Util;
using Extensions;


namespace QuadroLib.Acuator {
    /// <summary>
    /// Class to controll an Electronic Speed Controler (ESC) for Airplanes (NO REVERSE)
    /// </summary>
    public class BrushlessMotor {
        protected static uint Period;// = 20 * 1000 * 1000;
        protected const uint Max = 20 * 100 * 1000;
        protected const uint Min = 10 * 100 * 1000;

        // Scale
        private const uint Precalc = (Max - Min) / 255;

        private readonly PWM _pwmPin;

        /// <summary>
        /// Uses 50Hz
        /// </summary>
        /// <param name="pin">PWM Pin</param>
        public BrushlessMotor(PWM.Pin pin)
            : this(pin, Periods.P50Hz) {
        }

        /// <summary>
        /// Use higher period, means faster response, not supported by all ESC
        /// </summary>
        /// <param name="pin">PWM pin</param>
        /// <param name="period">Period</param>
        public BrushlessMotor(PWM.Pin pin, Periods period) {
            Period = (uint)period;
            this._pwmPin = new PWM(pin);
            this._pwmPin.SetPulse(Period, Min);
        }

        /// <summary>
        /// Sets the outputpower, from 0 to 255 (finer resolution than 0-100%)
        /// </summary>
        /// <param name="power">0...255</param>
        public void SetPower(int power) {
            uint highTime = (uint)(Min + (Precalc * power.Constrain(0, 255)));
            this._pwmPin.SetPulse(Period, highTime);
        }

        public void Stop() {
            this._pwmPin.SetPulse(Period, Min);
        }

        public void Full() {
            this._pwmPin.SetPulse(Period, Max);
        }
    }

    public enum Periods : uint {
        /// <summary>
        /// Standard rate
        /// </summary>
        P50Hz = 20 * 1000000,
        /// <summary>
        /// Turbo PWM
        /// </summary>
        P400Hz = 25 * 100000,
        P500Hz = 2 * 1000000,
    }
}
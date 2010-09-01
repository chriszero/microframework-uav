using System;

using GHIElectronics.NETMF.Hardware;

using Microsoft.SPOT;

namespace QuadroLib.Acuator {
    public class ServoMotor {
        private const uint Period = 20 * 1000 * 1000;
        private const uint Max = 25 * 100 * 1000;
        private const uint Min = 6 * 100 * 1000;

        private const uint Precalc = (Max - Min) / 180;

        private readonly PWM _pwmPin;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pin">Pwm Pin</param>
        public ServoMotor(PWM.Pin pin) {
            this._pwmPin = new PWM(pin); ;
            this._pwmPin.SetPulse(Period, Min);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deg">0 ... 180</param>
        public void SetDegree(byte deg) {
            if (deg > 180) {
                deg = 180;
            }
            if (deg < 0) {
                deg = 0;
            }
            this._pwmPin.SetPulse(Period, CalcHightime(deg));
        }

        private static uint CalcHightime(byte power) {
            uint highTime = Min + Precalc * power;
            return highTime;
        }
    }
}
using System;
using GHIElectronics.NETMF.Hardware;

using Microsoft.SPOT;
using QuadroLib.Acuator;

namespace QuadroLib {
    public class MotorAxis {
        private readonly BrushlessMotor _left;
        private readonly BrushlessMotor _right;
        private bool _armed;

        private readonly int[] _motorCommand = new int[2];

        public MotorAxis(PWM.Pin leftMotorPin, PWM.Pin rightMotorPin, QuadroLib.Util.Periods period) {
            _left = new BrushlessMotor(leftMotorPin, period);
            _right = new BrushlessMotor(rightMotorPin, period);
            this.LockMotors();
        }

        /// <summary>
        /// Gibt die Motoren frei
        /// </summary>
        public void ArmMotors() {
            _armed = true;
        }

        /// <summary>
        /// Verriegelt die Motoren und stellt sie auf 0 Schub, sollte keinesfalls im Flug passieren
        /// </summary>
        public void LockMotors() {
            _left.Stop();
            _right.Stop();
            _armed = false;
        }

        public void SetOffset(Motors motor, int value) {
            _motorCommand[(int) motor] = value;
        }

        public void Write() {
            if (_armed) {
                _left.SetPower(this._motorCommand[(int) Motors.Left]);
                _right.SetPower(this._motorCommand[(int) Motors.Right]);
            }
        }
    }
}

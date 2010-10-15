using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace QuadroLib.Input {
    public class AnalogChannel : PwmIn {
        private readonly double _scale = 100.0;
        // defaults
        private long _negativeHightime = 1*10000;
        private long _neutralHightime = 15*1000;
        private long _positiveHightime = 2*10000;

        /// <summary>
        /// 
        /// </summary>
        public uint NeutralHysteresis = 2;

        /// <summary>
        /// Actual position of the stick
        /// </summary>
        public short Position;

        private double _deltaPositive;
        private double _deltaNegative;

        /// <summary>
        /// Interrupt Pin
        /// </summary>
        /// <param name="pin"></param>
        public AnalogChannel(Cpu.Pin pin) :base(pin) {
            this.Precalc();
        }

        /// <summary>
        /// Initializes an AnalogChannel and applies a scale
        /// </summary>
        /// <param name="pin">Interrupt Pin</param>
        /// <param name="scale">+/- Scale eg. 30°</param>
        public AnalogChannel(Cpu.Pin pin, double scale)
            : this(pin) {
            _scale = scale;
        }

        public long NegativeHightime {
            get { return this._negativeHightime; }
            set {
                this._negativeHightime = value;
                this.Precalc();
            }
        }

        public long NeutralHightime {
            get { return this._neutralHightime; }
            set { 
                this._neutralHightime = value;
                this.Precalc();
            }
        }

        public long PositiveHightime {
            get { return this._positiveHightime; }
            set {
                this._positiveHightime = value;
                this.Precalc();
            }
        }

        /// <summary>
        /// Leave stick in neutral position and call
        /// </summary>
        public void CalibrateNeutral() {
            this.NeutralHightime = base.Hightime;
        }

        /// <summary>
        /// Move stick to the left and call
        /// </summary>
        public void CalibrateLeft() {
            this.NegativeHightime = base.Hightime;
        }

        /// <summary>
        ///  Move stick to the right and call
        /// </summary>
        public void CalibrateRight() {
            this.PositiveHightime = base.Hightime;
        }

        /// <summary>
        /// Precalculate the deltas, they doesn't change during runtime, divides are evil ;)
        /// </summary>
        private void Precalc() {
            this._deltaNegative = _scale / (this.NeutralHightime - this.NegativeHightime - this.NeutralHysteresis);
            this._deltaPositive = _scale / (this.PositiveHightime - this.NeutralHightime + this.NeutralHysteresis);
        }

        protected override void InPortOnInterrupt(uint port, uint state, DateTime time) {
            base.InPortOnInterrupt(port, state, time);
            this.Position = (short)((this._neutralHightime < base.Hightime ? this._deltaPositive : this._deltaNegative) * (base.Hightime - this._neutralHightime));
            //Debug.Print("Pos: " + this.Position);
        }

    }
}
using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace QuadroLib.Input {
    public class AnalogChannel : PwmIn {
        private readonly double _scale = 100.0;
        // Standartwerte
        private uint _negativeHightime = 1*10000;
        private uint _neutralHightime = 15*1000;
        private uint _positiveHightime = 2*10000;

        public uint NeutralHysteresis = 2;

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="pin">Interrupt Pin</param>
        /// <param name="scale">+/- Scale zb 30°</param>
        public AnalogChannel(Cpu.Pin pin, double scale)
            : this(pin) {
            _scale = scale;
        }

        public uint NegativeHightime {
            get { return this._negativeHightime; }
            set {
                this._negativeHightime = value;
                this.Precalc();
            }
        }

        public uint NeutralHightime {
            get { return this._neutralHightime; }
            set { 
                this._neutralHightime = value;
                this.Precalc();
            }
        }

        public uint PositiveHightime {
            get { return this._positiveHightime; }
            set {
                this._positiveHightime = value;
                this.Precalc();
            }
        }
        
        /// <summary>
        /// Zum Zwischenspeichern der Deltas, Wert ändert sich wärend der Laufzeit nicht
        /// </summary>
        private void Precalc() {
            // Precalculate Deltas
            this._deltaNegative = _scale / (this.NeutralHightime - this.NegativeHightime - this.NeutralHysteresis);
            this._deltaPositive = _scale / (this.PositiveHightime - this.NeutralHightime + this.NeutralHysteresis);
        }

        protected override void InPortOnInterrupt(uint port, uint state, DateTime time) {
            base.InPortOnInterrupt(port, state, time);
            this.Position = (short)((this._neutralHightime < this.Hightime ? this._deltaPositive : this._deltaNegative) * (this.Hightime - this._neutralHightime));
            //Debug.Print("Pos: " + this.Position);
        }

    }
}
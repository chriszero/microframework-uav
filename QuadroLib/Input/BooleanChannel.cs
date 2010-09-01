using System;

using Microsoft.SPOT.Hardware;

namespace QuadroLib.Input {
    public class BooleanChannel : PwmIn {

        /// <summary>
        /// Schwellwert der False/True Erkennung
        /// </summary>
        public uint HighThreshold = 16*1000;

        /// <summary>
        /// True wenn Kanal Aktiv
        /// </summary>
        public bool State;

        public BooleanChannel(Cpu.Pin pin) :base(pin) {
            TimeSpan ts = new TimeSpan(0,0,0,0,1);
        }

        protected override void InPortOnInterrupt(uint port, uint state, DateTime time) {
            base.InPortOnInterrupt(port, state, time);
            this.State = this.HighThreshold < this.Hightime;
            //Debug.Print("" + State);
        }
    }
}
using System;

using Microsoft.SPOT.Hardware;

namespace QuadroLib.Input {
    public class BooleanChannel : PwmIn {

        /// <summary>
        /// Threshold for False/True recognition
        /// </summary>
        public long HighThreshold = 16*1000;

        /// <summary>
        /// True when channel is active
        /// </summary>
        public bool State;

        /// <summary>
        /// Switch on and call
        /// </summary>
        public void CalibrateTrue() {
            // Apply some hysteresis
            HighThreshold = base.Hightime - (base.Hightime / 100 * 15);
        }

        public BooleanChannel(Cpu.Pin pin) :base(pin) {
        }

        protected override void InPortOnInterrupt(uint port, uint state, DateTime time) {
            base.InPortOnInterrupt(port, state, time);
            this.State = this.HighThreshold < base.Hightime;
        }
    }
}
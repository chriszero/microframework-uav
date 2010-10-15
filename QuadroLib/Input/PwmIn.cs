using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace QuadroLib.Input {
    public abstract class PwmIn {
        /// <summary>
        /// Ticks
        /// </summary>
        protected long Period = 1;
        /// <summary>
        /// Ticks
        /// </summary>
        protected long Hightime = 1;

        private readonly InterruptPort _inPort;
        private DateTime _lastRisingEdge;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pin">Interrupt Pin</param>
        public PwmIn(Cpu.Pin pin) {
            this._inPort = new InterruptPort(pin,
                                        false,
                                        Port.ResistorMode.Disabled,
                                        Port.InterruptMode.InterruptEdgeBoth
                );
            this._inPort.OnInterrupt += this.InPortOnInterrupt;
        }

        protected virtual void InPortOnInterrupt(uint port, uint state, DateTime time) {
            if(state == 0) {
                    // falling edge
                    this.Hightime = time.Ticks - this._lastRisingEdge.Ticks;
            }
            else {
                    // rising edge
                    this.Period = time.Ticks - this._lastRisingEdge.Ticks;
                    this._lastRisingEdge = time;
            }
            //this._inPort.ClearInterrupt();
        }
    }
}
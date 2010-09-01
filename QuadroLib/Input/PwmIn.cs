using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace QuadroLib.Input {
    public class PwmIn {
        /// <summary>
        /// Ticks
        /// </summary>
        public long Period = 1;
        /// <summary>
        /// Ticks
        /// </summary>
        public long Hightime = 1;

        private readonly InterruptPort _inPort;
        private DateTime _steigendeFlanke;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pin">Interrupt Pin</param>
        public PwmIn(Cpu.Pin pin) {
            this._inPort = new InterruptPort(pin,
                                        false,
                                        Port.ResistorMode.PullUp,
                                        Port.InterruptMode.InterruptEdgeBoth
                );
            this._inPort.OnInterrupt += this.InPortOnInterrupt;
        }

        protected virtual void InPortOnInterrupt(uint port, uint state, DateTime time) {
            //Debug.Print("Port:" + port + " State:" + state + " Time:" + time);
            switch (state) {
                case 0:
                    // Fallende Flanke
                    this.Hightime = time.Ticks - this._steigendeFlanke.Ticks;
                    //Debug.Print("Period: " + this.Period + " High:" + this.Hightime);
                    break;
                case 1:
                    // Steigende Flanke
                    this.Period = time.Ticks - this._steigendeFlanke.Ticks;
                    this._steigendeFlanke = time;
                    break;
            }
            this._inPort.ClearInterrupt();
        }
    }
}
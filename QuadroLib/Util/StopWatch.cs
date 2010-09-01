using System;

namespace QuadroLib.Util {
    public class StopWatch {
        private DateTime _start;
        private DateTime _stop; // Verhindert Division durch Null
        private bool _isRunning;

        /// <summary>
        /// Startet die Stopuhr
        /// </summary>
        public void Start() {
            _start = DateTime.Now;
            _isRunning = true;
        }

        /// <summary>
        /// Stoppt die Stopuhr
        /// </summary>
        public void Stop() {
            _stop = DateTime.Now;
            _isRunning = false;
        }

        /// <summary>
        /// Gibt die Anbgelaufene Zeit in ms zurück und stoppt die Stopuhr
        /// </summary>
        /// <returns></returns>
        public double ElapsedMillis() {
            if (_isRunning) {
                this.Stop();
            }
            return (_stop - _start).Milliseconds;// / 10000; // Ticks zu Millis
        }
    }
}

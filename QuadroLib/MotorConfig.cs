using System;
using Microsoft.SPOT;
using QuadroLib.Ahrs;
using QuadroLib.Control;

namespace QuadroLib {
    public abstract class MotorConfig {
        /// <summary>
        /// AHRS
        /// </summary>
        protected IAhrs _ahrs;
        /// <summary>
        /// Roll Pid
        /// </summary>
        protected IPid _rollPid;
        /// <summary>
        /// Pitch Pid
        /// </summary>
        protected IPid _pitchPid;
        /// <summary>
        /// Yaw / Kurs Pid
        /// </summary>
        protected IPid _yawPid;
    }
}

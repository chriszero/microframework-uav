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
        protected IControl _rollPid;
        /// <summary>
        /// Pitch Pid
        /// </summary>
        protected IControl _pitchPid;
        /// <summary>
        /// Yaw / Kurs Pid
        /// </summary>
        protected IControl _yawPid;

        public MotorConfig(IAhrs ahrs, IControl roll, IControl pitch, IControl yaw) {
            _ahrs = ahrs;
            _rollPid = roll;
            _pitchPid = pitch;
            _yawPid = yaw;
        }
    }
}

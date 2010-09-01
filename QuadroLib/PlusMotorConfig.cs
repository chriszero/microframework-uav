using System;
using Microsoft.SPOT;
using QuadroLib.Ahrs;
using QuadroLib.Control;

namespace QuadroLib {
    public class PlusMotorConfig : MotorConfig {

        /// <summary>
        /// Erstellt eine neue Instanz
        /// </summary>
        /// <param name="ahrs">Das AHRS (Attitude Heading Reference System)</param>
        /// <param name="roll">Roll PID Regler</param>
        /// <param name="pitch">Pitch PID Regler</param>
        /// <param name="yaw">Yaw / Kurs PID Regler</param>
        public PlusMotorConfig(IAhrs ahrs, IPid roll, IPid pitch, IPid yaw) {
            _ahrs = ahrs;
            _rollPid = roll;
            _pitchPid = pitch;
            _yawPid = yaw;
        }
    }
}

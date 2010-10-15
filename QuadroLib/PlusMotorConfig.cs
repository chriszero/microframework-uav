using System;
using Microsoft.SPOT;
using QuadroLib.Ahrs;
using QuadroLib.Control;
using QuadroLib.Acuator;

namespace QuadroLib {
    public class PlusMotorConfig : MotorConfig {

        private BrushlessMotor _front;
        private BrushlessMotor _rear;
        private BrushlessMotor _left;
        private BrushlessMotor _right;

        /// <summary>
        /// Erstellt eine neue Instanz
        /// </summary>
        /// <param name="ahrs">Das AHRS (Attitude Heading Reference System)</param>
        /// <param name="roll">Roll PID Regler</param>
        /// <param name="pitch">Pitch PID Regler</param>
        /// <param name="yaw">Yaw / Kurs PID Regler</param>
        public PlusMotorConfig(IAhrs ahrs, IControl roll, IControl pitch, IControl yaw,
            BrushlessMotor front, BrushlessMotor rear,
            BrushlessMotor left, BrushlessMotor right)
            : base(ahrs, roll, pitch, yaw) {

            _front = front;
            _rear = rear;
            _left = left;
            _right = right;
        }

        public void Compute(double rollPos, double pitchPos, double yawPos) {
            double roll, pitch, yaw;
            double powerRoll, powerPitch, powerYaw;
            _ahrs.Get(out roll, out pitch, out yaw);

            _rollPid.Compute(rollPos, roll, out powerRoll);
            _pitchPid.Compute(pitchPos, pitch, out powerPitch);
            _yawPid.Compute(yawPos, yaw, out powerYaw);
        }
    }
}

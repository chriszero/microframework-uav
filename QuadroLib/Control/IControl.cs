using System;

namespace QuadroLib.Control {
    public interface IControl {
        void Compute(double setPoint, double input, out double output);
        void SetOutputLimits(double OUTMin, double OUTMax);
        void SetInputLimits(double INMin, double INMax);
        void SetSampleTime(int NewSampleTime);
    }
}

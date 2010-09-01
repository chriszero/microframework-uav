using System;

namespace QuadroLib.Control {
    public interface IPid {
        void Compute(double setPoint, double input, out double output);
        void SetTunings(double Kc, double TauI, double TauD);
        void SetOutputLimits(double OUTMin, double OUTMax);
        void SetInputLimits(double INMin, double INMax);
        void SetSampleTime(int NewSampleTime);
    }
}

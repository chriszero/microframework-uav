using System;
using Microsoft.SPOT;

namespace QuadroLib.Ahrs {
    public interface IAhrs {
        void Get(out double roll, out double pitch, out double yaw); 
        void Analogs(out double x, out double y, out double z);
        void Acc(out double x, out double y, out double z);
    }
}

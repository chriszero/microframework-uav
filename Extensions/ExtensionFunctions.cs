using System;
using System.IO.Ports;

namespace Extensions {
    public static class Extensions {

        private static byte[] tempBuffer = new byte[1];
        public static byte ReadByte(this SerialPort port) {
            port.Read(tempBuffer, 0, 1);
            return tempBuffer[0];
        }

        public static double TotalMicroseconds(this TimeSpan ts) {
            return ts.Ticks * 0.01;
        }

        public static double TotalMilliseconds(this TimeSpan ts) {
            return ts.Ticks * 0.0001;
        }

        public static double TotalSeconds(this TimeSpan ts) {
            return ts.Ticks * 1E-07;
        }

        public static float Constrain(this float x, float a, float b) {
            if(x > a && x < b) {
                return x;
            }
            if(x < a){
                return a;
            }
            return b;
        }

        public static double Constrain(this double x, double a, double b) {
            if (x > a && x < b) {
                return x;
            }
            if (x < a) {
                return a;
            }
            return b;
        }

        public static uint Constrain(this uint x, uint a, uint b) {
            if (x > a && x < b) {
                return x;
            }
            if (x < a) {
                return a;
            }
            return b;
        }

        public static int Constrain(this int x, int a, int b) {
            if (x > a && x < b) {
                return x;
            }
            if (x < a) {
                return a;
            }
            return b;
        }
    }
}

namespace System.Runtime.CompilerServices {
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ExtensionAttribute : Attribute { }

}

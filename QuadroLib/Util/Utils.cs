using System;

namespace QuadroLib.Util {
    public static class BitConverter {
        // careful with endianness!!!
        public static void ToInt16(out short val, byte[] inVal, int offset) {
            //Debug.Assert(inVal.Length == 2);
            val = (short)(inVal[0 + offset] << 0 | inVal[1 + offset] << 8);
        }
        public static void ToInt32(out int val, byte[] inVal, int offset) {
            //Debug.Assert(inVal.Length == 4);
            val = (inVal[0 + offset] << 0 | inVal[1 + offset] << 8 | inVal[2 + offset] << 16 | inVal[3 + offset] << 24);
        }
    }
}
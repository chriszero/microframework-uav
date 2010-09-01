using System;

namespace Extensions {
    /// <summary>
    /// Stellt schnelle Mothoden bereit um Ascii Zeichen zu Parsen
    /// </summary>
    public static class FastNumberParse {
        private static readonly float[] FastAtofTable = {
                                                      0.0f,
                                                      0.1f,
                                                      0.01f,
                                                      0.001f,
                                                      0.0001f,
                                                      0.00001f,
                                                      0.000001f,
                                                      0.0000001f,
                                                      0.00000001f,
                                                      0.000000001f,
                                                      0.0000000001f,
                                                      0.00000000001f,
                                                      0.000000000001f,
                                                      0.0000000000001f,
                                                      0.00000000000001f,
                                                      0.000000000000001f,
                                                      0.0000000000000001f
                                                  };

        /// <summary>
        /// Wandelt eine char Zeichenfolge in einen float wert um
        /// </summary>
        /// <param name="str">das Chararray mit den Zeichen</param>
        /// <param name="offset">offset</param>
        /// <returns></returns>
        public static float ParseFloat(char[] str, ref int offset) {
            bool negative = false;

            //int k = offset;
            if (str[offset] == '-') {
                negative = true;
                offset++;
            }

            float f = Strtol10(str, ref offset);

            if (str[offset] == '.') {
                offset++;
                int j = offset;
                float pl = Strtol10(str, ref offset);
                pl *= FastAtofTable[offset - j];

                f += pl;
            }

            if (negative) {
                return -(f);
            }
            return f;
        }

        /// <summary>
        /// Wandelt eine char Zeichenfolge in einen float wert um
        /// </summary>
        /// <param name="str">das Chararray mit den Zeichen</param>
        /// <param name="offset">offset</param>
        /// <returns></returns>
        public static Int32 ParseInt(char[] str, ref int offset) {
            bool negative = false;

            //int k = offset;
            if (str[offset] == '-') {
                negative = true;
                offset++;
            }
            int d = Strtol10(str, ref offset);

            if (negative) {
                return -(d);
            }
            return d;
        }

        private static Int32 Strtol10(char[] inp, ref int offset) {
            int value = 0;

            while (offset < inp.Length && (inp[offset] >= '0') && (inp[offset] <= '9')) {
                value = (value * 10) + (inp[offset] - '0');
                offset++;
            }
            return value;
        }
    }
}

using System;

namespace Extensions {
    public class StringBuffer {
        private readonly char[] _charBuffer;
        private int _capacity;
        private int _length;
        private int _position = 0;

        /// <summary>
        /// Initializes a new StringBuffer
        /// </summary>
        /// <param name="capacity"></param>
        public StringBuffer(int capacity) {
            _charBuffer = new char[capacity];
        }

        public int Capacity {
            get { return _charBuffer.Length; }
        }

        public int Position {
            get { return _position; }
            set {
                _position = value;
                Array.Clear(_charBuffer, _position, _charBuffer.Length - _position);
            }
        }

        public void Write(char value) {
            this._charBuffer[this._position++] = value;
            if (this._position > this._length) {
                this._length = this._position;
            }

        }

        public void Insert(string str, int pos) {
            _position = pos;
            for (int i = 0; i<str.Length;) {
                _charBuffer[_position++] = str[i++];
            }
        }

        public void Insert(char[] cArr, int offset, int count) {
            int num = this._position + count;
            if (num > this._length) {
                if (num > this._capacity) {
                    //this.EnsureCapacity(num);
                }
                this._length = num;
            }
            Array.Copy(cArr, offset, this._charBuffer, this._position, count);
            this._position = num;

        }

        public override string ToString() {
            char[] cArr = new char[this._length];
            Array.Copy(this._charBuffer, cArr, this._length);
            return new string(cArr, 0, _length);
        }
        public string ToString(int length) {
            char[] cArr = new char[length];
            for (int i = 0; i < length; i++) {
                cArr[i] = (char) _charBuffer[i];
            }
            return new string(cArr, 1, length);
        }

        public char this[int idx] {
            get { return _charBuffer[idx]; }
            set { _charBuffer[idx] = value; }
        }

        public static implicit operator char[](StringBuffer sb) {
            return sb._charBuffer;
        }
    }
}

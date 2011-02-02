using System;

namespace QuadroLib.Collections
{
    public class CircularByteBuffer
    {
        private int capacity;
        private int size;
        private int head;
        private int tail;
        private byte[] buffer;

        object sync = new object();

        public CircularByteBuffer(int capacity)
            : this(capacity, false)
        {
        }

        public CircularByteBuffer(int capacity, bool allowOverflow)
        {
            if (capacity < 0)
                throw new ArgumentException("capacity must be greater than or equal to zero.",
                    "capacity");

            this.capacity = capacity;
            size = 0;
            head = 0;
            tail = 0;
            buffer = new byte[capacity];
            AllowOverflow = allowOverflow;
        }

        public bool AllowOverflow
        {
            get;
            set;
        }

        public int Capacity
        {
            get { return capacity; }
            set
            {
                if (value == capacity)
                    return;

                if (value < size)
                    throw new ArgumentOutOfRangeException("value",
                        "value must be greater than or equal to the buffer size.");

                var dst = new byte[value];
                if (size > 0)
                    CopyTo(dst);
                buffer = dst;

                capacity = value;
            }
        }

        public int Size
        {
            get { return size; }
        }

        public void Clear()
        {
            size = 0;
            head = 0;
            tail = 0;
        }

        public int Put(byte[] src)
        {
            return Put(src, 0, src.Length);
        }

        public int Put(byte[] src, int offset, int count)
        {
            lock (sync)
            {
                int realCount = AllowOverflow ? count : Math.Min(count, capacity - size);
                /*
                int srcIndex = offset;
                for (int i = 0; i < realCount; i++, tail++, srcIndex++)
                {
                    if (tail == capacity)
                        tail = 0;
                    buffer[tail] = src[srcIndex];
                }
                */

                var freespace = capacity - tail;
                if (realCount > freespace)
                {
                    Array.Copy(src, offset, buffer, tail, freespace);
                    Array.Copy(src, offset + freespace, buffer, 0, realCount - freespace);
                    tail = realCount - freespace;
                }
                else
                {
                    Array.Copy(src, offset, buffer, tail, realCount);
                    tail += realCount;
                }

                size = Math.Min(size + realCount, capacity);
                return realCount;
            }
        }

        public void Put(byte item)
        {
            if (!AllowOverflow && size == capacity)
                throw new Exception("BufferOverflowException, Buffer is full.");

            buffer[tail] = item;
            if (tail++ == capacity)
                tail = 0;
            size++;
        }

        public void Skip(int count)
        {
            head += count;
            if (head >= capacity)
                head -= capacity;
        }

        public byte[] Get(int count)
        {
            var dst = new byte[count];
            Get(dst);
            return dst;
        }

        public int Get(byte[] dst)
        {
            return Get(dst, 0, dst.Length);
        }

        public int Get(byte[] dst, int offset, int count)
        {
            int realCount = Math.Min(count, size);
            int dstIndex = offset;
            for (int i = 0; i < realCount; i++, head++, dstIndex++)
            {
                if (head == capacity)
                    head = 0;
                dst[dstIndex] = buffer[head];
            }
            size -= realCount;
            return realCount;
        }

        public byte Get()
        {
            lock (sync)
            {
                if (size == 0)
                    throw new InvalidOperationException("Buffer is empty.");

                var item = buffer[head];
                if (++head == capacity)
                    head = 0;

                size--;
                return item;
            }
        }

        public void CopyTo(byte[] array)
        {
            CopyTo(array, 0);
        }

        public void CopyTo(byte[] array, int arrayIndex)
        {
            CopyTo(0, array, arrayIndex, size);
        }

        public void CopyTo(int index, byte[] array, int arrayIndex, int count)
        {
            if (count > size)
                throw new ArgumentOutOfRangeException("count",
                    "count cannot be greater than the buffer size.");

            int bufferIndex = head;
            for (int i = 0; i < count; i++, bufferIndex++, arrayIndex++)
            {
                if (bufferIndex == capacity)
                    bufferIndex = 0;
                array[arrayIndex] = buffer[bufferIndex];
            }
        }

        public byte[] GetBuffer()
        {
            return buffer;
        }

        public byte[] ToArray()
        {
            var dst = new byte[size];
            CopyTo(dst);
            return dst;
        }
    }
}

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Utility.Extensions
{
    public class LittleEndianBinaryReader : BinaryReader, IBinaryReader
    {
        #region Fields not directly related to properties
        /// <summary>
        /// Whether or not this reader has been disposed yet.
        /// </summary>
        bool disposed = false;
        /// <summary>
        /// Decoder to use for string conversions.
        /// </summary>
        Decoder decoder;
        /// <summary>
        /// Buffer used for temporary storage before conversion into primitives
        /// </summary>
        byte[] buffer = new byte[262144];// new byte[16];
        /// <summary>
        /// Buffer used for temporary storage when reading a single character
        /// </summary>
        char[] charBuffer = new char[1];
        /// <summary>
        /// Minimum number of bytes used to encode a character
        /// </summary>
        int minBytesPerChar;
        #endregion

        public LittleEndianBinaryReader(Stream input) : base(input) { }

        public LittleEndianBinaryReader(Stream input, Encoding encoding) : base(input, encoding) { }

        public LittleEndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }

        /// <summary>
        /// Reads exactly <paramref name="count"/> bytes into <see cref="buffer"/>, growing the buffer if needed and
        /// looping until the request is satisfied. <see cref="Stream.Read(byte[],int,int)"/> is permitted to return
        /// fewer bytes than requested, so calling it once and assuming success silently corrupts decoded values.
        /// </summary>
        /// <exception cref="EndOfStreamException">The stream ended before <paramref name="count"/> bytes were read.</exception>
        private void ReadIntoBuffer(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (buffer.Length < count) buffer = new byte[count];
            int total = 0;
            while (total < count)
            {
                int read = BaseStream.Read(buffer, total, count - total);
                if (read == 0) throw new EndOfStreamException($"Expected {count} bytes but reached end of stream after {total}.");
                total += read;
            }
        }

        public bool[] ReadBooleans(int count)
        {
            if (count == 0) return new bool[0];
            var byteCount = count;
            ReadIntoBuffer(byteCount);
            bool[] copy = new bool[count];
            Buffer.BlockCopy(buffer, 0, copy, 0, byteCount);
            return copy;
        }

        public unsafe override double ReadDouble()
        {
            ReadIntoBuffer(8);
            fixed (byte* bptr = buffer)
            {
                ulong fconv = ((ulong*)bptr)[0];
                return *((double*)&fconv);
            }
        }

        public unsafe double[] ReadDoubles(int count)
        {
            if (count == 0) return new double[0];
            var byteCount = count * 8;
            ReadIntoBuffer(byteCount);
            double[] copy = new double[count];
            Buffer.BlockCopy(buffer, 0, copy, 0, byteCount);
            return copy;
        }

        public unsafe double ReadIbmDouble()
        {
            // Not so sure this works, needs to be tested and modified
            ReadIntoBuffer(8);
            fixed (byte* bptr = buffer)
            {
                long fmant;
                long t;
                long fconv = ((long*)bptr)[0];
                //fconv = (fconv << 24) | ((fconv >> 24) & 0xff) | ((fconv & 0xff00) << 8) | ((fconv & 0xff0000) >> 8);   // reordering bytes to accomodate big endian initial encoding. (WAAAY faster than array indexing to reorder)
                if (fconv != 0)
                {
                    fmant = 0x00ffffff & fconv;
                    t = (long)((0x7f000000 & fconv) >> 22) - 130;
                    while ((fmant & 0x00800000) == 0) { --t; fmant <<= 1; }
                    if (t > 254) fconv = (long)((0x80000000 & fconv) | 0x7f7fffff);
                    else if (t <= 0) fconv = 0;
                    else fconv = unchecked((long)(0x80000000 & fconv)) | (t << 56) | (0x007fffff & fmant);
                }
                return *((double*)&fconv);
            }
        }

        public unsafe double[] ReadIbmDoubles(int count)
        {
            throw new NotImplementedException();
        }

        public unsafe float ReadIbmSingle()
        {
            ReadIntoBuffer(4);
            fixed (byte* bptr = buffer)
            {
                int fmant;
                int t;
                int fconv = ((int*)bptr)[0];
                //fconv = (fconv << 24) | ((fconv >> 24) & 0xff) | ((fconv & 0xff00) << 8) | ((fconv & 0xff0000) >> 8);   // reordering bytes to accomodate big endian initial encoding. (WAAAY faster than array indexing to reorder)
                if (fconv != 0)
                {
                    fmant = 0x00ffffff & fconv;
                    t = (int)((0x7f000000 & fconv) >> 22) - 130;
                    while ((fmant & 0x00800000) == 0) { --t; fmant <<= 1; }
                    if (t > 254) fconv = (int)((0x80000000 & fconv) | 0x7f7fffff);
                    else if (t <= 0) fconv = 0;
                    else fconv = unchecked((int)(0x80000000 & fconv)) | (t << 23) | (0x007fffff & fmant);
                }
                return *((float*)&fconv);
            }
        }

        public unsafe float[] ReadIbmSingles(int count)
        {
            if (count == 0) return new float[0];

            ReadIntoBuffer(count * 4);
            fixed (byte* bptr = buffer)
            {
                float[] copy = new float[count];
                int fmant;
                int t;
                int fconv;
                for (int i = 0; i < count; i++)
                {
                    fconv = ((int*)bptr)[i];
                    //fconv = (fconv << 24) | ((fconv >> 24) & 0xff) | ((fconv & 0xff00) << 8) | ((fconv & 0xff0000) >> 8);   // reordering bytes to accomodate big endian initial encoding. (WAAAY faster than array indexing to reorder)
                    if (fconv != 0)
                    {
                        fmant = 0x00ffffff & fconv;
                        t = (int)((0x7f000000 & fconv) >> 22) - 130;
                        while ((fmant & 0x00800000) == 0) { --t; fmant <<= 1; }
                        if (t > 254) fconv = (int)((0x80000000 & fconv) | 0x7f7fffff);
                        else if (t <= 0) fconv = 0;
                        else fconv = unchecked((int)(0x80000000 & fconv)) | (t << 23) | (0x007fffff & fmant);
                    }
                    copy[i] = *((float*)&fconv);
                }
                return copy;
            }
        }

        public unsafe override short ReadInt16()
        {
            ReadIntoBuffer(2);
            fixed (byte* bptr = buffer)
            {
                return ((short*)bptr)[0];
            }
        }

        public unsafe short[] ReadInt16s(int count)
        {
            if (count == 0) return new short[0];
            var byteCount = count * 2;
            ReadIntoBuffer(byteCount);
            short[] copy = new short[count];
            Buffer.BlockCopy(buffer, 0, copy, 0, byteCount);
            return copy;
        }

        public unsafe override int ReadInt32()
        {
            ReadIntoBuffer(4);
            fixed (byte* pBuffer = buffer)
            {
                int fconv = ((int*)pBuffer)[0];
                return fconv;
            }
        }

        public unsafe int[] ReadInt32s(int count)
        {
            if (count == 0) return new int[0];
            var byteCount = count * 4;
            ReadIntoBuffer(byteCount);
            int[] copy = new int[count];
            Buffer.BlockCopy(buffer, 0, copy, 0, byteCount);
            return copy;
        }

        public unsafe override long ReadInt64()
        {
            ReadIntoBuffer(8);
            fixed (byte* pBuffer = buffer)
            {
                return ((long*)pBuffer)[0];
            }
        }

        public unsafe long[] ReadInt64s(int count)
        {
            if (count == 0) return new long[0];
            var byteCount = count * 8;
            ReadIntoBuffer(byteCount);
            long[] copy = new long[count];
            Buffer.BlockCopy(buffer, 0, copy, 0, byteCount);
            return copy;
        }

        public unsafe sbyte[] ReadSBytes(int count)
        {
            if (count == 0) return new sbyte[0];
            var byteCount = count;
            ReadIntoBuffer(byteCount);
            sbyte[] copy = new sbyte[count];
            Buffer.BlockCopy(buffer, 0, copy, 0, byteCount);
            return copy;
        }

        public unsafe override float ReadSingle()
        {
            ReadIntoBuffer(4);
            fixed (byte* bptr = buffer)
            {
                int fconv = ((int*)bptr)[0];
                return *((float*)&fconv);
            }
        }

        public unsafe float[] ReadSingles(int count)
        {
            if (count == 0) return new float[0];
            var byteCount = count * 4;
            ReadIntoBuffer(byteCount);
            float[] copy = new float[count];
            Buffer.BlockCopy(buffer, 0, copy, 0, byteCount);
            return copy;
        }

        public unsafe override ushort ReadUInt16()
        {
            ReadIntoBuffer(2);
            fixed (byte* bptr = buffer)
            {
                return ((ushort*)bptr)[0];
            }
        }

        public unsafe ushort[] ReadUInt16s(int count)
        {
            if (count == 0) return new ushort[0];
            var byteCount = count * 2;
            ReadIntoBuffer(byteCount);
            ushort[] copy = new ushort[count];
            Buffer.BlockCopy(buffer, 0, copy, 0, byteCount);
            return copy;
        }

        public unsafe override uint ReadUInt32()
        {
            ReadIntoBuffer(4);
            fixed (byte* bptr = buffer)
            {
                return ((uint*)bptr)[0];
            }
        }

        public unsafe uint[] ReadUInt32s(int count)
        {
            if (count == 0) return new uint[0];
            var byteCount = count * 4;
            ReadIntoBuffer(byteCount);
            uint[] copy = new uint[count];
            Buffer.BlockCopy(buffer, 0, copy, 0, byteCount);
            return copy;
        }

        public unsafe override ulong ReadUInt64()
        {
            ReadIntoBuffer(8);
            fixed (byte* pBuffer = buffer)
            {
                return ((ulong*)pBuffer)[0];
            }
        }

        public unsafe ulong[] ReadUInt64s(int count)
        {
            if (count == 0) return new ulong[0];
            var byteCount = count * 8;
            ReadIntoBuffer(byteCount);
            ulong[] copy = new ulong[count];
            Buffer.BlockCopy(buffer, 0, copy, 0, byteCount);
            return copy;
        }
    }
}

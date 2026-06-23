using System.IO;

namespace Utility.Extensions
{
    public class LittleEndianBinaryWriter : BinaryWriter, IBinaryWriter
    {
        byte[] buffer = new byte[262144];// new byte[16];

        /// <summary>
        /// Ensures the scratch <see cref="buffer"/> can hold at least <paramref name="byteCount"/> bytes.
        /// The original fixed 256 KB buffer silently overran for arrays larger than 65 536 4-byte samples.
        /// </summary>
        private void EnsureBuffer(int byteCount)
        {
            if (buffer.Length < byteCount) buffer = new byte[byteCount];
        }

        public LittleEndianBinaryWriter(FileStream stream) : base(stream) { }

        public unsafe void Write(ulong[] values)
        {
            var byteCount = values.Length * 8;
            EnsureBuffer(byteCount);
            fixed(ulong* p = values)
            {
                for (int i = 0; i < byteCount; i++)
                    buffer[i] = ((byte*)p)[i];
            }
            BaseStream.Write(buffer, 0, byteCount);
        }

        public unsafe void Write(uint[] values)
        {
            var byteCount = values.Length * 4;
            EnsureBuffer(byteCount);
            fixed (uint* p = values)
            {
                for (int i = 0; i < byteCount; i++)
                    buffer[i] = ((byte*)p)[i];
            }
            BaseStream.Write(buffer, 0, byteCount);
        }

        public unsafe void Write(ushort[] values)
        {
            var byteCount = values.Length * 2;
            EnsureBuffer(byteCount);
            fixed (ushort* p = values)
            {
                for (int i = 0; i < byteCount; i++)
                    buffer[i] = ((byte*)p)[i];
            }
            BaseStream.Write(buffer, 0, byteCount);
        }

        public unsafe void Write(short[] values)
        {
            var byteCount = values.Length * 2;
            EnsureBuffer(byteCount);
            fixed (short* p = values)
            {
                for (int i = 0; i < byteCount; i++)
                    buffer[i] = ((byte*)p)[i];
            }
            BaseStream.Write(buffer, 0, byteCount);
        }

        public unsafe void Write(int[] values)
        {
            var byteCount = values.Length * 4;
            EnsureBuffer(byteCount);
            fixed (int* p = values)
            {
                for (int i = 0; i < byteCount; i++)
                    buffer[i] = ((byte*)p)[i];
            }
            BaseStream.Write(buffer, 0, byteCount);
        }

        public unsafe void Write(long[] values)
        {
            var byteCount = values.Length * 8;
            EnsureBuffer(byteCount);
            fixed (long* p = values)
            {
                for (int i = 0; i < byteCount; i++)
                    buffer[i] = ((byte*)p)[i];
            }
            BaseStream.Write(buffer, 0, byteCount);
        }

        public unsafe void Write(string[] values)
        {
            foreach (var val in values)
                base.Write(val);
        }

        public unsafe void Write(float[] values)
        {
            var byteCount = values.Length * 4;
            EnsureBuffer(byteCount);
            fixed (float* p = values)
            {
                for (int i = 0; i < byteCount; i++)
                    buffer[i] = ((byte*)p)[i];
            }
            BaseStream.Write(buffer, 0, byteCount);
        }

        public unsafe void Write(sbyte[] values)
        {
            var byteCount = values.Length;
            EnsureBuffer(byteCount);
            fixed (sbyte* p = values)
            {
                for (int i = 0; i < byteCount; i++)
                    buffer[i] = ((byte*)p)[i];
            }
            BaseStream.Write(buffer, 0, byteCount);
        }

        public unsafe void Write(double[] values)
        {
            var byteCount = values.Length * 8;
            EnsureBuffer(byteCount);
            fixed (double* p = values)
            {
                for (int i = 0; i < byteCount; i++)
                    buffer[i] = ((byte*)p)[i];
            }
            BaseStream.Write(buffer, 0, byteCount);
        }

        public unsafe void Write(decimal[] values)
        {
            var byteCount = values.Length * sizeof(decimal);
            EnsureBuffer(byteCount);
            fixed (decimal* p = values)
            {
                for (int i = 0; i < byteCount; i++)
                    buffer[i] = ((byte*)p)[i];
            }
            BaseStream.Write(buffer, 0, byteCount);
        }

        public unsafe void Write(bool[] values)
        {
            var byteCount = values.Length;
            EnsureBuffer(byteCount);
            fixed (bool* p = values)
            {
                for (int i = 0; i < byteCount; i++)
                    buffer[i] = ((byte*)p)[i];
            }
            BaseStream.Write(buffer, 0, byteCount);
        }

        public unsafe void WriteIbm(float value)
        {
            int fconv;
            int fmant;
            int i;
            int t;
            fconv = *((int*)&value);
            if (fconv != 0)
            {
                fmant = (0x007fffff & fconv) | 0x00800000;
                t = ((0x7f800000 & fconv) >> 23) - 126;
                while ((t & 0x3) != 0) { ++t; fmant >>= 1; }
                fconv = (int)(0x80000000 & fconv) | (((t >> 2) + 64) << 24) | fmant;
            }
            // fconv = (fconv << 24) | ((fconv >> 24) & 0xff) | ((fconv & 0xff00) << 8) | ((fconv & 0xff0000) >> 8);                // Endianess conversion
            var to = fconv;
            Write(to);
        }

        public unsafe void WriteIbm(float[] values)
        {
            int n = values.Length;
            EnsureBuffer(n * 4);
            int fconv;
            int fmant;
            int i;
            int t;
            fixed (float* pbuffer = values)
            {
                for (i = 0; i < n; ++i)
                {
                    int iByte = i * 4;
                    fconv = *(int*)&pbuffer[i];
                    if (fconv != 0)
                    {
                        fmant = (0x007fffff & fconv) | 0x00800000;
                        t = ((0x7f800000 & fconv) >> 23) - 126;
                        while ((t & 0x3) != 0) { ++t; fmant >>= 1; }
                        fconv = (int)(0x80000000 & fconv) | (((t >> 2) + 64) << 24) | fmant;
                    }
                    //fconv = (fconv << 24) | ((fconv >> 24) & 0xff) | ((fconv & 0xff00) << 8) | ((fconv & 0xff0000) >> 8);         // Endianess conversion
                    var bytes = (byte*)&fconv;
                    buffer[iByte + 0] = bytes[0];
                    buffer[iByte + 1] = bytes[1];
                    buffer[iByte + 2] = bytes[2];
                    buffer[iByte + 3] = bytes[3];
                }
            }
            BaseStream.Write(buffer, 0, n * 4);
        }
    }
}

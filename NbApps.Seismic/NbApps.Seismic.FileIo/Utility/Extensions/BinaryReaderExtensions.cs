using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Utility.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static unsafe float[] ReadSinglesBlockCopy(this BinaryReader reader, int count)
        {
            if (count == 0) return new float[0];
            var byteCount = count * 4;
            byte[] buffer = new byte[byteCount];
            float[] copy = new float[count];
            reader.BaseStream.Read(buffer, 0, byteCount);
            Buffer.BlockCopy(buffer, 0, copy, 0, byteCount);
            return copy;
        }

        public static unsafe double[] ReadDoublesBlockCopy(this BinaryReader reader, int count)
        {
            if (count == 0) return new double[0];
            var byteCount = count * 4;
            byte[] buffer = new byte[byteCount];
            double[] copy = new double[count];
            reader.BaseStream.Read(buffer, 0, byteCount);
            Buffer.BlockCopy(buffer, 0, copy, 0, byteCount);
            return copy;
        }

        public static unsafe float[] ReadSingles(this BinaryReader reader, int count)
        {
            var ns = count;
            if (count < 0) throw new ArgumentOutOfRangeException($"count: {count}");

            if (count == 0)
                return new float[0];

            byte[] buffer = new byte[count * 4];

            int numRead = 0;
            do
            {
                int n = reader.BaseStream.Read(buffer, numRead, count * 4);
                if (n == 0)
                    break;
                numRead += n;
                count -= n;
            } while (count > 0);

            // Trim array.  This should happen on EOF & possibly net streams.
            fixed (byte* bptr = buffer)
            {
                float[] copy = new float[numRead];

                for (int i = 0; i < ns; i++)
                {
                    int fconv = ((int*)bptr)[i];
                    copy[i] = *((float*)&fconv);
                }
                return copy;
            }
        }

        public static unsafe double[] ReadDoubles(this BinaryReader reader, int count)
        {
            var ns = count;
            if (count < 0) throw new ArgumentOutOfRangeException($"count: {count}");

            if (count == 0)
                return new double[0];

            byte[] buffer = new byte[count * 4];

            int numRead = 0;
            do
            {
                int n = reader.BaseStream.Read(buffer, numRead, count * 4);
                if (n == 0)
                    break;
                numRead += n;
                count -= n;
            } while (count > 0);

            // Trim array.  This should happen on EOF & possibly net streams.
            fixed (byte* bptr = buffer)
            {
                double[] copy = new double[numRead];

                for (int i = 0; i < ns; i++)
                {
                    int fconv = ((int*)bptr)[i];
                    copy[i] = *((double*)&fconv);
                }
                return copy;
            }
        }

        //public static unsafe float[] ReadIbmSingles(this BinaryReader reader, int count)
        //{
        //    var ns = count;
        //    if (count < 0) throw new ArgumentOutOfRangeException($"count: {count}");

        //    if (count == 0)
        //        return new float[0];

        //    byte[] buffer = new byte[count * 4];

        //    int numRead = 0;
        //    do
        //    {
        //        int n = reader.BaseStream.Read(buffer, numRead, count * 4);
        //        if (n == 0)
        //            break;
        //        numRead += n;
        //        count -= n;
        //    } while (count > 0);

        //    // Trim array.  This should happen on EOF & possibly net streams.
        //    fixed (byte* bptr = buffer)
        //    {
        //        float[] copy = new float[ns];
        //        int fmant;
        //        int t;
        //        int fconv;
        //        for (int i = 0; i < ns; i++)
        //        {
        //            fconv = ((int*)bptr)[i];
        //            //fconv = (int)(buffer[i] | buffer[i + 1] << 8 | buffer[i + 2] << 16 | buffer[i + 3] << 24);
        //            fconv = (fconv << 24) | ((fconv >> 24) & 0xff) | ((fconv & 0xff00) << 8) | ((fconv & 0xff0000) >> 8);
        //            if (fconv != 0)
        //            {
        //                fmant = 0x00ffffff & fconv;
        //                t = (int)((0x7f000000 & fconv) >> 22) - 130;
        //                while ((fmant & 0x00800000) == 0) { --t; fmant <<= 1; }
        //                if (t > 254) fconv = (int)((0x80000000 & fconv) | 0x7f7fffff);
        //                else if (t <= 0) fconv = 0;
        //                else fconv = unchecked((int)(0x80000000 & fconv)) | (t << 23) | (0x007fffff & fmant);
        //            }
        //            copy[i] = *((float*)&fconv);
        //        }
        //        return copy;
        //    }
        //}
    }

    public class BigEndianBinaryReader2 : BinaryReader
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

        public BigEndianBinaryReader2(Stream stream) : base(stream) { }

        //public override int ReadInt32()
        //{
        //    base.BaseStream.Read(buffer, 0, 4);
        //    var fconv = BitConverter.ToInt32(buffer, 0);
        //    fconv = (fconv << 24) | ((fconv >> 24) & 0xff) | ((fconv & 0xff00) << 8) | ((fconv & 0xff0000) >> 8);   // reordering bytes to accomodate big endian initial encoding. (WAAAY faster than array indexing to reorder)
        //    return fconv;
        //}

        //public override Int16 ReadInt16()
        //{
        //    var data = base.ReadBytes(2);
        //    Array.Reverse(data);
        //    return BitConverter.ToInt16(data, 0);
        //}

        //public override Int64 ReadInt64()
        //{
        //    var data = base.ReadBytes(8);
        //    BaseStream.Read(buffer, 0, 8);
        //    Array.Reverse(data);
        //    return BitConverter.ToInt64(data, 0);
        //}

        //public override UInt64 ReadUInt64()
        //{
        //    var data = base.ReadBytes(8);
        //    BaseStream.Read(buffer, 0, 8);
        //    Array.Reverse(data);
        //    return BitConverter.ToUInt64(data, 0);
        //}

        //public override UInt32 ReadUInt32()
        //{
        //    var data = base.ReadBytes(4);
        //    Array.Reverse(data);
        //    return BitConverter.ToUInt32(data, 0);
        //}

        public unsafe float[] ReadSingles(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException($"count: {count}");

            if (count == 0)
                return new float[0];

            BaseStream.Read(buffer, 0, count * 4);
            fixed (byte* bptr = buffer)
            {
                float[] copy = new float[count];
                for (int i = 0; i < count; i++)
                {
                    int fconv = ((int*)bptr)[i];
                    copy[i] = *((float*)&fconv);
                }
                return copy;
            }
        }

        /// <summary>
        /// Assumes each byte set of 4 that represents a float needs to be reordered to account for big endinan encoding.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public unsafe float[] ReadIbmSingles(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException($"count: {count}");
            if (count == 0) return new float[0];

            BaseStream.Read(buffer, 0, count * 4);
            fixed (byte* bptr = buffer)
            {
                float[] copy = new float[count];
                int fmant;
                int t;
                int fconv;
                for (int i = 0; i < count; i++)
                {
                    fconv = ((int*)bptr)[i];
                    fconv = (fconv << 24) | ((fconv >> 24) & 0xff) | ((fconv & 0xff00) << 8) | ((fconv & 0xff0000) >> 8);   // reordering bytes to accomodate big endian initial encoding. (WAAAY faster than array indexing to reorder)
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
    }
}

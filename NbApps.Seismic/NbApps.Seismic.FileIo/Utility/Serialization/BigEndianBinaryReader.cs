using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Utility.Serialization;

namespace Utility.Extensions
{
    public class BigEndianBinaryReader : BinaryReader, IBinaryReader
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

        public BigEndianBinaryReader(Stream input) : base(input) { }

        public BigEndianBinaryReader(Stream input, Encoding encoding) : base(input, encoding) { }

        public BigEndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }

        public bool[] ReadBooleans(int count)
        {
            bool[] booleans = new bool[count];
            for (int i = 0; i < count; i++)
                booleans[i] = base.ReadBoolean();
            return booleans;
        }

        public unsafe override double ReadDouble()
        {
            BaseStream.Read(buffer, 0, 8);
            fixed (byte* bptr = buffer)
            {
                ulong fconv = ((ulong*)bptr)[0];
                fconv = EndianUtilities.Swap(fconv);
                return *((double*)&fconv);
            }
        }

        public unsafe double[] ReadDoubles(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException($"count: {count}");
            if (count == 0) return new double[0];

            BaseStream.Read(buffer, 0, count * 8);
            fixed (byte* bptr = buffer)
            {
                double[] copy = new double[count];
                for (int i = 0; i < count; i++)
                {
                    ulong fconv = ((ulong*)bptr)[i];
                    fconv = EndianUtilities.Swap(fconv);
                    copy[i] = *((double*)&fconv);
                }
                return copy;
            }
        }

        public double ReadIbmDouble()
        {
            throw new NotImplementedException();
        }

        public unsafe double[] ReadIbmDoubles(int count)
        {
            throw new NotImplementedException();
        }

        public unsafe float ReadIbmSingle()
        {
            BaseStream.Read(buffer, 0, 4);
            fixed (byte* bptr = buffer)
            {
                int fmant;
                int t;
                int fconv = ((int*)bptr)[0];
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
                return *(float*)&fconv;
            }
        }

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
                    copy[i] = *(float*)&fconv;
                }
                return copy;
            }
        }

        public unsafe override short ReadInt16()
        {
            BaseStream.Read(buffer, 0, 2);
            fixed(byte* bptr = buffer)
            {
                short fconv = ((short*)bptr)[0];
                return EndianUtilities.Swap(fconv);
            }
        }

        public unsafe short[] ReadInt16s(int count)
        {
            short[] result = new short[count];
            BaseStream.Read(buffer, 0, count * 2);
            fixed(byte* pBuffer = buffer)
            {
                for (int i = 0; i < count; i++)
                {
                    ushort fconv = ((ushort*)pBuffer)[i];
                    fconv = EndianUtilities.Swap(fconv);
                    result[i] = *((short*)&fconv);
                }
                return result;
            }
        }

        public unsafe override int ReadInt32()
        {
            BaseStream.Read(buffer, 0, 4);
            fixed (byte* pBuffer = buffer)
            {
                int fconv = ((int*)pBuffer)[0];
                fconv = (fconv << 24) | ((fconv >> 24) & 0xff) | ((fconv & 0xff00) << 8) | ((fconv & 0xff0000) >> 8);   // reordering bytes to accomodate big endian initial encoding. (WAAAY faster than array indexing to reorder)
                return fconv;
            }
        }

        public unsafe int[] ReadInt32s(int count)
        {
            BaseStream.Read(buffer, 0, count * 4);
            fixed (byte* bptr = buffer)
            {
                int[] copy = new int[count];
                for (int i = 0; i < count; i++)
                {
                    int fconv = ((int*)bptr)[i];
                    copy[i] = (fconv << 24) | ((fconv >> 24) & 0xff) | ((fconv & 0xff00) << 8) | ((fconv & 0xff0000) >> 8);   // reordering bytes to accomodate big endian initial encoding. (WAAAY faster than array indexing to reorder)
                }
                return copy;
            }
        }

        public unsafe override long ReadInt64()
        {
            BaseStream.Read(buffer, 0, 8);
            fixed(byte* pBuffer = buffer)
            {
                long fconv = ((long*)pBuffer)[0];
                return EndianUtilities.Swap(fconv);
            }
        }

        public unsafe long[] ReadInt64s(int count)
        {
            long[] result = new long[count];
            BaseStream.Read(buffer, 0, count * 8);
            fixed (byte* pBuffer = buffer)
            {
                for (int i = 0; i < count; i++)
                {
                    long fconv = ((long*)pBuffer)[i];
                    result[i] = EndianUtilities.Swap(fconv);
                }
                return result;
            }
        }

        public unsafe sbyte[] ReadSBytes(int count)
        {
            sbyte[] result = new sbyte[count];
            BaseStream.Read(buffer, 0, count);
            fixed (byte* pBuffer = buffer)
            {
                for (int i = 0; i < count; i++)
                {
                    sbyte fconv = ((sbyte*)pBuffer)[i];
                    result[i] = fconv;
                }
                return result;
            }
        }

        public unsafe override float ReadSingle()
        {
            BaseStream.Read(buffer, 0, 4);
            fixed(byte* bptr = buffer)
            {
                int fconv = ((int*)bptr)[0];
                fconv = (fconv << 24) | ((fconv >> 24) & 0xff) | ((fconv & 0xff00) << 8) | ((fconv & 0xff0000) >> 8);   // reordering bytes to accomodate big endian initial encoding. (WAAAY faster than array indexing to reorder)
                return *((float*)&fconv);
            }
        }

        public unsafe float[] ReadSingles(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException($"count: {count}");
            if (count == 0) return new float[0];

            BaseStream.Read(buffer, 0, count * 4);
            fixed (byte* bptr = buffer)
            {
                float[] copy = new float[count];
                for (int i = 0; i < count; i++)
                {
                    int fconv = ((int*)bptr)[i];
                    fconv = (fconv << 24) | ((fconv >> 24) & 0xff) | ((fconv & 0xff00) << 8) | ((fconv & 0xff0000) >> 8);   // reordering bytes to accomodate big endian initial encoding. (WAAAY faster than array indexing to reorder)
                    copy[i] = *((float*)&fconv);
                }
                return copy;
            }
        }

        public unsafe override ushort ReadUInt16()
        {
            BaseStream.Read(buffer, 0, 2);
            fixed (byte* bptr = buffer)
            {
                ushort fconv = ((ushort*)bptr)[0];
                return EndianUtilities.Swap(fconv);
            }
        }

        public unsafe ushort[] ReadUInt16s(int count)
        {
            ushort[] result = new ushort[count];
            BaseStream.Read(buffer, 0, count * 2);
            fixed (byte* pBuffer = buffer)
            {
                for (int i = 0; i < count; i++)
                {
                    ushort fconv = ((ushort*)pBuffer)[i];
                    fconv = EndianUtilities.Swap(fconv);
                    result[i] = *&fconv;
                }
                return result;
            }
        }

        public unsafe override uint ReadUInt32()
        {
            BaseStream.Read(buffer, 0, 4);
            fixed (byte* bptr = buffer)
            {
                uint fconv = ((uint*)bptr)[0];
                return EndianUtilities.Swap(fconv);
            }
        }

        public unsafe uint[] ReadUInt32s(int count)
        {
            uint[] result = new uint[count];
            BaseStream.Read(buffer, 0, count * 4);
            fixed (byte* pBuffer = buffer)
            {
                for (int i = 0; i < count; i++)
                {
                    uint fconv = ((uint*)pBuffer)[i];
                    fconv = EndianUtilities.Swap(fconv);
                    result[i] = *&fconv;
                }
                return result;
            }
        }

        public unsafe override ulong ReadUInt64()
        {
            BaseStream.Read(buffer, 0, 8);
            fixed (byte* pBuffer = buffer)
            {
                var value = ((ulong*)pBuffer)[0];
                return EndianUtilities.Swap(value);
            }
        }

        public unsafe ulong[] ReadUInt64s(int count)
        {
            BaseStream.Read(buffer, 0, count * 8);
            fixed (byte* pBuffer = buffer)
            {
                ulong[] result = new ulong[count];
                for (int i = 0; i < count; i++)
                {
                    var value = ((ulong*)pBuffer)[i];
                    result[i] = EndianUtilities.Swap(value);
                    //value = (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 | (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 | (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 | (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
                }
                return result;
            }
        }

    }
    internal static class EndianUtilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Swap(ushort val)
        {
            unchecked
            {
                return (ushort)(((val & 0xFF00U) >> 8) | ((val & 0x00FFU) << 8));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Swap(short val)
        {
            unchecked
            {
                return (short)Swap((ushort)val);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Swap(uint val)
        {
            // Swap adjacent 16-bit blocks
            val = (val >> 16) | (val << 16);
            // Swap adjacent 8-bit blocks
            val = ((val & 0xFF00FF00U) >> 8) | ((val & 0x00FF00FFU) << 8);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Swap(int val)
        {
            unchecked
            {
                return (int)Swap((uint)val);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Swap(ulong val)
        {
            // Swap adjacent 32-bit blocks
            val = (val >> 32) | (val << 32);
            // Swap adjacent 16-bit blocks
            val = ((val & 0xFFFF0000FFFF0000U) >> 16) | ((val & 0x0000FFFF0000FFFFU) << 16);
            // Swap adjacent 8-bit blocks
            val = ((val & 0xFF00FF00FF00FF00U) >> 8) | ((val & 0x00FF00FF00FF00FFU) << 8);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Swap(long val)
        {
            unchecked
            {
                return (long)Swap((ulong)val);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Swap(float val)
        {
            // (Inefficient) alternatives are BitConverter.ToSingle(BitConverter.GetBytes(val).Reverse().ToArray(), 0)
            // and BitConverter.ToSingle(BitConverter.GetBytes(Swap(BitConverter.ToInt32(BitConverter.GetBytes(val), 0))), 0)

            UInt32SingleMap map = new UInt32SingleMap() { Single = val };
            map.UInt32 = Swap(map.UInt32);
            return map.Single;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Swap(double val)
        {
            // We *could* use BitConverter.Int64BitsToDouble(Swap(BitConverter.DoubleToInt64Bits(val))), but that throws if
            // system endianness isn't LittleEndian

            UInt64DoubleMap map = new UInt64DoubleMap() { Double = val };
            map.UInt64 = Swap(map.UInt64);
            return map.Double;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Swap(decimal val)
        {
            UInt128DecimalMap map = new UInt128DecimalMap() { Decimal = val };
            var bytes = map.BigInt.ToByteArray();
            Array.Reverse(bytes);
            var bigEBigInt = new BigInteger(bytes);
            map.BigInt = bigEBigInt;
            return map.Decimal;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct UInt32SingleMap
        {
            [FieldOffset(0)] public uint UInt32;
            [FieldOffset(0)] public float Single;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct UInt64DoubleMap
        {
            [FieldOffset(0)] public ulong UInt64;
            [FieldOffset(0)] public double Double;
        }
        [StructLayout(LayoutKind.Explicit)]
        private struct UInt128DecimalMap
        {
            [FieldOffset(0)] public BigInteger BigInt;
            [FieldOffset(0)] public decimal Decimal;
        }
    }
}

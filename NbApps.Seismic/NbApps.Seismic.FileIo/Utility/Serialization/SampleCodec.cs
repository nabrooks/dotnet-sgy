using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Utility.Io.Serialization
{
    /// <summary>
    /// Stateless conversions between on-disk SEG-Y sample words and <see cref="float"/> values.
    /// <para>
    /// Every method operates purely on the spans passed in — there is no shared scratch buffer and no stream state —
    /// so these helpers are safe to call concurrently from multiple threads. They replace the per-instance
    /// <c>byte[] buffer</c> that the old <c>BigEndian/LittleEndianBinaryReader</c> classes mutated in place.
    /// </para>
    /// <para>
    /// Performance: the byte span is reinterpreted as a span of 32-bit words once via <see cref="MemoryMarshal.Cast{TFrom,TTo}(Span{TFrom})"/>
    /// and sliced to the exact element count, so the JIT eliminates per-element bounds checks. Endianness is handled by
    /// <see cref="BinaryPrimitives.ReverseEndianness(int)"/>, which the JIT lowers to a single <c>BSWAP</c>/<c>REV</c>
    /// instruction — cheaper than the old shift-and-mask byte juggling and with no call overhead. The host-endianness
    /// branch is hoisted out of the loop.
    /// </para>
    /// The IBM/IEEE mantissa-and-exponent math is the same logic the previous readers and writers used.
    /// </summary>
    internal static class SampleCodec
    {
        /// <summary>
        /// Decodes big-endian IBM System/360 single-precision floats from <paramref name="src"/> into <paramref name="dst"/>.
        /// </summary>
        public static void DecodeIbm(ReadOnlySpan<byte> src, Span<float> dst)
        {
            // IBM samples are always stored big-endian, so a swap is needed iff the host is little-endian.
            ReadOnlySpan<int> words = MemoryMarshal.Cast<byte, int>(src).Slice(0, dst.Length);
            if (BitConverter.IsLittleEndian)
                for (int i = 0; i < words.Length; i++)
                    dst[i] = IbmToIeee(BinaryPrimitives.ReverseEndianness(words[i]));
            else
                for (int i = 0; i < words.Length; i++)
                    dst[i] = IbmToIeee(words[i]);
        }

        /// <summary>
        /// Decodes IEEE-754 single-precision floats from <paramref name="src"/> into <paramref name="dst"/>.
        /// </summary>
        public static void DecodeIeee(ReadOnlySpan<byte> src, Span<float> dst, bool littleEndian)
        {
            ReadOnlySpan<int> words = MemoryMarshal.Cast<byte, int>(src).Slice(0, dst.Length);
            if (littleEndian != BitConverter.IsLittleEndian)
                for (int i = 0; i < words.Length; i++)
                    dst[i] = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReverseEndianness(words[i]));
            else
                for (int i = 0; i < words.Length; i++)
                    dst[i] = BitConverter.Int32BitsToSingle(words[i]);
        }

        /// <summary>
        /// Encodes <paramref name="src"/> as big-endian IBM System/360 single-precision floats into <paramref name="dst"/>.
        /// </summary>
        public static void EncodeIbm(ReadOnlySpan<float> src, Span<byte> dst)
        {
            Span<int> words = MemoryMarshal.Cast<byte, int>(dst).Slice(0, src.Length);
            if (BitConverter.IsLittleEndian)
                for (int i = 0; i < src.Length; i++)
                    words[i] = BinaryPrimitives.ReverseEndianness(IeeeToIbm(src[i]));
            else
                for (int i = 0; i < src.Length; i++)
                    words[i] = IeeeToIbm(src[i]);
        }

        /// <summary>
        /// Encodes <paramref name="src"/> as IEEE-754 single-precision floats into <paramref name="dst"/>.
        /// </summary>
        public static void EncodeIeee(ReadOnlySpan<float> src, Span<byte> dst, bool littleEndian)
        {
            Span<int> words = MemoryMarshal.Cast<byte, int>(dst).Slice(0, src.Length);
            if (littleEndian != BitConverter.IsLittleEndian)
                for (int i = 0; i < src.Length; i++)
                    words[i] = BinaryPrimitives.ReverseEndianness(BitConverter.SingleToInt32Bits(src[i]));
            else
                for (int i = 0; i < src.Length; i++)
                    words[i] = BitConverter.SingleToInt32Bits(src[i]);
        }

        /// <summary>
        /// Converts a 32-bit IBM System/360 floating-point word (already in native byte order) to IEEE-754.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float IbmToIeee(int fconv)
        {
            if (fconv != 0)
            {
                int fmant = 0x00ffffff & fconv;
                int t = (int)((0x7f000000 & fconv) >> 22) - 130;
                while ((fmant & 0x00800000) == 0) { --t; fmant <<= 1; }
                if (t > 254) fconv = (int)((0x80000000 & fconv) | 0x7f7fffff);
                else if (t <= 0) fconv = 0;
                else fconv = unchecked((int)(0x80000000 & fconv)) | (t << 23) | (0x007fffff & fmant);
            }
            return BitConverter.Int32BitsToSingle(fconv);
        }

        /// <summary>
        /// Converts an IEEE-754 single to a 32-bit IBM System/360 floating-point word (in native byte order).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IeeeToIbm(float value)
        {
            int fconv = BitConverter.SingleToInt32Bits(value);
            if (fconv != 0)
            {
                int fmant = (0x007fffff & fconv) | 0x00800000;
                int t = ((0x7f800000 & fconv) >> 23) - 126;
                while ((t & 0x3) != 0) { ++t; fmant >>= 1; }
                fconv = (int)(0x80000000 & fconv) | (((t >> 2) + 64) << 24) | fmant;
            }
            return fconv;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Io.Serialization
{
    // The BitConverter class contains methods for
    // converting an array of bytes to one of the base data 
    // types, as well as for converting a base data type to an
    // array of bytes.
    // 
    // Only statics, does not need to be marked with the serializable attribute
    public static class ByteConverter
    {
        public static bool IsLittleEndian = false;

        unsafe public static void ToSingles(byte[] bytes, ref float[] floats)
        {
            var ns = bytes.Length / 4;
            if (floats.Length < ns) throw new ArgumentException("The length of the output array is less than that of the input byte array / 4");

            fixed (byte* pbyte = bytes)
            {
                for (int i = 0; i < ns; i++)
                {
                    var intResult = (*(pbyte + (i * 4) + 0) << 24) | (*(pbyte + (i * 4) + 1) << 16) | (*(pbyte + (i * 4) + 2) << 8) | (*(pbyte + (i * 4) + 3));
                    floats[i] = *(float*)&intResult;
                }
            }
        }

        unsafe public static float ToSingle(byte[] value, int startIndex)
        {
            int intResult;
            fixed (byte* pbyte = &value[startIndex]) intResult = (*pbyte << 24) | (*(pbyte + 1) << 16) | (*(pbyte + 2) << 8) | (*(pbyte + 3));
            return *(float*)&intResult;
        }
    }
}

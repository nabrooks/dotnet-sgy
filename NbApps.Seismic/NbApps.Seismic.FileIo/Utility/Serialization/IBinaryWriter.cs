using System;
using System.IO;

namespace Utility.Extensions
{
    public interface IBinaryWriter : IDisposable
    {
        Stream BaseStream { get; }
        void Flush();
        void Write(ulong value);
        void Write(uint value);
        void Write(ushort value);
        void Write(short value);
        void Write(int value);
        void Write(long value);
        void Write(string value);
        void Write(float value);
        void WriteIbm(float value);
        void Write(sbyte value);
        void Write(double value);
        void Write(decimal value);
        void Write(byte value);
        void Write(bool value);
        void Write(char ch);
        void Write(ulong[] value);
        void Write(uint[] value);
        void Write(ushort[] value);
        void Write(short[] value);
        void Write(int[] value);
        void Write(long[] value);
        void Write(string[] value);
        void Write(float[] value);
        void WriteIbm(float[] value);
        void Write(sbyte[] value);
        void Write(double[] value);
        void Write(decimal[] value);
        void Write(byte[] value);
        void Write(bool[] value);
        void Write(char[] ch);
    }
}

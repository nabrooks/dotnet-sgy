using Utility.Io;
using System;
using System.Reflection.Metadata.Ecma335;
using Utility.Serialization;
using System.Threading.Tasks;

namespace Utility.Io.Serialization
{
    /// <summary>
    /// An implementation of <see cref="IBinarySerializer{T}"/> where byte instances are to be 
    /// directly serialized and deserialized
    /// </summary>
    public class ByteSerializer : IBinarySerializer<byte>
    {
        /// <inheritdoc/>
        public int SizeOfT => sizeof(Byte);

        /// <inheritdoc/>
        public byte Deserialize(byte[] objElements) => objElements[0];

        /// <inheritdoc/>
        public byte Deserialize(byte[] buffer, int startIndex) => buffer[startIndex];

        /// <inheritdoc/>
        public byte[] Deserialize(byte[] buffer, int startIndex, int objectCount)
        {
            var result = new byte[objectCount * SizeOfT];
            Buffer.BlockCopy(buffer, startIndex, result, 0, result.Length);
            return result;
        }

        /// <inheritdoc/>
        public byte[] Serialize(byte obj) => new[] { obj };

        /// <inheritdoc/>
        public void Serialize(byte obj, byte[] buffer, int startIndex) => buffer[startIndex] = obj;

        /// <inheritdoc/>
        public byte[] Serialize(byte[] objects) => objects;
    }
}

using System;
using System.IO;

namespace Utility.Io.Serialization
{
    /// <summary>
    /// An implementation of <see cref="IBinarySerializer{T}"/> where byte instances are to be 
    /// directly serialized and deserialized
    /// </summary>
    public unsafe class FloatSerializer : IBinarySerializer<float>
    {
        Func<byte[], float> deserializer;
        Func<byte[], int, float> deserializerStartIndex;
        Func<byte[], int, int, float[]> deserializerStartIndexObjectCount;

        Func<float, byte[]> serializer;
        Func<float[], byte[]> serializerObjects;
        Action<float, byte[], int> serializerBufferStartIndex;

        EndianBitConverter bitConverter;

        public FloatSerializer(bool isLittleEndian = true)
        {
            EndianBitConverter lilC = new LittleEndianBitConverter();
            EndianBitConverter bigC = new BigEndianBitConverter();
            bitConverter = isLittleEndian == true ? lilC : bigC;

            LittleEndianBitConverter lilEndianConverter = new LittleEndianBitConverter();
            BigEndianBitConverter bigEndianConverter = new BigEndianBitConverter();
            
            deserializer = (bytes) => bitConverter.ToSingle(bytes, 0);
            deserializerStartIndex = (bytes, startIndex) => bitConverter.ToSingle(bytes, startIndex);
            deserializerStartIndexObjectCount = (bytes, startIndex, objectCount) =>
            {
                float[] result = new float[objectCount];
                for (int i = 0; i < objectCount; i++)
                {
                    var val = bitConverter.ToSingle(bytes, i * SizeOfT);
                    result[i] = val;
                }
                return result;
            };

            serializer = (f) => bitConverter.GetBytes(f);
            serializerObjects = (floats) => {
                byte[] result = new byte[floats.Length * SizeOfT];
                for (int i = 0; i < floats.Length; i++)
                {
                    var iResult = i * SizeOfT;
                    var bytes = bitConverter.GetBytes(floats[i]);
                    result[iResult] = bytes[0];
                    result[iResult+1] = bytes[1];
                    result[iResult+2] = bytes[2];
                    result[iResult+3] = bytes[3];
                }
                return result;
            };
            serializerBufferStartIndex = (f, buffer, startIndex) =>
            {
                var bytes = bitConverter.GetBytes(f);
                buffer[startIndex] = bytes[0];
                buffer[startIndex + 1] = bytes[1];
                buffer[startIndex + 2] = bytes[2];
                buffer[startIndex + 3] = bytes[3];
            };
        }

        /// <inheritdoc/>
        public int SizeOfT => sizeof(Single);

        public float Deserialize(byte[] objElements) => this.deserializer(objElements);

        public float Deserialize(byte[] buffer, int startIndex) => deserializerStartIndex(buffer, startIndex);

        public float[] Deserialize(byte[] buffer, int startIndex, int objectCount) => deserializerStartIndexObjectCount(buffer, startIndex, objectCount);

        public byte[] Serialize(float obj) => serializer(obj);

        public byte[] Serialize(float[] objs) => serializerObjects(objs);

        public void Serialize(float obj, byte[] buffer, int startIndex) => serializerBufferStartIndex(obj, buffer, startIndex);
    }
}

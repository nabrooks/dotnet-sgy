namespace Utility.Io.Serialization
{
    /// <summary>
    /// An interface defining the signature of an object serializer/deserializer
    /// that converts an object to a sequence of bytes and back
    /// </summary>
    /// <typeparam name="T">The type of object to serialize</typeparam>
    public interface IBinarySerializer<T>
    {
        /// <summary>
        /// The number of bytes represented by this type
        /// </summary>
        int SizeOfT { get; }

        /// <summary>
        /// Serializes an instance of the object into an array of bytes
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>An array of bytes</returns>
        byte[] Serialize(T obj);

        /// <summary>
        /// Serializes multiple instances of the object type into an array of bytes
        /// </summary>
        /// <param name="objs">Instances of the object</param>
        /// <returns>A concatenated array of bytes representing serialized objects</returns>
        byte[] Serialize(T[] objs);

        /// <summary>
        /// Serializes an instance of the object into a preexisting buffered array of bytes
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="buffer">The buffer to copy bytes into</param>
        /// <param name="startIndex">The starting index to which serialization result should be copied to</param>
        void Serialize(T obj, byte[] buffer, int startIndex);

        /// <summary>
        /// Deserializes a sequence of bytes into an instance of <see cref="T"/>
        /// </summary>
        /// <param name="objElements">The set of bytes intended to convert into an object</param>
        /// <returns>An instance of type <see cref="T"/></returns>
        T Deserialize(byte[] objElements);

        /// <summary>
        /// Deserializes a sequence of bytes into an instance of <see cref="T"/>
        /// </summary>
        /// <param name="buffer">A set of bytes from which a subset is intended to convert into an object</param>
        /// <param name="startIndex">The array offset from which an instance of type <see cref="T"/> should be created</param>
        /// <returns>An instance of type <see cref="T"/></returns>
        T Deserialize(byte[] buffer, int startIndex);

        /// <summary>
        /// Deserializes a sequence of bytes into a set of intances of <see cref="T"/>
        /// </summary>
        /// <param name="buffer">The byte buffer from which data should be deserialized</param>
        /// <param name="startIndex">The index from which to start deserialization</param>
        /// <returns>A set of objects</returns>
        T[] Deserialize(byte[] buffer, int startIndex, int objectCount);
    }
}

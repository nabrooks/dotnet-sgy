using System;
using System.Runtime.InteropServices;

public static class RectangularArray
{
    public static T Create<T>(params int[] arrayDimensions)
    {
        return (T)CreateJaggedArray(typeof(T).GetElementType(), 0, arrayDimensions);
    }

    private static object CreateJaggedArray(Type type, int index, int[] lengths)
    {
        Array array = Array.CreateInstance(type, lengths[index]);
        Type elementType = type.GetElementType();

        if (elementType != null)
        {
            for (int i = 0; i < lengths[index]; i++)
            {
                array.SetValue(CreateJaggedArray(elementType, index + 1, lengths), i);
            }
        }

        return array;
    }

    public static T[] GetRow<T>(this T[,] array, int row)
    {
        if (!typeof(T).IsPrimitive)
            throw new InvalidOperationException("Not supported for managed types.");

        if (array == null)
            throw new ArgumentNullException("array");

        int cols = array.GetUpperBound(1) + 1;
        T[] result = new T[cols];

        int size;

        if (typeof(T) == typeof(bool))
            size = 1;
        else if (typeof(T) == typeof(char))
            size = 2;
        else
            size = Marshal.SizeOf<T>();

        Buffer.BlockCopy(array, row * cols * size, result, 0, cols * size);

        return result;
    }
}


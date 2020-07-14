using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace Utility.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Combines two arrays and returns a new array containing both values
        /// </summary>
        /// <typeparam name="TArrayType">Type of the data in the array</typeparam>
        /// <param name="array1">Array 1</param>
        /// <param name="additions">Arrays to concat onto the first item</param>
        /// <returns>A new array containing both arrays' values</returns>
        /// <example>
        /// <code>
        ///  int[] TestObject1 = new int[] { 1, 2, 3 };
        ///  int[] TestObject2 = new int[] { 4, 5, 6 };
        ///  int[] TestObject3 = new int[] { 7, 8, 9 };
        ///  TestObject1 = TestObject1.Combine(TestObject2, TestObject3);
        /// </code>
        /// </example>
        public static TArrayType[] Concat<TArrayType>(this TArrayType[] array1, params TArrayType[][] additions)
        {
            CodeContract.Requires<ArgumentNullException>(array1 != null, "Array1");
            CodeContract.Requires<ArgumentNullException>(additions != null, "Additions");
            CodeContract.Requires<ArgumentNullException>(Contract.ForAll(additions, x => x != null), "Additions");
            TArrayType[] Result = new TArrayType[array1.Length + additions.Sum(x => x.Length)];
            int Offset = array1.Length;
            Array.Copy(array1, 0, Result, 0, array1.Length);
            for (int x = 0; x < additions.Length; ++x)
            {
                Array.Copy(additions[x], 0, Result, Offset, additions[x].Length);
                Offset += additions[x].Length;
            }
            return Result;
        }

        /// <summary>
        /// Fills elements of a rectangular array at the given position and size to a specific value.
        /// Ranges given will fill in as many elements as possible, ignoring positions outside the bounds of the array.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="array">The source array.</param>
        /// <param name="value">Value to fill with.</param>
        /// <param name="row">Row to start on (inclusive, zero-index).</param>
        /// <param name="col">Column to start on (inclusive, zero-index).</param>
        /// <param name="width">Positive width of area to fill.</param>
        /// <param name="height">Positive height of area to fill.</param>
        public static void Fill<T>(this T[,] array, T value, int row, int col, int width, int height)
        {
            for (int r = row; r < row + height; r++)
            {
                for (int c = col; c < col + width; c++)
                {
                    if (r >= 0 && c >= 0 && r < array.GetLength(0) && c < array.GetLength(1))
                    {
                        array[r, c] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Yields a row from a rectangular array.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="rectarray">The source array.</param>
        /// <param name="row">Row record to retrieve, 0-based index.</param>
        /// <returns>Yielded row.</returns>
        public static IEnumerable<T> GetRow<T>(this T[,] rectarray, int row)
        {
            if (row < 0 || row >= rectarray.GetLength(0))
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            for (int c = 0; c < rectarray.GetLength(1); c++)
            {
                yield return rectarray[row, c];
            }
        }

        public static int GetRowCount<T>(this T[,] rectarray)
        {
            return rectarray.GetLength(0);
        }

        public static int GetColumnCount<T>(this T[,] rectarray)
        {
            return rectarray.GetLength(1);
        }

        public static int GetRowCount<T>(this T[][] rectarray)
        {
            return rectarray.Length;
        }

        public static int GetMaxColumnCount<T>(this T[][] rectarray)
        {
            int cols = rectarray.GetUpperBound(1) + 1;

            var rowCount = rectarray.Length;
            int max = int.MinValue;
            int min = int.MaxValue;
            for (int ri = 0; ri < rowCount; ri++)
            {
                var length = GetColumn(rectarray, rowCount).Count();
                if (length > max) max = length;
                if (length < min) min = length;
            }
            return max;
        }

        public static int GetMinColumnCount<T>(this T[][] rectarray)
        {
            int cols = rectarray.GetUpperBound(1) + 1;

            var rowCount = rectarray.Length;
            int min = int.MaxValue;
            for (int ri = 0; ri < rowCount; ri++)
            {
                var length = GetColumn(rectarray, rowCount).Count();
                if (length < min) min = length;
            }
            return min;
        }

        /// <summary>
        /// Yields a column from a rectangular array.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="rectarray">The source array.</param>
        /// <param name="column">Column record to retrieve, 0-based index.</param>
        /// <returns>Yielded column.</returns>
        public static IEnumerable<T> GetColumn<T>(this T[,] rectarray, int column)
        {
            if (column < 0 || column >= rectarray.GetLength(1))
            {
                throw new ArgumentOutOfRangeException(nameof(column));
            }

            for (int r = 0; r < rectarray.GetLength(0); r++)
            {
                yield return rectarray[r, column];
            }
        }

        /// <summary>
        /// Yields a row from a rectangular array.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="rectarray">The source array.</param>
        /// <param name="row">Row record to retrieve, 0-based index.</param>
        /// <returns>Yielded row.</returns>
        public static IEnumerable<T> GetRow<T>(this T[][] rectarray, int row)
        {
            if (!typeof(T).IsPrimitive)
                throw new InvalidOperationException("Not supported for managed types.");

            if (rectarray == null)
                throw new ArgumentNullException("array");

            int cols = rectarray.GetUpperBound(1) + 1;
            T[] result = new T[cols];

            int size;

            if (typeof(T) == typeof(bool))
                size = 1;
            else if (typeof(T) == typeof(char))
                size = 2;
            else
                size = Marshal.SizeOf<T>();

            Buffer.BlockCopy(rectarray, row * cols * size, result, 0, cols * size);

            return result;
        }

        /// <summary>
        /// Yields a column from a jagged array.
        /// An exception will be thrown if the column is out of bounds, and return default in places where there are no elements from inner arrays.
        /// Note: There is no equivalent GetRow method, as you can use array[row] to retrieve.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="rectarray">The source array.</param>
        /// <param name="column">Column record to retrieve, 0-based index.</param>
        /// <returns>Yielded enumerable of column elements for given column, and default values for smaller inner arrays.</returns>
        public static IEnumerable<T> GetColumn<T>(this T[][] rectarray, int column)
        {
            //if (column < 0 || column >= rectarray.Max(array => array.Length))
            //{
            //    throw new ArgumentOutOfRangeException(nameof(column));
            //}
            int ni = rectarray.Length;
            int nj = rectarray[0].Length;
            T[] result = new T[ni];
            for (int i = 0; i < ni; i++)
            {
                yield return rectarray[i][column];
            }

            //for (int r = 0; r < rectarray.GetLength(0); r++)
            //{
            //    if (column >= rectarray[r].Length)
            //    {
            //        yield return default(T);

            //        continue;
            //    }

            //    yield return rectarray[r][column];
            //}
        }

        /// <summary>
        /// Returns a simple string representation of an array.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="array">The source array.</param>
        /// <returns>String representation of the array.</returns>
        public static string ToArrayString<T>(this T[] array)
        {
            return "[" + string.Join(",\t", array) + "]";
        }

        /// <summary>
        /// Returns a simple string representation of a jagged array.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="mdarray">The source array.</param>
        /// <returns>String representation of the array.</returns>
        public static string ToArrayString<T>(this T[][] mdarray)
        {
            string[] inner = new string[mdarray.GetLength(0)];

            for (int r = 0; r < mdarray.GetLength(0); r++)
            {
                inner[r] = string.Join(",\t", mdarray[r]);
            }

            return "[[" + string.Join("]," + Environment.NewLine + " [", inner) + "]]";
        }

        /// <summary>
        /// Returns a simple string representation of a rectangular array.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="rectarray">The source array.</param>
        /// <returns>String representation of the array.</returns>
        public static string ToArrayString<T>(this T[,] rectarray)
        {
            string[] inner = new string[rectarray.GetLength(0)];

            for (int r = 0; r < rectarray.GetLength(0); r++)
            {
                inner[r] = string.Join(",\t", rectarray.GetRow(r));
            }

            return "[[" + string.Join("]," + Environment.NewLine + " [", inner) + "]]";
        }

        #region Float Array Extensions

        /// <summary>
        /// Calculates a percentile from an array of values
        /// </summary>
        /// <param name="sequence">The array of values</param>
        /// <param name="percentile">The percentile (must be between 0 and 1) to calculate</param>
        /// <returns>The percentile</returns>
        public static float Percentile(this float[] sequence, float percentile)
        {
            Array.Sort(sequence);
            int N = sequence.Length;
            float n = (N - 1) * percentile + 1;
            // Another method: double n = (N + 1) * percentile;
            if (n == 1d) return sequence[0];
            else if (n == N) return sequence[N - 1];
            else
            {
                int k = (int)n;
                float d = n - k;
                return sequence[k - 1] + d * (sequence[k] - sequence[k - 1]);
            }
        }

        /// <summary>
        /// Calculates a percentile from an array of values
        /// </summary>
        /// <param name="sequence">The array of values</param>
        /// <param name="percentile">The percentile (must be between 0 and 1) to calculate</param>
        /// <returns>The percentile</returns>
        public static double Percentile(this double[] sequence, float percentile)
        {
            Array.Sort(sequence);
            int N = sequence.Length;
            double n = (N - 1) * percentile + 1;
            // Another method: double n = (N + 1) * percentile;
            if (n == 1d) return sequence[0];
            else if (n == N) return sequence[N - 1];
            else
            {
                int k = (int)n;
                double d = n - k;
                return sequence[k - 1] + d * (sequence[k] - sequence[k - 1]);
            }
        }

        /// <summary>
        /// Assumes Rectangular array
        /// </summary>
        /// <param name="sequences"></param>
        /// <param name="percentile"></param>
        /// <returns></returns>
        public static float[] Percentile(this float[][] sequences, float percentile)
        {
            var nj = sequences[0].Length;
            var result = new float[nj];

            for (int j = 0; j < nj; j++)
            {
                var col = sequences.GetColumn(j).ToArray();
                var p = col.Percentile(percentile);
                result[j] = p;
            }
            return result;
        }

        /// <summary>
        /// Evaluates if a data array is normalized from 0 to 1 if minmax is null, otherwise to min and max
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsNormalized(this float[] data, (float, float)? minmax = null)
        {
            if (minmax == null)
                return data.Min() == 0 && data.Max() == 1;
            else
                return data.Min() == minmax.Value.Item1 && data.Max() == minmax.Value.Item2;
        }

        /// <summary>
        /// Clips sample values of a data array to min and max values
        /// Note: also sets the input data to clipped values for performance reasons.
        /// </summary>
        /// <param name="data">The data to clip</param>
        /// <param name="minmax">The min and max bounds to clip to</param>
        /// <returns>A clipped array to min and max bounds</returns>
        public static float[] Clip(this float[] data, (float, float)? minmax = null)
        {
            if (minmax == null)
                return data;
            var min = minmax.Value.Item1;
            var max = minmax.Value.Item2;
            Parallel.For(0, data.Length, i =>
            {
                if (data[i] < min)
                    data[i] = min;
                else if (data[i] > max)
                    data[i] = max;
            });
            return data;
        }

        /// <summary>
        /// Normalizes an array of data to minmax bounds.  If bounds are null, 
        /// then theyre set to the min and max of the array sample values.
        /// Note: also sets the input data to normalized values for performance reasons.
        /// </summary>
        /// <param name="data">The data to normalize</param>
        /// <param name="minmax">The min and max values to normalize data to </param>
        /// <returns>an array of normalized data</returns>
        public static float[] Normalize(this float[] data, (float, float)? minmax = null)
        {
            float dataMax = data.Max();
            float dataMin = data.Min();
            float range = dataMax - dataMin;

            if (minmax == null)
            {
                Parallel.For(0, data.Length, i =>
                {
                    data[i] = (data[i] - dataMin) / range;
                });
            }
            else
            {
                var min = minmax.Value.Item1;
                var max = minmax.Value.Item2;
                Parallel.For(0, data.Length, i =>
                {
                    data[i] = (data[i] - dataMin) / range;
                    data[i] = (1 - data[i]) * min + data[i] * max;
                });
            }
            return data;
        }
        #endregion Float Array Extensions
    }
}


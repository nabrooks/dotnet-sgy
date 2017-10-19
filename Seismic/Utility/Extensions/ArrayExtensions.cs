using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Seismic.SegyFileIo
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
    }


}

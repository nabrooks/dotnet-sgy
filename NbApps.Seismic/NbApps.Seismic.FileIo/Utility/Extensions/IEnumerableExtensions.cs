using NbApps.Seismic.FileIo.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Extensions
{
    public static class IEnumerableExtensions
    {
        #region ToArray

        /// <summary>
        /// Converts an enumerable to a <see cref="BigArray{T}"/> by enumerating over the enumerable.
        /// </summary>
        /// <typeparam name="T">The type of array to conver</typeparam>
        /// <param name="enumerable">The enumerable to conver</param>
        /// <returns>A <see cref="BigArray{T}"/> of elements populated from the enumerable argument</returns>
        public static BigArray<T> ToBigArray<T>(this IEnumerable<T> enumerable)
        {
            CodeContract.Requires<ArgumentNullException>(enumerable != null, "Enumerable is null.");

            var retval = enumerable as BigArray<T>;
            if (retval != null) return retval;

            BigArray<T> bigArr = new BigArray<T>(enumerable.Count());

            int bigArrIndex = 0;
            foreach (T element in enumerable)
            {
                bigArr[bigArrIndex] = element;
                bigArrIndex++;
            }
            return bigArr;
        }

        #endregion
    }
}

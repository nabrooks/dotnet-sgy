using System;
using System.Collections.Generic;

namespace Utility.Extensions
{
    public static class IListExtensions
    {
        public static int BinarySearch<T>(this IList<T> list, T value)
        {
            if (list == null) throw new ArgumentException("ilist cannot be null");
            if (list.Count <= 1) throw new ArgumentException("ilist must have more than one element");

            var comp = Comparer<T>.Default;
            int lo = 0, hi = list.Count - 1;
            while (lo < hi)
            {
                int m = (hi + lo) / 2;  // this might overflow; be careful.
                if (comp.Compare(list[m], value) < 0) lo = m + 1;
                else hi = m - 1;
            }
            if (comp.Compare(list[lo], value) < 0) lo++;
            return lo;
        }

        public static void AddRange<T>(this IList<T> list, IEnumerable<T> objects)
        {
            foreach (var obj in objects) list.Add(obj);
        }

        /// <summary>
        /// Gets the median from the list
        /// </summary>
        /// <typeparam name="T">The data type of the list</typeparam>
        /// <param name="Values">The list of values</param>
        /// <returns>The median value</returns>
        public static T Median<T>(List<T> list)
        {
            if (list.Count == 0)
                throw new Exception("There are no elements in 'list'");

            list.Sort();
            return list[(list.Count / 2)];
        }
    }
}

using System;
using System.Collections.Generic;

namespace Utility.Extensions
{
    public static class SortedListExtensions
    {
        public static int FindFirstIndexGreaterThanOrEqualTo<T, U>(this SortedList<T, U> sortedList, T key)
        {
            return sortedList.Keys.BinarySearch(key);
        }
    }


}

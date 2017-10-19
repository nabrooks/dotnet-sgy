using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.DataTypes;

namespace Utility.Extensions
{
    public static class IEnumerableExtensions
    {
        #region ForEach

        /// <summary>
        /// Does an action for each item in the IEnumerable
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="List">IEnumerable to iterate over</param>
        /// <param name="Action">Action to do</param>
        /// <returns>The original list</returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> List, Action<T> Action)
        {
            CodeContract.Requires<ArgumentNullException>(List != null, "List");
            CodeContract.Requires<ArgumentNullException>(Action != null, "Action");
            foreach (T Item in List)
                Action(Item);
            return List;
        }

        /// <summary>
        /// Does a function for each item in the IEnumerable, returning a list of the results
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <typeparam name="R">Return type</typeparam>
        /// <param name="List">IEnumerable to iterate over</param>
        /// <param name="Function">Function to do</param>
        /// <returns>The resulting list</returns>
        public static IEnumerable<R> ForEach<T, R>(this IEnumerable<T> List, Func<T, R> Function)
        {
            CodeContract.Requires<ArgumentNullException>(List != null, "List");
            CodeContract.Requires<ArgumentNullException>(Function != null, "Function");
            List<R> ReturnValues = new List<R>();
            foreach (T Item in List)
                ReturnValues.Add(Function(Item));
            return ReturnValues;
        }

        /// <summary>
        /// Does an action for each item in the IEnumerable
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="List">IEnumerable to iterate over</param>
        /// <param name="Action">Action to do</param>
        /// <param name="CatchAction">Action that occurs if an exception occurs</param>
        /// <returns>The original list</returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> List, Action<T> Action, Action<T, Exception> CatchAction)
        {
            CodeContract.Requires<ArgumentNullException>(List != null, "List");
            CodeContract.Requires<ArgumentNullException>(Action != null, "Action");
            CodeContract.Requires<ArgumentNullException>(CatchAction != null, "CatchAction");
            foreach (T Item in List)
            {
                try
                {
                    Action(Item);
                }
                catch (Exception e) { CatchAction(Item, e); }
            }
            return List;
        }

        /// <summary>
        /// Does a function for each item in the IEnumerable, returning a list of the results
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <typeparam name="R">Return type</typeparam>
        /// <param name="List">IEnumerable to iterate over</param>
        /// <param name="Function">Function to do</param>
        /// <param name="CatchAction">Action that occurs if an exception occurs</param>
        /// <returns>The resulting list</returns>
        public static IEnumerable<R> ForEach<T, R>(this IEnumerable<T> List, Func<T, R> Function, Action<T, Exception> CatchAction)
        {
            CodeContract.Requires<ArgumentNullException>(List != null, "List");
            CodeContract.Requires<ArgumentNullException>(Function != null, "Function");
            CodeContract.Requires<ArgumentNullException>(CatchAction != null, "CatchAction");
            List<R> ReturnValues = new List<R>();
            foreach (T Item in List)
            {
                try
                {
                    ReturnValues.Add(Function(Item));
                }
                catch (Exception e) { CatchAction(Item, e); }
            }
            return ReturnValues;
        }

        #endregion

        #region ToArray

        /// <summary>
        /// Converts a list to an array
        /// </summary>
        /// <typeparam name="Source">Source type</typeparam>
        /// <typeparam name="Target">Target type</typeparam>
        /// <param name="List">List to convert</param>
        /// <param name="ConvertingFunction">Function used to convert each item</param>
        /// <returns>The array containing the items from the list</returns>
        public static Target[] ToArray<Source, Target>(this IEnumerable<Source> List, Func<Source, Target> ConvertingFunction)
        {
            CodeContract.Requires<ArgumentNullException>(List != null, "List");
            CodeContract.Requires<ArgumentNullException>(ConvertingFunction != null, "ConvertingFunction");
            return List.ForEach(ConvertingFunction).ToArray();
        }

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

        #region ToList

        /// <summary>
        /// Converts an IEnumerable to a list
        /// </summary>
        /// <typeparam name="Source">Source type</typeparam>
        /// <typeparam name="Target">Target type</typeparam>
        /// <param name="List">IEnumerable to convert</param>
        /// <param name="ConvertingFunction">Function used to convert each item</param>
        /// <returns>The list containing the items from the IEnumerable</returns>
        public static List<Target> ToList<Source, Target>(this IEnumerable<Source> List, Func<Source, Target> ConvertingFunction)
        {
            CodeContract.Requires<ArgumentNullException>(List != null, "List");
            CodeContract.Requires<ArgumentNullException>(ConvertingFunction != null, "ConvertingFunction");
            return List.ForEach(ConvertingFunction).ToList();
        }

        /// <summary>
        /// Converts an enumerable to a <see cref="BigList{T}"/> by enumerating over the enumerable.
        /// </summary>
        /// <typeparam name="T">The type of array to conver</typeparam>
        /// <param name="enumerable">The enumerable to conver</param>
        /// <returns>A <see cref="BigList{T}"/> of elements populated from the enumerable argument</returns>
        public static BigList<T> ToBigList<T>(this IEnumerable<T> enumerable)
        {
            CodeContract.Requires<ArgumentNullException>(enumerable != null, "Enumerable is null.");

            var retval = enumerable as BigList<T>;
            if (retval != null) return retval;

            BigList<T> bigLst = new BigList<T>(enumerable.Count());

            int bigArrIndex = 0;
            foreach (T element in enumerable)
            {
                bigLst[bigArrIndex] = element;
                bigArrIndex++;
            }
            return bigLst;
        }

        #endregion
    }
}

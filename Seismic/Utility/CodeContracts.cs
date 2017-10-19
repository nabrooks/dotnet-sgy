using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    /// <summary>
    /// CodeContract class for evaluating pre and post conditions,
    /// I wanted to avoid all the heavy lifting involved in MS's
    /// code CodeContracts
    /// </summary>
    public static class CodeContract
    {
        [DebuggerNonUserCode]
        [Conditional("DEBUG")]
        public static void Requires<TException>(bool predicate, string message)
            where TException : Exception, new()
        {
            if (!predicate)
            {
                Debug.WriteLine(message);
                throw new TException();
            }
        }

        [DebuggerNonUserCode]
        [Conditional("DEBUG")]
        public static void Requires(bool condition, string message)
        {
            if (condition)
            {
                return;
            }

            throw new Exception(message);
        }

        [DebuggerNonUserCode]
        [Conditional("DEBUG")]
        public static void Requires(bool condition)
        {
            Requires(condition, "A requirement condition has failed");
        }

        [DebuggerNonUserCode]
        [Conditional("DEBUG")]
        public static void Assume(bool condition, string message)
        {
            if (condition)
            {
                return;
            }

            throw new Exception(message);
        }

        [DebuggerNonUserCode]
        [Conditional("DEBUG")]
        public static void Assume(bool condition)
        {
            Assume(condition, "An assumption condition has failed");
        }

        [DebuggerNonUserCode]
        [Conditional("DEBUG")]
        public static void Ensure<TException>(bool predicate, string message)
            where TException : Exception, new()
        {
            if (!predicate)
            {
                Debug.WriteLine(message);
                throw new TException();
            }
        }

        [DebuggerNonUserCode]
        [Conditional("DEBUG")]
        public static void Ensure(bool condition, string message)
        {
            if (condition)
            {
                return;
            }
            throw new Exception(message);
        }

        [DebuggerNonUserCode]
        [Conditional("DEBUG")]
        public static void Ensure(bool condition)
        {
            Ensure(condition, "An assertion has failed");
        }

        /// <summary>Adds an assumption that your object is not null. This method will inline to nothing in a release application,
        /// so is not appropriate for checking user input!</summary>
        /// <param name="obj">The object</param>
        /// <param name="assumptionReason">The reason for the assumption, preferably detailing programmer reasoning.</param>
        /// <returns>The object, now lovingly CodeContracted with a CodeContract.Assume(obj != null)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "assumptionReason", Justification = "This value is only used in debug builds")]
        [DebuggerNonUserCode]
        public static T CodeContractAssumeNotNull<T>(this T obj, string assumptionReason) where T : class
        {
            Requires(!String.IsNullOrEmpty(assumptionReason));

            return obj;
        }

        /// <summary>Adds an assumption that your object is neither null nor empty. This method will inline to nothing in a release application
        /// so is not appropriate for checking user input!</summary>
        /// <param name="s">The string</param>
        /// <param name="assumptionReason">The reason for the assumption, preferably detailing programmer reasoning.</param>
        /// <returns>The string, now lovingly CodeContracted with a CodeContract.Assume(!string.IsNullOrEmpty(s))</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "assumptionReason", Justification = "This value is only used in debug builds")]
        [DebuggerNonUserCode]
        public static string CodeContractAssumeNotNullOrEmpty(this string s, string assumptionReason)
        {
            Requires(!String.IsNullOrEmpty(assumptionReason));

            return s;
        }

        /// <summary>Adds an assumption that your string has an exact length. This method will inline to nothing in a release application
        /// so is not appropriate for checking user input!</summary>
        /// <param name="s">The string</param>
        /// <param name="length">The intended length of the string</param>
        /// <param name="assumptionReason">The reason for the assumption, preferably detailing programmer reasoning.</param>
        /// <returns>The string, now lovingly CodeContracted with a CodeContract.Assume(string.Length == length)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "assumptionReason", Justification = "This value is only used in debug builds")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "length", Justification = "This value is only used in debug builds")]
        [DebuggerNonUserCode]
        public static string CodeContractAssumeLengthIs(this string s, int length, string assumptionReason)
        {
            Requires(!String.IsNullOrEmpty(assumptionReason));

            return s;
        }

        /// <summary>Adds an assumption that your array has a minimum length. This method will inline to nothing in a release application
        /// so is not appropriate for checking user input!</summary>
        /// <param name="array">The array for CodeContracting</param>
        /// <param name="minimumLength">The minimum length expected for the array</param>
        /// <param name="assumptionReason">The reason for the assumption, preferably detailing programmer reasoning.</param>
        /// <returns>The array, now lovingly CodeContracted with a CodeContract.Assume(array.Length >= minimumLength)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "assumptionReason", Justification = "This value is only used in debug builds")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "minimumLength", Justification = "This value is only used in debug builds")]
        [DebuggerNonUserCode]
        public static T[] CodeContractAssumeLengthIsAtLeast<T>(this T[] array, int minimumLength, string assumptionReason)
        {
            Requires(array != null);
            Requires(!String.IsNullOrEmpty(assumptionReason));
            Requires(minimumLength >= 0);

            return array;
        }

        /// <summary>Adds an assumption that your array has a minimum length. This method will inline to nothing in a release application
        /// so is not appropriate for checking user input!</summary>
        /// <param name="array">The array for CodeContracting</param>
        /// <param name="length">The minimum length expected for the array</param>
        /// <param name="assumptionReason">The reason for the assumption, preferably detailing programmer reasoning.</param>
        /// <returns>The array, now lovingly CodeContracted with a CodeContract.Assume(array.Length == minimumLength)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "assumptionReason", Justification = "This value is only used in debug builds")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            MessageId = "length", Justification = "This value is only used in debug builds")]
        [DebuggerNonUserCode]
        public static T[] CodeContractAssumeLengthIs<T>(this T[] array, int length, string assumptionReason)
        {
            Requires(array != null);
            Requires(!String.IsNullOrEmpty(assumptionReason));
            Requires(length >= 0);

            return array;
        }
    }
}

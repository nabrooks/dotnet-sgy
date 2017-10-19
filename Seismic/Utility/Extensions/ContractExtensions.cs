using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Extensions
{
    /// <summary>
    /// Methods which make contracts easier to work with.
    /// </summary>
    public static class ContractExtensions
    {
        /// <summary>Adds an assumption that your object is not null. This method will inline to nothing in a release application,
        /// so is not appropriate for checking user input!</summary>
        /// <param name="obj">The object</param>
        /// <param name="assumptionReason">The reason for the assumption, preferably detailing programmer reasoning.</param>
        /// <returns>The object, now lovingly contracted with a Contract.Assume(obj != null)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "assumptionReason", Justification = "This value is only used in debug builds")]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T ContractAssumeNotNull<T>(this T obj, string assumptionReason) where T : class
        {
            CodeContract.Requires(!String.IsNullOrEmpty(assumptionReason));

            return obj;
        }

        /// <summary>Adds an assumption that your object is neither null nor empty. This method will inline to nothing in a release application
        /// so is not appropriate for checking user input!</summary>
        /// <param name="s">The string</param>
        /// <param name="assumptionReason">The reason for the assumption, preferably detailing programmer reasoning.</param>
        /// <returns>The string, now lovingly contracted with a Contract.Assume(!string.IsNullOrEmpty(s))</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "assumptionReason", Justification = "This value is only used in debug builds")]
        [System.Diagnostics.DebuggerNonUserCode]
        public static string ContractAssumeNotNullOrEmpty(this string s, string assumptionReason)
        {
            CodeContract.Requires(!String.IsNullOrEmpty(assumptionReason));

            return s;
        }

        /// <summary>Adds an assumption that your string has an exact length. This method will inline to nothing in a release application
        /// so is not appropriate for checking user input!</summary>
        /// <param name="s">The string</param>
        /// <param name="length">The intended length of the string</param>
        /// <param name="assumptionReason">The reason for the assumption, preferably detailing programmer reasoning.</param>
        /// <returns>The string, now lovingly contracted with a Contract.Assume(string.Length == length)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "assumptionReason", Justification = "This value is only used in debug builds")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length", Justification = "This value is only used in debug builds")]
        [System.Diagnostics.DebuggerNonUserCode]
        public static string ContractAssumeLengthIs(this string s, int length, string assumptionReason)
        {
            CodeContract.Requires(!String.IsNullOrEmpty(assumptionReason));

            return s;
        }

        /// <summary>Adds an assumption that your array has a minimum length. This method will inline to nothing in a release application
        /// so is not appropriate for checking user input!</summary>
        /// <param name="array">The array for contracting</param>
        /// <param name="minimumLength">The minimum length expected for the array</param>
        /// <param name="assumptionReason">The reason for the assumption, preferably detailing programmer reasoning.</param>
        /// <returns>The array, now lovingly contracted with a Contract.Assume(array.Length >= minimumLength)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "assumptionReason", Justification = "This value is only used in debug builds")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "minimumLength", Justification = "This value is only used in debug builds")]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T[] ContractAssumeLengthIsAtLeast<T>(this T[] array, int minimumLength, string assumptionReason)
        {
            CodeContract.Requires(array != null);
            CodeContract.Requires(!String.IsNullOrEmpty(assumptionReason));
            CodeContract.Requires(minimumLength >= 0);

            return array;
        }

        /// <summary>Adds an assumption that your array has a minimum length. This method will inline to nothing in a release application
        /// so is not appropriate for checking user input!</summary>
        /// <param name="array">The array for contracting</param>
        /// <param name="length">The minimum length expected for the array</param>
        /// <param name="assumptionReason">The reason for the assumption, preferably detailing programmer reasoning.</param>
        /// <returns>The array, now lovingly contracted with a Contract.Assume(array.Length == minimumLength)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "assumptionReason", Justification = "This value is only used in debug builds")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "length", Justification = "This value is only used in debug builds")]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T[] ContractAssumeLengthIs<T>(this T[] array, int length, string assumptionReason)
        {
            CodeContract.Requires(array != null);
            CodeContract.Requires(!String.IsNullOrEmpty(assumptionReason));
            CodeContract.Requires(length >= 0);

            return array;
        }
    }
}


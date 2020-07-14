using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Utility.Extensions
{
    public static class StringExtensions
    { /// <summary>
      /// Converts from base: 0 - 62
      /// </summary>
      /// <param name="source">The source.</param>
      /// <param name="from">From.</param>
      /// <param name="to">To.</param>
      /// <returns></returns>
        public static string BaseConvert(this string source, int from, int to)
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var result = "";
            var length = source.Length;
            var number = new int[length];

            for (var i = 0; i < length; i++)
            {
                number[i] = chars.IndexOf(source[i]);
            }

            int newlen;

            do
            {
                var divide = 0;
                newlen = 0;

                for (var i = 0; i < length; i++)
                {
                    divide = divide * @from + number[i];

                    if (divide >= to)
                    {
                        number[newlen++] = divide / to;
                        divide = divide % to;
                    }
                    else if (newlen > 0)
                    {
                        number[newlen++] = 0;
                    }
                }

                length = newlen;
                result = chars[divide] + result;
            }
            while (newlen != 0);

            return result;
        }

        public static string EncodeXml(this string value)
        {
            return value.Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;");
        }

        public static string EncodeJson(this string value)
        {
            return String.Concat
                ("\"",
                    value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n"),
                    "\""
                );
        }

        public static string UrlEncode(this string text, bool upperCase = false)
        {
            if (String.IsNullOrEmpty(text)) return text;

            var sb = new StringBuilder();
            var fmt = upperCase ? "X2" : "x2";

            foreach (var charCode in Encoding.UTF8.GetBytes(text))
            {

                if (
                    charCode >= 65 && charCode <= 90        // A-Z
                    || charCode >= 97 && charCode <= 122    // a-z
                    || charCode >= 48 && charCode <= 57     // 0-9
                    || charCode >= 44 && charCode <= 46     // ,-.
                    )
                {
                    sb.Append((char)charCode);
                }
                else if (charCode == 32)
                {
                    sb.Append('+');
                }
                else
                {
                    sb.Append('%' + charCode.ToString(fmt));
                }
            }

            return sb.ToString();
        }

        public static string UrlDecode(this string text)
        {
            if (String.IsNullOrEmpty(text)) return null;

            var bytes = new List<byte>();

            var textLength = text.Length;
            for (var i = 0; i < textLength; i++)
            {
                var c = text[i];
                if (c == '+')
                {
                    bytes.Add(32);
                }
                else if (c == '%')
                {
                    var hexNo = Convert.ToByte(text.Substring(i + 1, 2), 16);
                    bytes.Add(hexNo);
                    i += 2;
                }
                else
                {
                    bytes.Add((byte)c);
                }
            }

            byte[] byteArray = bytes.ToArray();
            return Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
        }

        public static string HexUnescape(this string text, params char[] anyCharOf)
        {
            if (String.IsNullOrEmpty(text)) return null;
            if (anyCharOf == null || anyCharOf.Length == 0) return text;

            var sb = new StringBuilder();

            var textLength = text.Length;
            for (var i = 0; i < textLength; i++)
            {
                var c = text.Substring(i, 1);
                if (c == "%")
                {
                    var hexNo = Convert.ToInt32(text.Substring(i + 1, 2), 16);
                    sb.Append((char)hexNo);
                    i += 2;
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static string UrlFormat(this string url, params string[] urlComponents)
        {
            var encodedUrlComponents = new string[urlComponents.Length];
            for (var i = 0; i < urlComponents.Length; i++)
            {
                var x = urlComponents[i];
                encodedUrlComponents[i] = x.UrlEncode();
            }

            return String.Format(url, encodedUrlComponents);
        }

        public static string ToRot13(this string value)
        {
            var array = value.ToCharArray();
            for (var i = 0; i < array.Length; i++)
            {
                var number = (int)array[i];

                if (number >= 'a' && number <= 'z')
                    number += (number > 'm') ? -13 : 13;

                else if (number >= 'A' && number <= 'Z')
                    number += (number > 'M') ? -13 : 13;

                array[i] = (char)number;
            }
            return new string(array);
        }

        public static string WithTrailingSlash(this string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (path[path.Length - 1] != '/')
            {
                return path + "/";
            }
            return path;
        }

        public static string AppendPath(this string uri, params string[] uriComponents)
        {
            return AppendUrlPaths(uri, uriComponents);
        }

        public static string AppendUrlPaths(this string uri, params string[] uriComponents)
        {
            var sb = new StringBuilder(uri.WithTrailingSlash());
            var i = 0;
            foreach (var uriComponent in uriComponents)
            {
                if (i++ > 0) sb.Append('/');
                sb.Append(uriComponent.UrlEncode());
            }
            return sb.ToString();
        }

        public static string AppendUrlPathsRaw(this string uri, params string[] uriComponents)
        {
            var sb = new StringBuilder(uri.WithTrailingSlash());
            var i = 0;
            foreach (var uriComponent in uriComponents)
            {
                if (i++ > 0) sb.Append('/');
                sb.Append(uriComponent);
            }
            return sb.ToString();
        }

        public static string FromUtf8Bytes(this byte[] bytes)
        {
            return bytes == null ? null
                : Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public static byte[] ToUtf8Bytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static byte[] ToUtf8Bytes(this int intVal)
        {
            return FastToUtf8Bytes(intVal.ToString());
        }

        public static byte[] ToUtf8Bytes(this long longVal)
        {
            return FastToUtf8Bytes(longVal.ToString());
        }

        public static byte[] ToUtf8Bytes(this ulong ulongVal)
        {
            return FastToUtf8Bytes(ulongVal.ToString());
        }

        public static byte[] ToUtf8Bytes(this double doubleVal)
        {
            var doubleStr = doubleVal.ToString(CultureInfo.InvariantCulture.NumberFormat);

            if (doubleStr.IndexOf('E') != -1 || doubleStr.IndexOf('e') != -1)
                doubleStr = DoubleConverter.ToExactString(doubleVal);

            return FastToUtf8Bytes(doubleStr);
        }

        /// <summary>
        /// Skip the encoding process for 'safe strings' 
        /// </summary>
        /// <param name="strVal"></param>
        /// <returns></returns>
        private static byte[] FastToUtf8Bytes(string strVal)
        {
            var bytes = new byte[strVal.Length];
            for (var i = 0; i < strVal.Length; i++)
                bytes[i] = (byte)strVal[i];

            return bytes;
        }

        public static string[] SplitOnFirst(this string strVal, char needle)
        {
            if (strVal == null) return new string[0];
            var pos = strVal.IndexOf(needle);
            return pos == -1
                ? new[] { strVal }
                : new[] { strVal.Substring(0, pos), strVal.Substring(pos + 1) };
        }

        public static string[] SplitOnFirst(this string strVal, string needle)
        {
            if (strVal == null) return new string[0];
            var pos = strVal.IndexOf(needle);
            return pos == -1
                ? new[] { strVal }
                : new[] { strVal.Substring(0, pos), strVal.Substring(pos + 1) };
        }

        public static string[] SplitOnLast(this string strVal, char needle)
        {
            if (strVal == null) return new string[0];
            var pos = strVal.LastIndexOf(needle);
            return pos == -1
                ? new[] { strVal }
                : new[] { strVal.Substring(0, pos), strVal.Substring(pos + 1) };
        }

        public static string[] SplitOnLast(this string strVal, string needle)
        {
            if (strVal == null) return new string[0];
            var pos = strVal.LastIndexOf(needle);
            return pos == -1
                ? new[] { strVal }
                : new[] { strVal.Substring(0, pos), strVal.Substring(pos + 1) };
        }

        public static string WithoutExtension(this string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            var extPos = filePath.LastIndexOf('.');
            if (extPos == -1) return filePath;

            var dirPos = filePath.LastIndexOfAny(DirSeps);
            return extPos > dirPos ? filePath.Substring(0, extPos) : filePath;
        }

        public static string GetExtension(this string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            var extPos = filePath.LastIndexOf('.');
            return extPos == -1 ? string.Empty : filePath.Substring(extPos);
        }

        public static bool HasExtension(this string filePath)
        {
            return !filePath.GetExtension().IsNullOrEmpty();
        }

        static readonly char[] DirSeps = new[] { '\\', '/' };

        public static string FormatWith(this string text, params object[] args)
        {
            return String.Format(text, args);
        }

        public static string Fmt(this string text, params object[] args)
        {
            return String.Format(text, args);
        }

        public static int IndexOfAny(this string text, params string[] needles)
        {
            return IndexOfAny(text, 0, needles);
        }

        public static int IndexOfAny(this string text, int startIndex, params string[] needles)
        {
            var firstPos = -1;
            if (text != null)
            {
                foreach (var needle in needles)
                {
                    var pos = text.IndexOf(needle, startIndex);
                    if ((pos >= 0) && (firstPos == -1 || pos < firstPos))
                        firstPos = pos;
                }
            }

            return firstPos;
        }

        public static string ExtractContents(this string fromText, string startAfter, string endAt)
        {
            return ExtractContents(fromText, startAfter, startAfter, endAt);
        }

        public static string ExtractContents(this string fromText, string uniqueMarker, string startAfter, string endAt)
        {
            if (String.IsNullOrEmpty(uniqueMarker))
                throw new ArgumentNullException("uniqueMarker");
            if (String.IsNullOrEmpty(startAfter))
                throw new ArgumentNullException("startAfter");
            if (String.IsNullOrEmpty(endAt))
                throw new ArgumentNullException("endAt");

            if (String.IsNullOrEmpty(fromText)) return null;

            var markerPos = fromText.IndexOf(uniqueMarker);
            if (markerPos == -1) return null;

            var startPos = fromText.IndexOf(startAfter, markerPos);
            if (startPos == -1) return null;
            startPos += startAfter.Length;

            var endPos = fromText.IndexOf(endAt, startPos);
            if (endPos == -1) endPos = fromText.Length;

            return fromText.Substring(startPos, endPos - startPos);
        }

        private const int LowerCaseOffset = 'a' - 'A';

        public static string ToCamelCase(this string value)
        {
            if (String.IsNullOrEmpty(value)) return value;

            var len = value.Length;
            var newValue = new char[len];
            var firstPart = true;

            for (var i = 0; i < len; ++i)
            {
                var c0 = value[i];
                var c1 = i < len - 1 ? value[i + 1] : 'A';
                var c0isUpper = c0 >= 'A' && c0 <= 'Z';
                var c1isUpper = c1 >= 'A' && c1 <= 'Z';

                if (firstPart && c0isUpper && (c1isUpper || i == 0))
                    c0 = (char)(c0 + LowerCaseOffset);
                else
                    firstPart = false;

                newValue[i] = c0;
            }

            return new string(newValue);
        }

        public static string ToLowercaseUnderscore(this string value)
        {
            if (String.IsNullOrEmpty(value)) return value;
            value = value.ToCamelCase();

            var sb = new StringBuilder(value.Length);
            foreach (char t in value)
            {
                if (Char.IsDigit(t) || (Char.IsLetter(t) && Char.IsLower(t)) || t == '_')
                {
                    sb.Append(t);
                }
                else
                {
                    sb.Append("_");
                    sb.Append(Char.ToLowerInvariant(t));
                }
            }
            return sb.ToString();
        }

        public static string SafeSubstring(this string value, int startIndex)
        {
            return SafeSubstring(value, startIndex, value.Length);
        }

        public static string SafeSubstring(this string value, int startIndex, int length)
        {
            if (String.IsNullOrEmpty(value)) return String.Empty;
            if (value.Length >= (startIndex + length))
                return value.Substring(startIndex, length);

            return value.Length > startIndex ? value.Substring(startIndex) : String.Empty;
        }

        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static T ToEnumOrDefault<T>(this string value, T defaultValue)
        {
            if (String.IsNullOrEmpty(value)) return defaultValue;
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static bool IsEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        public static bool IsExistingFile(this string path)
        {
            return File.Exists(path);
        }

        public static bool IsExistingDirectory(this string path)
        {
            return Directory.Exists(path);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        public static bool EqualsIgnoreCase(this string value, string other)
        {
            return String.Equals(value, other, StringComparison.CurrentCultureIgnoreCase);
        }

        public static string ReplaceFirst(this string haystack, string needle, string replacement)
        {
            var pos = haystack.IndexOf(needle);
            if (pos < 0) return haystack;

            return haystack.Substring(0, pos) + replacement + haystack.Substring(pos + needle.Length);
        }

        public static string ReplaceAll(this string haystack, string needle, string replacement)
        {
            int pos;
            // Avoid a possible infinite loop
            if (needle == replacement) return haystack;
            while ((pos = haystack.IndexOf(needle)) > 0)
            {
                haystack = haystack.Substring(0, pos)
                           + replacement
                           + haystack.Substring(pos + needle.Length);
            }
            return haystack;
        }

        public static bool ContainsAny(this string text, params string[] testMatches)
        {
            foreach (var testMatch in testMatches)
            {
                if (text.Contains(testMatch)) return true;
            }
            return false;
        }

        public static string Join(this List<string> items, string delimeter)
        {
            return String.Join(delimeter, items.ToArray());
        }

        public static string ToParentPath(this string path)
        {
            var pos = path.LastIndexOf('/');
            if (pos == -1) return "/";

            var parentPath = path.Substring(0, pos);
            return parentPath;
        }

        public static string RemoveCharFlags(this string text, bool[] charFlags)
        {
            if (text == null) return null;

            var copy = text.ToCharArray();
            var nonWsPos = 0;

            for (var i = 0; i < text.Length; i++)
            {
                var @char = text[i];
                if (@char < charFlags.Length && charFlags[@char]) continue;
                copy[nonWsPos++] = @char;
            }

            return new String(copy, 0, nonWsPos);
        }

        public static string ToNullIfEmpty(this string text)
        {
            return String.IsNullOrEmpty(text) ? null : text;
        }

        private static char[] SystemTypeChars = new[] { '<', '>', '+' };

        public static bool IsInt(this string text)
        {
            if (String.IsNullOrEmpty(text)) return false;
            int ret;
            return Int32.TryParse(text, out ret);
        }

        public static int ToInt(this string text)
        {
            return text == null ? default(int) : Int32.Parse(text);
        }

        public static int ToInt(this string text, int defaultValue)
        {
            int ret;
            return Int32.TryParse(text, out ret) ? ret : defaultValue;
        }

        public static long ToInt64(this string text)
        {
            return Int64.Parse(text);
        }

        public static long ToInt64(this string text, long defaultValue)
        {
            long ret;
            return Int64.TryParse(text, out ret) ? ret : defaultValue;
        }

        public static float ToFloat(this string text)
        {
            return text == null ? default(float) : float.Parse(text);
        }

        public static float ToFloat(this string text, float defaultValue)
        {
            float ret;
            return float.TryParse(text, out ret) ? ret : defaultValue;
        }

        public static double ToDouble(this string text)
        {
            return text == null ? default(double) : double.Parse(text);
        }

        public static double ToDouble(this string text, double defaultValue)
        {
            double ret;
            return double.TryParse(text, out ret) ? ret : defaultValue;
        }

        public static decimal ToDecimal(this string text)
        {
            return text == null ? default(decimal) : decimal.Parse(text);
        }

        public static decimal ToDecimal(this string text, decimal defaultValue)
        {
            decimal ret;
            return decimal.TryParse(text, out ret) ? ret : defaultValue;
        }

        public static bool Matches(this string value, string pattern)
        {
            return value.Glob(pattern);
        }

        public static bool Glob(this string value, string pattern)
        {
            int pos;
            for (pos = 0; pattern.Length != pos; pos++)
            {
                switch (pattern[pos])
                {
                    case '?':
                        break;

                    case '*':
                        for (int i = value.Length; i >= pos; i--)
                        {
                            if (Glob(value.Substring(i), pattern.Substring(pos + 1)))
                                return true;
                        }
                        return false;

                    default:
                        if (value.Length == pos || Char.ToUpper(pattern[pos]) != Char.ToUpper(value[pos]))
                        {
                            return false;
                        }
                        break;
                }
            }

            return value.Length == pos;
        }

        public static string TrimPrefixes(this string fromString, params string[] prefixes)
        {
            if (string.IsNullOrEmpty(fromString))
                return fromString;

            foreach (var prefix in prefixes)
            {
                if (fromString.StartsWith(prefix))
                    return fromString.Substring(prefix.Length);
            }

            return fromString;
        }

        public static Dictionary<string, string> ParseKeyValueText(this string text, string delimiter = " ")
        {
            var to = new Dictionary<string, string>();
            if (text == null) return to;

            foreach (var parts in text.ReadLines().Select(line => SplitOnFirst(line, delimiter)))
            {
                var key = parts[0].Trim();
                if (key.Length == 0 || key.StartsWith("#")) continue;
                to[key] = parts.Length == 2 ? parts[1].Trim() : null;
            }

            return to;
        }

        public static IEnumerable<string> ReadLines(this string text)
        {
            string line;
            var reader = new StringReader(text ?? "");
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        public static int CountOccurrencesOf(this string text, char needle)
        {
            var chars = text.ToCharArray();
            var count = 0;
            var length = chars.Length;
            for (var n = length - 1; n >= 0; n--)
            {
                if (chars[n] == needle)
                    count++;
            }
            return count;
        }
        public static string TrimLastCharacter(this String str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return str;
            }
            else
            {
                return str.TrimEnd(str[str.Length - 1]);
            }
        }
    }


}

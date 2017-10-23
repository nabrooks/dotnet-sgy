using System;

namespace Utility.Io.Encodings
{
    /// <summary>
    /// Exception thrown if the embedded resource describing the
    /// EBCDIC encodings is missing or invalid.
    /// 
    /// Code was obtained from http://jonskeet.uk/csharp/ebcdic/
    /// </summary>
    internal class InvalidEbcdicDataException : Exception
    {
        internal InvalidEbcdicDataException(string reason) : base(reason)
        {
        }
    }
}

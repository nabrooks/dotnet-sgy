using System;

namespace Seismic.SegyFileIo.Exceptions
{
    public class SegyEndianessException : Exception
    {
        public SegyEndianessException(string message) : base(message) { }
    }
}

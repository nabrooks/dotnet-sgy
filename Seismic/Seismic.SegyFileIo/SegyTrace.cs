using System;
using System.Linq;
using Utility;
using Utility.Serialization;

namespace Seismic.SegyFileIo
{
    /// <summary>
    /// Describes a trace as read from a Segy file.
    /// </summary>
    public class SegyTrace : IEquatable<SegyTrace>
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public SegyTrace()
        {
            Header = new SegyTraceHeader();
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="traceHeader">The trace header to use for this trace</param>
        /// <param name="data">Trace data for this trace</param>
        /// <param name="componentAxis">The cartesian coordinate direction in which this trace exists</param>
        public SegyTrace(SegyTraceHeader traceHeader, float[] data, int componentAxis = 0)
        {
            Header = traceHeader;
            Data = data;
            ComponentAxis = componentAxis;

            CodeContract.Assume(data.Length <= ushort.MaxValue,"The length of the data array for this trace must not exceed ushort.MaxValue due to type definitions of the sample count property in the trace header.");

            Header.SampleCount = (ushort)data.Length;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sampleCount">The number of samples in this trace</param>
        public SegyTrace(ushort sampleCount) : this()
        {
            Header.SampleCount = sampleCount;
            Data = new float[sampleCount];
        }

        /// <summary>
        /// The trace header describing properties about the trace
        /// </summary>
        public SegyTraceHeader Header { get; set; }

        /// <summary>
        /// Returns an integer value corresponding to the axis on which this trace is defined (0 = X, 1 = Y, 2 = Z).
        /// </summary>
        public int ComponentAxis { get; set; } = 0;

        /// <summary>
        /// Sample values for this trace
        /// </summary>
        public float[] Data { get; set; }

        /// <summary>
        /// Serializes the trace into a byte array.
        /// </summary>
        /// <returns>an array of bytes representing both the trace header and data</returns>
        public byte[] GetBytes()
        {
            byte[] headerBytes = Header.GetBytes();
            byte[] traceBytes = new byte[headerBytes.Length + Data.Length * sizeof(float)];

            float[] traceDataCpy = new float[Data.Length];
            byte[] traceDataBytes = new byte[Data.Length * sizeof(float)];

            // copy header bytes to output byte array
            Buffer.BlockCopy(headerBytes, 0, traceBytes, 0, headerBytes.Length);

            // copy data float array to copy array
            Buffer.BlockCopy(Data, 0, traceDataCpy, 0, traceDataCpy.Length * sizeof(float));

            IbmConverter.float_to_ibm(traceDataCpy, traceDataCpy, traceDataCpy.Length);

            Buffer.BlockCopy(traceDataCpy, 0, traceDataBytes, 0, traceDataBytes.Length);

            for (int i = 0; i < traceDataCpy.Length; i++)
            {
                //var ibmBytes = IbmConverter.GetBytes(Data[i]);
                for (int j = 0; j < 4; j++) traceBytes[240 + i * 4 + j] = traceDataBytes[i * 4 + j];
            }
            return traceBytes;
        }

        /// <summary>
        /// Comparison method
        /// </summary>
        /// <param name="other">The Segy trace to compare with</param>
        /// <returns>True if all trace header properties and sample values are the same as this trace, else returns false.</returns>
        public bool Equals(SegyTrace other)
        {
            if (ComponentAxis != other?.ComponentAxis) return false;
            if (Data != null && other.Data == null) return false;
            if (Data == null && other.Data != null) return false;
            if (!Header.Equals(other.Header) || Data.Length != other.Data.Length) return false;

            return !Data.Where((t, i) => t != other.Data[i]).Any();
        }
    }
}

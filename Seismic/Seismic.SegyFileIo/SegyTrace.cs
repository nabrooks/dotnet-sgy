using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Serialization;

namespace Seismic.SegyFileIo
{
    public class SegyTrace : IEquatable<SegyTrace>
    {
        public SegyTrace()
        {
            Header = new SegyTraceHeader();
        }

        public SegyTrace(SegyTraceHeader trHeader, float[] data, int componentAxis = 0)
        {
            Header = trHeader;
            Data = data;
            ComponentAxis = componentAxis;
        }

        public SegyTrace(ushort sampleCount) : this()
        {
            Header.SampleCount = sampleCount;
            Data = new float[sampleCount];
        }

        public SegyTraceHeader Header { get; set; }

        /// <summary>
        /// Returns an integer value corresponding to the axis on which this trace is defined (0 = X, 1 = Y, 2 = Z).
        /// </summary>
        public int ComponentAxis { get; set; } = 0;

        public float[] Data { get; set; }

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

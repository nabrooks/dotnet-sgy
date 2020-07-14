namespace Hess.Seismic.SegyFileIo
{
    /// <summary>
    /// Represents a seismic trace
    /// </summary>
    public class Trace
    {
        /// <summary>
        /// ctor
        /// </summary>
        public Trace(TraceHeader header, float[] data)
        {
            Header = header;
            Data = data;
        }

        /// <summary>
        /// The binary trace header of this trace
        /// </summary>
        public TraceHeader Header ;

        /// <summary>
        /// The sample data of this trace
        /// </summary>
        public float[] Data ;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Utility;
using Utility.DataTypes;
using Utility.Extensions;
using Utility.Io.Encodings;

namespace Seismic.SegyFileIo
{
    /// <summary>
    /// Opens a file stream for writing data and file headers to a Society of Exploration Geophysicists "Y" (Segy) file format.
    /// </summary>
    public class SegyWriter : Disposable
    {
        private readonly string _filepath;
        private readonly FileStream _stream;
        private static readonly Encoding _textHeaderEncoding = EbcdicEncoding.GetEncoding("EBCDIC-US");
        private readonly BinaryWriter _writer;

        #region Constant values

        public const int TextHeaderSize = 3200;
        public const int BinaryHeaderSize = 400;
        public const int TraceHeaderSize = 240;
        public const int SampleFormatIndex = 24;
        public const int SampleCountIndex = 114;

        #endregion Constant values

        /// <summary>
        /// Ctor    
        /// </summary>
        /// <param name="filepath">The path of the file.</param>
        public SegyWriter(string filepath)
        {
            CodeContract.Requires<NullReferenceException>(!string.IsNullOrEmpty(filepath), "Filepath is null.");

            _filepath = filepath;
            _stream = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 2 << 18);
            _writer = new BinaryWriter(_stream);

            if (_stream.Length == 0)
                // if file is new Pad stream for text header and binary header lengths
                _stream.SetLength(TextHeaderSize + BinaryHeaderSize);
        }

        /// <summary>
        /// Other Ctor
        /// </summary>
        /// <param name="fileinfo">The FileInfo of the file intended to be written</param>
        public SegyWriter(FileInfo fileinfo) : this(fileinfo.FullName)
        {
            CodeContract.Assume(fileinfo != null, "The file info used to create a new segy file must not be null.");
        }

        /// <summary>
        /// The number of traces currently written to the file
        /// </summary>
        public long TraceCount { get; private set; } = 0;

        /// <summary>
        /// The number of samples per trace in the file.
        /// </summary>
        public int TraceSampleCount { get; private set; } = 0;

        /// <summary>
        /// Writes the first textual file header in "EBCDIC" encoding.
        /// </summary>
        /// <param name="textualFileHeader">The string to write.</param>
        public void Write(string textualFileHeader)
        {
            CodeContract.Requires<NullReferenceException>(textualFileHeader != null,"The file textual header string must not be null if intended to be written.");

            // pad or remove characters from string if it is not the correct size.
            string textheader = string.Empty;
            if (textualFileHeader.Length < TextHeaderSize)
                textheader = textualFileHeader.PadRight(TextHeaderSize);
            else if (textualFileHeader.Length > TextHeaderSize)
                textheader = textualFileHeader.Remove(TextHeaderSize, textualFileHeader.Length - TextHeaderSize);

            // encode en ebcdic and write to file.
            _writer.Write(_textHeaderEncoding.GetBytes(textualFileHeader));
        }

        /// <summary>
        /// Writes the binary file header.
        /// </summary>
        /// <param name="binaryHeader">The binary file header to write.</param>
        public void Write(SegyFileHeader binaryHeader)
        {
            // write binary file header
            binaryHeader.DataSampleFormatCode = (short)FormatCode.IbmFloatingPoint4;
            _writer.BaseStream.Position = TextHeaderSize;
            _writer.Write(binaryHeader.GetBytes());
        }

        /// <summary>
        /// Appends a single segy trace to the end of the file or trace series.
        /// </summary>
        /// <param name="sgyTrace">The trace to write.</param>
        public void Write(SegyTrace sgyTrace)
        {
            CodeContract.Requires(sgyTrace.Data.Length != 0);

            if (TraceCount == 0) TraceSampleCount = sgyTrace.Data.Length;

            CodeContract.Assume(sgyTrace.Data.Length == TraceSampleCount);

            _writer.BaseStream.Position = _writer.BaseStream.Length;
            _writer.Write(sgyTrace.GetBytes());
            TraceCount++;
        }

        /// <summary>
        /// Appends a set of traces to the end of the file or trace series.
        /// </summary>
        /// <param name="traces">Traces to write</param>
        /// <param name="progress">A progress handler</param>
        /// <param name="ct">Cancellation token</param>
        public void Write(IEnumerable<SegyTrace> traces, IProgress<int> progress = null, CancellationToken ct = default(CancellationToken))
        {
            CodeContract.Requires<NullReferenceException>(traces != null, "Traces cannot be null.");

            if (ct.IsCancellationRequested) return;

            BigArray<SegyTrace> segyTraces = traces as BigArray<SegyTrace> ?? traces.ToBigArray();

            CodeContract.Assume(segyTraces.Any(), "There must be at least one trace to write.");

            // get "traces" statistics.
            var distinctTraceSampleCounts = traces.Select(tr => tr.Data.Length).Distinct();
            int numTraceLengths = distinctTraceSampleCounts.Count();

            // assume number of trace lengths is 1.
            CodeContract.Assume(numTraceLengths == 1, "There are traces to write with inconsistent lengths.  All traces must have the same length");
            _stream.Seek(0, SeekOrigin.End);

            if (TraceSampleCount == 0) TraceSampleCount = distinctTraceSampleCounts.FirstOrDefault();
            else CodeContract.Assume(TraceSampleCount == distinctTraceSampleCounts.FirstOrDefault(), "Trace lengths to write is not consistent with the rest of the trace lengths in this file.");

            var currProgress = 0;
            long traceCount = segyTraces.LongCount();
            long ctr = 0;
            foreach (var sgyTrace in traces)
            {
                _writer.Write(sgyTrace.GetBytes());
                ctr++;

                // report progress and cancel if requested
                if (ct.IsCancellationRequested) break;
                if (progress == null) continue;
                var progPercent = (int)(100 * (double)ctr / traceCount);
                if (currProgress == progPercent) continue;

                progress?.Report(progPercent);
                currProgress++;
            }
        }

        /// <summary>
        /// Closes the file stream
        /// </summary>
        public void Close()
        {
            _stream.Close();
        }

        #region Disposable members

        /// <summary>
        /// Disposes of the file stream
        /// </summary>
        protected override void DisposeManagedResources()
        {
            _stream.Dispose();
        }

        #endregion Disposable Members

        #region Utility

        /// <summary>
        /// Gets the byte position of the trace in the current filestream
        /// </summary>
        /// <param name="traceIndex">The trace index to get the stream byte position for.</param>
        /// <returns>An integral value representing the byte position in the stream of the corresponding trace</returns>
        private long GetStreamIndex(long traceIndex)
        {
            return TextHeaderSize + BinaryHeaderSize + (traceIndex * (TraceHeaderSize + (sizeof(float) * TraceSampleCount)));
        }

        /// <summary>
        /// Gets the available amount of ram on this machine.
        /// </summary>
        /// <returns>A single precision floating point value that represents the available amount of ram in bytes on this machine.</returns>
        private float AvailableRam()
        {
            PerformanceCounter ctr = new PerformanceCounter("Memory", "Available Bytes", true);
            float t = ctr.NextValue();
            return t;
        }

        #endregion Utility
    }
}

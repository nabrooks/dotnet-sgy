using NbApps.Seismic.FileIo.Utility;
using Seismic.SegyFileIo;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Utility;
using Utility.Extensions;
using Utility.Io.Encodings;
using Utility.Io.Serialization;

namespace Hess.Seismic.SegyFileIo
{
    /// <summary>
    /// A file accessor for the sgy file format supports static methods for instantiation only.
    /// <para>
    /// Trace I/O is performed with <see cref="RandomAccess"/> positioned reads/writes over a <see cref="SafeFileHandle"/>.
    /// Because each call passes an explicit file offset and uses its own scratch buffer (rented from
    /// <see cref="ArrayPool{T}"/>), there is no shared stream position or shared buffer — so the read methods are safe to
    /// call concurrently from multiple threads against a single <see cref="SgyFile"/> instance. Writes mutate the file and
    /// are serialized with an internal lock.
    /// </para>
    /// </summary>
    public class SgyFile : Disposable, IDisposable
    {
        #region Constant values
        public const int TextHeaderBytesCount = 3200;
        public const int BinaryHeaderBytesCount = 400;
        public const int TraceHeaderBytesCount = 240;
        public const int SampleFormatIndex = 24;
        public const int SampleCountIndex = 114;
        #endregion Constant values

        private readonly SafeFileHandle handle;
        private FileHeader header;
        private readonly int dataSampleSize;
        private readonly FormatCode formatCode;
        private readonly bool isLittleEndian;

        // Cached layout, all fixed for the lifetime of the file.
        private readonly int samplesPerTrace;
        private readonly int traceDataBytes;    // samplesPerTrace * dataSampleSize
        private readonly int traceLengthBytes;  // TraceHeaderBytesCount + traceDataBytes
        private readonly long dataStartOffset;  // byte offset of the first trace

        // Serializes writers (and Append's read-length-then-write) against each other. Reads do not take this lock.
        private readonly object writeLock = new object();

        /// <summary>
        /// Private constructor.  The point of this is to limit the number of ways a sgy file can be opened.  Either it must be created from scratch with header properties as
        /// input arguments, or it must be opened from a preexisting file and header information must be inferred from the contents.
        /// </summary>
        protected SgyFile(FileInfo fileInfo, SafeFileHandle handle, IEnumerable<string> textHeaders, Encoding textHeaderEncoding, FileHeader header, EndianBitConverter endianBitconverter)
        {
            this.handle = handle;
            this.FileInfo = fileInfo;
            this.TextHeaders = textHeaders;
            this.TextHeaderEncoding = textHeaderEncoding;
            this.header = header;
            this.BitConverter = endianBitconverter;
            this.isLittleEndian = endianBitconverter.Endianness == Endianness.LittleEndian;
            this.formatCode = (FormatCode)header.DataSampleFormatCode;
            if (formatCode != FormatCode.IbmFloatingPoint4 && formatCode != FormatCode.IeeeFloatingPoint4)
                throw new NotImplementedException($"Sample conversion not implemented yet for format code: {header.DataSampleFormatCode}");

            this.dataSampleSize = SizeFrom(formatCode);
            this.samplesPerTrace = header.SamplesPerTraceOfFile;
            this.traceDataBytes = samplesPerTrace * dataSampleSize;
            this.traceLengthBytes = TraceHeaderBytesCount + traceDataBytes;
            this.dataStartOffset = TextHeaderBytesCount + BinaryHeaderBytesCount + ((long)textHeaders.Count() - 1) * TextHeaderBytesCount;
            this.TraceCount = traceLengthBytes == 0 ? 0 : (RandomAccess.GetLength(handle) - dataStartOffset) / traceLengthBytes;
        }

        /// <summary>
        /// The File info of the file
        /// </summary>
        public FileInfo FileInfo { get; }

        /// <summary>
        /// The encoding format of the textual header (segy supports ascii and ebcdic)
        /// </summary>
        public Encoding TextHeaderEncoding { get; }

        /// <summary>
        /// The bit converter used to convert bytes to appropriate machine format (segy standard prioritizes big endian byte ordering)
        /// </summary>
        public EndianBitConverter BitConverter { get; }

        /// <summary>
        /// True when the file's numeric fields are stored little-endian. SEG-Y is big-endian by spec; this is inferred
        /// from the sample-format code when opening an existing file.
        /// </summary>
        public bool IsLittleEndian => isLittleEndian;

        /// <summary>
        /// The binary file header with seismic metadata
        /// </summary>
        public FileHeader BinaryFileHeader => this.header;

        /// <summary>
        /// The text headers with commentary and processing/acquisition information
        /// </summary>
        public IEnumerable<string> TextHeaders { get; }

        /// <summary>
        /// The number of traces as inferred from read sample interval, file size, text header count, etc...
        /// </summary>
        public long TraceCount { get; private set; }

        /// <summary>
        /// Read all metadata (headers, encodings, formats etc...)
        /// </summary>
        /// <param name="fileInfo">The file info of the sgy file</param>
        public static SgyFile Open(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo), "File info cannot be null");
            if (fileInfo.Exists == false) throw new FileNotFoundException($"File {fileInfo.FullName} does not exist");
            if (fileInfo.Length < TextHeaderBytesCount + BinaryHeaderBytesCount) throw new ArgumentException($"File {fileInfo.FullName} does not have enough bytes in its content to infer file metadata.  This implies the file is corrupt, or not a real sgy file. Try creating a new sgy file instead.");

            var handle = File.OpenHandle(fileInfo.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            try
            {
                var bigEBitConverter = new BigEndianBitConverter();
                var lilEBitConverter = new LittleEndianBitConverter();

                // Get text header bytes and binary header bytes.
                byte[] headerBuffer = new byte[TextHeaderBytesCount + BinaryHeaderBytesCount];
                ReadAtFully(handle, headerBuffer, 0);

                // Evaluate text encoding (ascii and ebcdic supported by sgy....for no modern day reason)
                Encoding textHeaderEncoding = headerBuffer[0] == 'C' ? Encoding.ASCII : EbcdicEncoding.GetEncoding("EBCDIC-US");
                var textHeaders = new List<string> { textHeaderEncoding.GetString(headerBuffer, 0, TextHeaderBytesCount) };

                // Endianness and format code. Valid SEG-Y sample-format codes are 1..8; whichever byte ordering yields a
                // value in that range tells us the file's endianness. SEG-Y is big-endian by spec, so prefer big-endian on a tie.
                short bigEFormatCode = bigEBitConverter.ToInt16(headerBuffer, 3224);
                short lilEFormatCode = lilEBitConverter.ToInt16(headerBuffer, 3224);
                EndianBitConverter endianBitConverter;
                if (bigEFormatCode >= 1 && bigEFormatCode <= 8) endianBitConverter = bigEBitConverter;
                else if (lilEFormatCode >= 1 && lilEFormatCode <= 8) endianBitConverter = lilEBitConverter;
                else throw new Exception("Cannot infer endianess from the format code");

                // File Binary Header
                FileHeader binaryHeader = FileHeader.From(headerBuffer, TextHeaderBytesCount, endianBitConverter);

                // Extended text headers
                if (binaryHeader.ExtendedTextHeadersCount > 0)
                {
                    int extendedHeaderBytes = binaryHeader.ExtendedTextHeadersCount * TextHeaderBytesCount;
                    byte[] extendedBuffer = new byte[extendedHeaderBytes];
                    ReadAtFully(handle, extendedBuffer, 3600);
                    for (int i = 0; i < binaryHeader.ExtendedTextHeadersCount; i++)
                        textHeaders.Add(textHeaderEncoding.GetString(extendedBuffer, i * TextHeaderBytesCount, TextHeaderBytesCount));
                }

                return new SgyFile(fileInfo, handle, textHeaders, textHeaderEncoding, binaryHeader, endianBitConverter);
            }
            catch
            {
                handle.Dispose();
                throw;
            }
        }

        #region Creation Methods

        /// <summary>
        /// Creates a new Sgy file
        /// </summary>
        public static SgyFile Create(FileInfo fileInfo, FileHeader header, bool overwrite = false)
        {
            return Create(fileInfo, new string(new char[TextHeaderBytesCount]), EbcdicEncoding.GetEncoding("EBCDIC-US"), header, new BigEndianBitConverter(), overwrite);
        }

        /// <summary>
        /// Creates a new Sgy file
        /// </summary>
        public static SgyFile Create(FileInfo fileInfo, string textHeader, FileHeader header, bool overwrite = false)
        {
            return Create(fileInfo, textHeader, EbcdicEncoding.GetEncoding("EBCDIC-US"), header, new BigEndianBitConverter(), overwrite);
        }

        /// <summary>
        /// Creates a new Sgy file
        /// </summary>
        public static SgyFile Create(FileInfo fileInfo, string textHeader, Encoding textHeaderEncoding, FileHeader header, bool overwrite = false)
        {
            return Create(fileInfo, textHeader, textHeaderEncoding, header, new BigEndianBitConverter(), overwrite);
        }

        /// <summary>
        /// Creates a new Sgy file
        /// </summary>
        public static SgyFile Create(FileInfo fileInfo, string textHeader, FileHeader header, EndianBitConverter endianBitconverter, bool overwrite = false)
        {
            return Create(fileInfo, textHeader, EbcdicEncoding.GetEncoding("EBCDIC-US"), header, endianBitconverter, overwrite);
        }

        /// <summary>
        /// Creates a new Sgy file
        /// </summary>
        public static SgyFile Create(FileInfo fileInfo, string textHeader, Encoding textHeaderEncoding, FileHeader header, EndianBitConverter endianBitConverter, bool overwrite = false)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo), "File info cannot be null");
            if (textHeader == null) throw new ArgumentNullException(nameof(textHeader), "Text header cannot be null");
            if (textHeaderEncoding == null) throw new ArgumentNullException(nameof(textHeaderEncoding), "Text header encoding cannot be null");
            if (textHeader.Length > TextHeaderBytesCount) throw new ArgumentException("The text header contains more than 3200 characters");
            if (endianBitConverter == null) throw new ArgumentNullException(nameof(endianBitConverter), "Endian bit converter cannot be null");
            if (fileInfo.Exists && overwrite == false) throw new ArgumentException($"File {fileInfo.FullName} already exists. Please delete first, or select overwrite = true");
            if (overwrite == true && fileInfo.Exists) fileInfo.Delete();

            var handle = File.OpenHandle(fileInfo.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            try
            {
                var conditionedTextHeader = HandleTextHeader(textHeader);
                var textHeaderBytes = textHeaderEncoding.GetBytes(conditionedTextHeader);
                var binHeaderBytes = header.ToBytes(endianBitConverter);
                RandomAccess.Write(handle, textHeaderBytes.AsSpan(0, TextHeaderBytesCount), 0);
                RandomAccess.Write(handle, binHeaderBytes.AsSpan(0, BinaryHeaderBytesCount), TextHeaderBytesCount);
                fileInfo.Refresh();
                return new SgyFile(fileInfo, handle, new string[] { conditionedTextHeader }, textHeaderEncoding, header, endianBitConverter);
            }
            catch
            {
                handle.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a new Sgy file pre-populated with the supplied trace headers and a constant sample value.
        /// </summary>
        public static SgyFile Create(FileInfo fileInfo, string textHeader, Encoding textHeaderEncoding, FileHeader header, EndianBitConverter endianBitConverter, IEnumerable<TraceHeader> traceHeaders, float sampleValue = 0.0f)
        {
            var newFile = Create(fileInfo, textHeader, textHeaderEncoding, header, endianBitConverter, true);

            float[] sampleBuffer = new float[header.SamplesPerTraceOfFile];
            var traceHeadersBigArray = traceHeaders.ToBigArray();

            if (sampleValue != 0)
                for (int i = 0; i < sampleBuffer.Length; i++)
                    sampleBuffer[i] = sampleValue;

            for (long i = 0; i < traceHeadersBigArray.Length; i++)
            {
                newFile.Append(new Trace(traceHeadersBigArray[i], sampleBuffer));
            }
            return newFile;
        }

        /// <summary>
        /// Copys a preexisting seismic file except for sample values in each traces and populates
        /// each trace with sample values specified by an input arguemnt at runtime
        /// </summary>
        public static SgyFile CopyPopulated(FileInfo sourceFileInfo, FileInfo destinationFileInfo, float sampleValue = 0.0f)
        {
            if (destinationFileInfo.Exists == true)
                destinationFileInfo.Delete();

            destinationFileInfo = sourceFileInfo.FastCopy(destinationFileInfo);

            SgyFile newSgyFile = SgyFile.Open(destinationFileInfo);

            float[] buffer = new float[newSgyFile.BinaryFileHeader.SamplesPerTraceOfFile];
            if (sampleValue != 0)
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = sampleValue;

            for (long ti = 0; ti < newSgyFile.TraceCount; ti++)
                newSgyFile.Write(buffer, ti);
            return newSgyFile;
        }

        #endregion Creation Methods

        #region Read Methods

        /// <summary>
        /// Reads a trace from a specified trace index
        /// </summary>
        public Trace ReadTrace(int traceIndex) => ReadTrace((long)traceIndex);

        /// <summary>
        /// Reads a trace from a specified trace index. Thread-safe: may be called concurrently for different indices.
        /// </summary>
        public Trace ReadTrace(long traceIndex)
        {
            if (traceIndex < 0 || traceIndex >= TraceCount) throw new ArgumentOutOfRangeException(nameof(traceIndex), $"Cannot read a trace with trace index: {traceIndex}, when file only contains {TraceCount} traces");

            long offset = dataStartOffset + traceLengthBytes * traceIndex;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(traceLengthBytes);
            try
            {
                ReadAtFully(handle, buffer.AsSpan(0, traceLengthBytes), offset);
                var trHeader = TraceHeader.From(buffer, BitConverter);
                var traceData = new float[samplesPerTrace];
                DecodeSamples(buffer.AsSpan(TraceHeaderBytesCount, traceDataBytes), traceData);
                return new Trace(trHeader, traceData);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Reads a trace header at a specified trace index
        /// </summary>
        public TraceHeader ReadTraceHeader(int traceIndex) => ReadTraceHeader((long)traceIndex);

        /// <summary>
        /// Reads a trace header at a specified trace index. Thread-safe.
        /// </summary>
        public TraceHeader ReadTraceHeader(long traceIndex)
        {
            if (traceIndex < 0 || traceIndex >= TraceCount) throw new ArgumentOutOfRangeException(nameof(traceIndex), $"Cannot read a trace with trace index: {traceIndex}, when file only contains {TraceCount} traces");

            long offset = dataStartOffset + traceLengthBytes * traceIndex;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(TraceHeaderBytesCount);
            try
            {
                ReadAtFully(handle, buffer.AsSpan(0, TraceHeaderBytesCount), offset);
                return TraceHeader.From(buffer, BitConverter);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Reads all trace headers in this file
        /// </summary>
        public IEnumerable<TraceHeader> ReadTraceHeaders()
        {
            return ReadTraceHeaders(0, TraceCount);
        }

        /// <summary>
        /// Read "n" trace headers from a starting position
        /// </summary>
        public IEnumerable<TraceHeader> ReadTraceHeaders(long startTraceIndex, long traceCount)
        {
            if (startTraceIndex < 0 || startTraceIndex > TraceCount) throw new ArgumentOutOfRangeException(nameof(startTraceIndex), $"Cannot read a trace header with trace index: {startTraceIndex}, when file only contains {TraceCount} traces");

            long fileLength = RandomAccess.GetLength(handle);
            BigArray<TraceHeader> traceHeaders = new BigArray<TraceHeader>(traceCount);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(TraceHeaderBytesCount);
            try
            {
                for (long tid = 0; tid < traceCount; tid++)
                {
                    long offset = dataStartOffset + traceLengthBytes * (startTraceIndex + tid);
                    if (offset + TraceHeaderBytesCount > fileLength) break;
                    ReadAtFully(handle, buffer.AsSpan(0, TraceHeaderBytesCount), offset);
                    traceHeaders[tid] = TraceHeader.From(buffer, BitConverter);
                }
                return traceHeaders;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Read all traces (careful if there a memory considerations)
        /// </summary>
        public IEnumerable<Trace> ReadTraces()
        {
            return ReadTraces(0, TraceCount);
        }

        /// <summary>
        /// Read "n" traces from a starting position
        /// </summary>
        public IEnumerable<Trace> ReadTraces(long startTraceIndex, long traceCount)
        {
            if (startTraceIndex < 0 || startTraceIndex > TraceCount) throw new ArgumentOutOfRangeException(nameof(startTraceIndex), $"Cannot read a trace with trace index: {startTraceIndex}, when file only contains {TraceCount} traces");

            BigList<Trace> traces = new BigList<Trace>(traceCount);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(traceLengthBytes);
            try
            {
                for (long tid = 0; tid < traceCount; tid++)
                {
                    long offset = dataStartOffset + traceLengthBytes * (startTraceIndex + tid);
                    ReadAtFully(handle, buffer.AsSpan(0, traceLengthBytes), offset);
                    var trHeader = TraceHeader.From(buffer, BitConverter);
                    var traceData = new float[samplesPerTrace];
                    DecodeSamples(buffer.AsSpan(TraceHeaderBytesCount, traceDataBytes), traceData);
                    traces.Add(new Trace(trHeader, traceData));
                }
                return traces;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        #endregion Read Methods

        #region Write Methods

        /// <summary>
        /// Appends a trace to the end of the file
        /// </summary>
        public void Append(Trace trace)
        {
            if (trace == null) throw new ArgumentNullException(nameof(trace), "Trace cannot be null when appending");
            if (trace.Data == null) throw new ArgumentNullException(nameof(trace), "Trace data cannot be null when appending");
            if (trace.Data.Length != samplesPerTrace) throw new ArgumentException($"Trace (Length:{trace.Data.Length}) must have {samplesPerTrace}");

            byte[] buffer = ArrayPool<byte>.Shared.Rent(traceLengthBytes);
            try
            {
                EncodeTrace(trace, buffer);
                lock (writeLock)
                {
                    long offset = RandomAccess.GetLength(handle);
                    RandomAccess.Write(handle, buffer.AsSpan(0, traceLengthBytes), offset);
                    TraceCount++;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Appends a set of traces to the end of the file
        /// </summary>
        public void Append(IEnumerable<Trace> traces)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(traceLengthBytes);
            try
            {
                lock (writeLock)
                {
                    long offset = RandomAccess.GetLength(handle);
                    foreach (var trace in traces)
                    {
                        if (trace?.Data == null || trace.Data.Length != samplesPerTrace)
                            throw new ArgumentException($"Each trace must contain {samplesPerTrace} samples");
                        EncodeTrace(trace, buffer);
                        RandomAccess.Write(handle, buffer.AsSpan(0, traceLengthBytes), offset);
                        offset += traceLengthBytes;
                        TraceCount++;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Writes a trace at a specified trace index (assumes constant trace length file)
        /// </summary>
        public void Write(Trace trace, int traceIndex) => Write(trace, (long)traceIndex);

        /// <summary>
        /// Writes a trace at a specified trace index (assumes constant trace length file)
        /// </summary>
        public void Write(Trace trace, long traceIndex)
        {
            if (trace == null) throw new ArgumentNullException(nameof(trace), "Trace cannot be null when writing");
            if (trace.Data == null) throw new ArgumentNullException(nameof(trace), "Trace data cannot be null when writing");
            if (trace.Data.Length != samplesPerTrace) throw new ArgumentException($"Trace (Length:{trace.Data.Length}) must have {samplesPerTrace}");

            byte[] buffer = ArrayPool<byte>.Shared.Rent(traceLengthBytes);
            try
            {
                EncodeTrace(trace, buffer);
                long offset = dataStartOffset + traceIndex * traceLengthBytes;
                lock (writeLock)
                    RandomAccess.Write(handle, buffer.AsSpan(0, traceLengthBytes), offset);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Writes a set of traces at a specified trace index (assumes constant trace length file)
        /// </summary>
        public void Write(IEnumerable<Trace> traces, int startTraceIndex) => Write(traces, (long)startTraceIndex);

        /// <summary>
        /// Writes a set of traces at a specified trace index (assumes constant trace length file)
        /// </summary>
        public void Write(IEnumerable<Trace> traces, long startTraceIndex)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(traceLengthBytes);
            try
            {
                long offset = dataStartOffset + startTraceIndex * traceLengthBytes;
                lock (writeLock)
                {
                    foreach (var trace in traces)
                    {
                        if (trace?.Data == null || trace.Data.Length != samplesPerTrace)
                            throw new ArgumentException($"Each trace must contain {samplesPerTrace} samples");
                        EncodeTrace(trace, buffer);
                        RandomAccess.Write(handle, buffer.AsSpan(0, traceLengthBytes), offset);
                        offset += traceLengthBytes;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Writes an array of floats (sample data only) to the trace at the given index.
        /// </summary>
        public void Write(float[] traceData, int traceIndex) => Write(traceData, (long)traceIndex);

        /// <summary>
        /// Writes an array of floats (sample data only) to the trace at the given index, leaving its header intact.
        /// </summary>
        public void Write(float[] traceData, long traceIndex)
        {
            if (traceData == null) throw new ArgumentNullException(nameof(traceData), "Trace data cannot be null when writing");
            if (traceData.Length != samplesPerTrace) throw new ArgumentException($"Trace (Length:{traceData.Length}) must have {samplesPerTrace}");

            byte[] buffer = ArrayPool<byte>.Shared.Rent(traceDataBytes);
            try
            {
                EncodeSamples(traceData, buffer.AsSpan(0, traceDataBytes));
                long offset = dataStartOffset + traceIndex * traceLengthBytes + TraceHeaderBytesCount;
                lock (writeLock)
                    RandomAccess.Write(handle, buffer.AsSpan(0, traceDataBytes), offset);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Writes a set of arrays of floats (sample data only) starting at the given trace index.
        /// </summary>
        public void Write(IEnumerable<float[]> traceDatas, int startTraceIndex) => Write(traceDatas, (long)startTraceIndex);

        /// <summary>
        /// Writes a set of arrays of floats (sample data only) starting at the given trace index.
        /// </summary>
        public void Write(IEnumerable<float[]> traceDatas, long startTraceIndex)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(traceDataBytes);
            try
            {
                long traceIndex = startTraceIndex;
                lock (writeLock)
                {
                    foreach (float[] traceData in traceDatas)
                    {
                        if (traceData == null || traceData.Length != samplesPerTrace)
                            throw new ArgumentException($"Each trace must contain {samplesPerTrace} samples");
                        EncodeSamples(traceData, buffer.AsSpan(0, traceDataBytes));
                        long offset = dataStartOffset + traceIndex * traceLengthBytes + TraceHeaderBytesCount;
                        RandomAccess.Write(handle, buffer.AsSpan(0, traceDataBytes), offset);
                        traceIndex++;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Writes a trace header at a specified trace index (assumes constant trace length file and the trace exists)
        /// </summary>
        public void Write(TraceHeader traceHeader, int traceIndex) => Write(traceHeader, (long)traceIndex);

        /// <summary>
        /// Writes a trace header at a specified trace index (assumes constant trace length file and the trace exists)
        /// </summary>
        public void Write(TraceHeader traceHeader, long traceIndex)
        {
            if (traceIndex < 0 || traceIndex >= TraceCount) throw new ArgumentOutOfRangeException(nameof(traceIndex), "There are fewer traces in the file than the intended trace header index to write");

            byte[] buffer = ArrayPool<byte>.Shared.Rent(TraceHeaderBytesCount);
            try
            {
                traceHeader.ToBytes(buffer, 0, BitConverter);
                long offset = dataStartOffset + traceIndex * traceLengthBytes;
                lock (writeLock)
                    RandomAccess.Write(handle, buffer.AsSpan(0, TraceHeaderBytesCount), offset);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Writes a set of trace headers starting at a specified trace index (assumes constant trace length file and the traces exist)
        /// </summary>
        public void Write(IEnumerable<TraceHeader> traceHeaders, int startTraceIndex) => Write(traceHeaders, (long)startTraceIndex);

        /// <summary>
        /// Writes a set of trace headers starting at a specified trace index (assumes constant trace length file and the traces exist)
        /// </summary>
        public void Write(IEnumerable<TraceHeader> traceHeaders, long startTraceIndex)
        {
            if (TraceCount < traceHeaders.Count() + startTraceIndex) throw new ArgumentException("There are fewer traces in the file then intended trace headers to write");

            byte[] buffer = ArrayPool<byte>.Shared.Rent(TraceHeaderBytesCount);
            try
            {
                long traceIndex = startTraceIndex;
                lock (writeLock)
                {
                    foreach (var traceHeader in traceHeaders)
                    {
                        traceHeader.ToBytes(buffer, 0, BitConverter);
                        long offset = dataStartOffset + traceIndex * traceLengthBytes;
                        RandomAccess.Write(handle, buffer.AsSpan(0, TraceHeaderBytesCount), offset);
                        traceIndex++;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        #endregion Write Methods

        /// <summary>
        /// Encodes a trace (240-byte header followed by sample data) into <paramref name="buffer"/>, which must be at
        /// least <see cref="traceLengthBytes"/> long.
        /// </summary>
        private void EncodeTrace(Trace trace, byte[] buffer)
        {
            trace.Header.ToBytes(buffer, 0, BitConverter);
            EncodeSamples(trace.Data, buffer.AsSpan(TraceHeaderBytesCount, traceDataBytes));
        }

        /// <summary>
        /// Decodes the file's sample format (IBM or IEEE) from <paramref name="src"/> into <paramref name="dst"/>.
        /// </summary>
        private void DecodeSamples(ReadOnlySpan<byte> src, Span<float> dst)
        {
            if (formatCode == FormatCode.IbmFloatingPoint4) SampleCodec.DecodeIbm(src, dst);
            else SampleCodec.DecodeIeee(src, dst, isLittleEndian);
        }

        /// <summary>
        /// Encodes <paramref name="src"/> into the file's sample format (IBM or IEEE) in <paramref name="dst"/>.
        /// </summary>
        private void EncodeSamples(ReadOnlySpan<float> src, Span<byte> dst)
        {
            if (formatCode == FormatCode.IbmFloatingPoint4) SampleCodec.EncodeIbm(src, dst);
            else SampleCodec.EncodeIeee(src, dst, isLittleEndian);
        }

        /// <summary>
        /// Reads exactly <paramref name="buffer"/>.Length bytes from <paramref name="handle"/> starting at
        /// <paramref name="offset"/>, looping until satisfied. Positioned reads are thread-safe.
        /// </summary>
        /// <exception cref="EndOfStreamException">The file ended before the buffer was filled.</exception>
        private static void ReadAtFully(SafeFileHandle handle, Span<byte> buffer, long offset)
        {
            int total = 0;
            while (total < buffer.Length)
            {
                int read = RandomAccess.Read(handle, buffer.Slice(total), offset + total);
                if (read == 0) throw new EndOfStreamException($"Expected {buffer.Length} bytes at offset {offset} but reached end of file after {total}.");
                total += read;
            }
        }

        /// <summary>
        /// Gets the size in bytes of the sample given an encoding format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int SizeFrom(FormatCode format)
        {
            switch (format)
            {
                case FormatCode.IbmFloatingPoint4: return 4;
                case FormatCode.TwosComplementInteger4: return 4;
                case FormatCode.TwosComplementInteger2: return 2;
                case FormatCode.FixedPointWithGain4: return 4;
                case FormatCode.IeeeFloatingPoint4: return 4;
                case FormatCode.TwosComplementInteger1: return 1;
                default: throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        /// <summary>
        /// Accepts a text string and evaluates if row delimiters have been inserted or not,
        /// then inserts them if needed.
        /// </summary>
        private static string HandleTextHeader(string textHeader)
        {
            // Pad up front so short/empty headers don't index out of range, and so every returned header is exactly
            // 3200 bytes (40 rows x 80 columns) — the size Create writes to disk.
            var textHeaderPadded = textHeader.PadRight(TextHeaderBytesCount);

            // if header already contains "C" prepended strings
            if (textHeaderPadded[0] == 'C' && (textHeaderPadded[1] == '1' || textHeaderPadded[2] == '1'))
                return textHeaderPadded;

            // otherwise insert "C" strings between
            StringBuilder sb = new StringBuilder();
            for (int ri = 0; ri < 40; ri++)
            {
                if (ri == 38)
                { sb.Append("C39 SEG Y REV 1".PadRight(80)); continue; }
                if (ri == 39)
                { sb.Append("C40 END TEXTUAL HEADER".PadRight(80)); continue; }

                var rowNumberString = (ri + 1).ToString("00");
                sb.Append('C' + rowNumberString);
                for (int ci = 0; ci < 77; ci++)
                    sb.Append(textHeaderPadded[ci + (ri * 77)]);
            }
            string result = sb.ToString();
            return result;
        }

        protected override void DisposeManagedResources()
        {
            // Positioned writes go straight to the OS handle (no user-space buffering to flush); closing the handle
            // is all that is required.
            this.handle.Dispose();
        }
    }
}

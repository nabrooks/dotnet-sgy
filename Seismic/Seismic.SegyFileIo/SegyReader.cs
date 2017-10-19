using Seismic.SegyFileIo.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.DataTypes;
using Utility.Io;
using Utility.Io.Serialization;
using Utility.Serialization;

namespace Seismic.SegyFileIo
{
    /// <summary>
    /// Opens a stream for reading data and file headers to a Society of Exploration Geophysicists "Y" file format.
    /// </summary>
    public class SgyReader : Disposable
    {
        private readonly string _filepath;
        private readonly FileStream _stream;
        private readonly Encoding _textHeaderEncoding = EbcdicEncoding.GetEncoding("EBCDIC-US");
        private readonly BinaryReader _reader;

        #region Const

        public const int TextHeaderBytesCount = 3200;
        public const int BinaryHeaderBytesCount = 400;
        public const int TraceHeaderBytesCount = 240;
        public const int SampleFormatIndex = 24;
        public const int SampleCountIndex = 114;

        #endregion Const

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="fileInfo">File Info for the file to read</param>
        public SgyReader(FileInfo fileInfo) : this(fileInfo.FullName) { }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="filepath">The path string of the file</param>
        public SgyReader(string filepath)
        {
            CodeContract.Requires(!string.IsNullOrEmpty(filepath));
            CodeContract.Requires<FileNotFoundException>(File.Exists(filepath), $"File {filepath} was not found.");

            _filepath = filepath;
            _stream = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Write);
            _reader = new BinaryReader(_stream);
            FileInfo = new FileInfo(filepath);

            //
            // Data Endianess
            //
            _reader.BaseStream.Seek(3224, SeekOrigin.Begin);
            var formatBytes = _reader.ReadBytes(2);

            var lilEndianFormatCode = BitConverter.ToInt16(formatBytes, 0);
            var bigEndianFormatCode = IbmConverter.ToInt16(formatBytes, 0);
            bool endianessSuccess = false;
            for (var i = 0; i <= 8; i++)
            {
                if (bigEndianFormatCode == i) { IsLittleEndian = false; endianessSuccess = true; }
                if (lilEndianFormatCode == i) { IsLittleEndian = true; endianessSuccess = true; }
            }
            if (endianessSuccess == false) throw new SegyEndianessException("Cannot infer endianess from format code. format code value is not within range 0 - 8 in either endianess");

            //
            // File Binary Header
            //
            _reader.BaseStream.Seek(TextHeaderBytesCount, SeekOrigin.Begin);
            byte[] binaryHeaderBytes = _reader.ReadBytes(BinaryHeaderBytesCount);
            FileBinaryHeader = BinaryHeader.From(binaryHeaderBytes, IsLittleEndian);
            FileBinaryHeaderBytes = binaryHeaderBytes;

            //
            // File Textual Headers
            //
            _stream.Seek(0, SeekOrigin.Begin);

            List<string> textFileHeaders = new List<string>();
            byte[] bytes = _reader.ReadBytes(TextHeaderBytesCount);
            string textHeader = bytes[0] == 'C'
                ? Encoding.Default.GetString(bytes)
                : _textHeaderEncoding.GetString(bytes);
            textFileHeaders.Add(textHeader);

            // as per rev 1, all data values are assumed big endian
            var binaryFileHeader = FileBinaryHeader;

            _stream.Seek(3600, SeekOrigin.Begin);
            byte[] extendedFileHeaders = binaryFileHeader.ExtendedTextHeadersCount < 0
                ? new byte[0]
                : _reader.ReadBytes(binaryFileHeader.ExtendedTextHeadersCount * 3200);

            for (int i = 0; i < binaryFileHeader.ExtendedTextHeadersCount; i++)
            {
                var extTextHeaderBytes = new byte[3200];
                Buffer.BlockCopy(extendedFileHeaders, i * 3200, bytes, 0, 3200);
                var extendedHeader = bytes[0] == 'C'
                    ? Encoding.Default.GetString(extTextHeaderBytes)
                    : _textHeaderEncoding.GetString(extTextHeaderBytes);
                textFileHeaders.Add(extendedHeader);
            }
            FileTextualHeaders = textFileHeaders.ToArray();

            //
            //Trace Count
            //
            var sampleformat = (FormatCode)FileBinaryHeader.DataSampleFormatCode;

            var dataStartIndex = TextHeaderBytesCount + BinaryHeaderBytesCount + TextHeaderBytesCount * (FileTextualHeaders.Length - 1);
            var dataEndIndex = _stream.Length;
            var sampleSz = SizeFrom(sampleformat);

            var fileSz = _stream.Length;
            var nsamples = FileBinaryHeader.SamplesPerTraceOfFile;
            var extTxtHdrCt = FileTextualHeaders.Length - 1;
            TraceCount = (fileSz - (3600 + (extTxtHdrCt * 3200))) / (240 + (nsamples * sampleSz));
        }

        /// <summary>
        /// The FileInfo of the file to be read.
        /// </summary>
        public FileInfo FileInfo { get; }

        /// <summary>
        /// The number of traces in the file. This is acquired dynamically and is synchronized with the current state of the file.
        /// </summary>
        public long TraceCount { get; }

        /// <summary>
        /// Attempts to infer the endianess of the binary serializer used when writing the file.
        /// via format code value inferred from both big and little endianess. if the format code value
        /// deserialized via little endian deserialization is an acceptable value (1-8), then returns 
        /// </summary>
        /// <returns>
        /// false if the format code value deserialized via big endian deserialization is either 1-8 or 0.
        /// true if the format code value deserialized via little endian deserialization is an acceptable value (1-8)
        /// </returns>
        public bool IsLittleEndian { get; }

        /// <summary>
        /// Reads the binary file header of the Segy file
        /// </summary>
        /// <returns>The binary header</returns>
        public BinaryHeader FileBinaryHeader { get; }

        /// <summary>
        /// The bytes read from the file that represent the binary file header
        /// </summary>
        public byte[] FileBinaryHeaderBytes { get; }

        /// <summary>
        /// Reads the first Ecbdic (assumed) textual file header and all extended file headers as counted for in the binary file header.
        /// </summary>
        /// <returns>A collection of strings, each of which represent a textual file header</returns>
        public string[] FileTextualHeaders { get; }

        /// <summary>
        /// Scans all sample values and evaluates the min and max amplitude of the sample values
        /// </summary>
        /// <param name="progress">progress notifier</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>A pair of floats, the first of whick is min, second is max.</returns>
        public Tuple<float, float> GetAmplitudeRange(IProgress<int> progress = null, CancellationToken ct = default(CancellationToken))
        {
            var lastTraceToReadIndex = TraceCount;
            if (lastTraceToReadIndex != Int64.MaxValue) CodeContract.Requires(lastTraceToReadIndex <= TraceCount, "Ending trace index must be less than the number of traces in the file.");

            var islilEndian = IsLittleEndian;

            var dataStartIndex = 3600 + (3200 * (FileTextualHeaders.Length - 1));
            var sampleFormat = (FormatCode)FileBinaryHeader.DataSampleFormatCode;
            int sampleSz = SizeFrom(sampleFormat);

            // as per rev 1, all data values are assumed big endian
            var streamLen = _stream.Length;
            var streamPos = dataStartIndex + 0 * (240 + sampleSz * FileBinaryHeader.SamplesPerTraceOfFile);

            CodeContract.Assume(streamPos <= _stream.Length, "initial trace index exceeds file length.");

            _stream.Seek(streamPos, SeekOrigin.Begin);
            int ns = FileBinaryHeader.SamplesPerTraceOfFile; // Assume that the binary header has num samples per trace. dont trust trace headers
            var traceDataBytesSz = ns * sampleSz;

            int progPercent = 0;
            float min = Single.PositiveInfinity;
            float max = Single.NegativeInfinity;
            for (long i = 0; (i < lastTraceToReadIndex) && (streamPos < streamLen); i++)
            {
                _stream.Seek(TraceHeaderBytesCount, SeekOrigin.Current);
                var trDataBytes = _reader.ReadBytes(traceDataBytesSz);
                var trData = GetData(trDataBytes, sampleFormat, ns);
                Parallel.For(0, trData.Length, k =>
                {
                    if (trData[k] < min) min = trData[k];
                    if (trData[k] > max) max = trData[k];
                });
                streamPos += 240 + traceDataBytesSz;

                if (ct.IsCancellationRequested) break;
                if (progress == null) continue;
                var percent = (int)(100 * (double)_stream.Position / _stream.Length);
                if (progPercent == percent) continue;
                progress.Report(percent);
                progPercent = percent;
            }
            return new Tuple<float, float>(min, max);
        }

        /// <summary>
        /// Reads a single trace from the file. 
        /// </summary>
        /// <param name="traceIndex">The trace index as it appears in squence in the file</param>
        /// <returns>The segy trace</returns>
        public SegyTrace ReadTrace(long traceIndex)
        {
            CodeContract.Requires(traceIndex < TraceCount, "Trace index to read must be less than the number of traces in the file.");
            CodeContract.Requires(traceIndex >= 0, "Trace index must be greater than or equal to 0");

            var islilEndian = IsLittleEndian;

            _stream.Seek(0, SeekOrigin.Begin);

            var textFileHeadersCount = FileTextualHeaders.Length;
            var binaryHeader = FileBinaryHeader;
            var dataStartIndex = TextHeaderBytesCount + BinaryHeaderBytesCount + TextHeaderBytesCount * (textFileHeadersCount - 1);
            var sampleFormat = (FormatCode)binaryHeader.DataSampleFormatCode;
            int sampleSz = SizeFrom(sampleFormat);

            // as per rev 1, all data values are assumed big endian
            var ns = binaryHeader.SamplesPerTraceOfFile;
            var initStreamPosition = dataStartIndex + (240 + ns * sampleSz) * traceIndex;

            CodeContract.Assume(initStreamPosition <= _stream.Length, "initial trace index exceeds file length.");
            _stream.Seek(initStreamPosition, SeekOrigin.Begin);

            var traceHeaderByteArr = _reader.ReadBytes(TraceHeaderBytesCount);
            var trHeader = SegyTraceHeader.From(traceHeaderByteArr, islilEndian);
            var traceDataBytesSz = trHeader.SampleCount * sampleSz;
            var traceDataBytes = _reader.ReadBytes(traceDataBytesSz);
            var traceData = GetData(traceDataBytes, sampleFormat, trHeader.SampleCount);
            var seismicTrace = new SegyTrace { ComponentAxis = 0, Data = traceData, Header = trHeader };

            return seismicTrace;
        }

        /// <summary>
        /// Reads a single trace header from the file
        /// </summary>
        /// <param name="traceIndex">The trace index as it appears in squence in the file</param>
        /// <returns>The segy trace header</returns>
        public SegyTraceHeader ReadTraceHeader(long traceIndex)
        {
            CodeContract.Requires(traceIndex < (long)TraceCount, "Trace index to read must be less than the number of traces in the file.");
            CodeContract.Requires(traceIndex >= 0, "Trace index must be greater than or equal to 0");
            var isLilEndian = IsLittleEndian;

            _stream.Seek(0, SeekOrigin.Begin);

            var textFileHeadersCount = FileTextualHeaders.Length;
            var binaryHeader = FileBinaryHeader;
            var dataStartIndex = TextHeaderBytesCount + BinaryHeaderBytesCount + TextHeaderBytesCount * (textFileHeadersCount - 1);
            var sampleFormat = (FormatCode)binaryHeader.DataSampleFormatCode;
            var sampleSz = SizeFrom(sampleFormat);

            var initStreamPosition = dataStartIndex +
                                     traceIndex * (TraceHeaderBytesCount + sampleSz * binaryHeader.SamplesPerTraceOfFile);

            // as per rev 1, all data values are assumed big endian
            CodeContract.Assume(initStreamPosition <= _stream.Length, "initial trace index exceeds file length.");
            _stream.Seek(initStreamPosition, SeekOrigin.Begin);


            var traceHeaderByteArr = _reader.ReadBytes(TraceHeaderBytesCount);
            return SegyTraceHeader.From(traceHeaderByteArr, isLilEndian);
        }

        /// <summary>
        /// Reads a set of segy traces in the file within a defined range
        /// <remarks>
        /// if startTrace and endTrace are default for this method, all traces in the file will be read</remarks>
        /// </summary>
        /// <param name="progress">A progress handler</param>
        /// <param name="ct">A cancellation token</param>
        /// <param name="startTrace">The 0 based starting trace index to read</param>
        /// <param name="nTraces">The 0 based ending trace index to read</param>
        /// <returns>A collection of segy traces</returns>
        public IEnumerable<SegyTrace> ReadTraces(IProgress<int> progress = null, CancellationToken ct = default(CancellationToken), long startTrace = 0, long nTraces = Int64.MaxValue)
        {
            var lastTraceToReadIndex = startTrace + nTraces;
            if (lastTraceToReadIndex > TraceCount) lastTraceToReadIndex = TraceCount;
            if (lastTraceToReadIndex != Int64.MaxValue) CodeContract.Requires(lastTraceToReadIndex <= TraceCount, "Ending trace index must be less than the number of traces in the file.");

            CodeContract.Requires(startTrace >= 0, "Cannot read a negative number of traces.");
            CodeContract.Requires(startTrace >= 0, "Starting trace index must be greater than 0.");

            var islilEndian = IsLittleEndian;

            var dataStartIndex = 3600 + (3200 * (FileTextualHeaders.Length - 1));
            var sampleFormat = (FormatCode)FileBinaryHeader.DataSampleFormatCode;
            int sampleSz = SizeFrom(sampleFormat);

            // as per rev 1, all data values are assumed big endian
            BigList<SegyTrace> traces;
            if (nTraces == long.MaxValue) traces = new BigList<SegyTrace>();
            else traces = new BigList<SegyTrace>(nTraces);
            var streamLen = _stream.Length;
            var streamPos = dataStartIndex + startTrace * (240 + sampleSz * FileBinaryHeader.SamplesPerTraceOfFile);

            CodeContract.Assume(streamPos <= _stream.Length, "initial trace index exceeds file length.");

            _stream.Seek(streamPos, SeekOrigin.Begin);
            int ns = FileBinaryHeader.SamplesPerTraceOfFile; // Assume that the binary header has num samples per trace. dont trust trace headers
            var traceDataBytesSz = ns * sampleSz;

            int progPercent = 0;

            for (long i = startTrace; (i < lastTraceToReadIndex) && (streamPos < streamLen); i++)
            {
                byte[] trHeaderBytes = _reader.ReadBytes(TraceHeaderBytesCount);
                byte[] trDataBytes = _reader.ReadBytes(traceDataBytesSz);

                var trHeader = SegyTraceHeader.From(trHeaderBytes, islilEndian);

                var trData = GetData(trDataBytes, sampleFormat, ns);
                traces.Add(new SegyTrace(trHeader, trData));
                streamPos += 240 + traceDataBytesSz;

                if (ct.IsCancellationRequested) break;
                if (progress == null) continue;
                var percent = (int)(100 * (double)_stream.Position / _stream.Length);
                if (progPercent == percent) continue;
                progress.Report(percent);
                progPercent = percent;
            }
            return traces;
        }

        /// <summary>
        /// Reads a set of segy traceheaders in the file within a defined range
        /// <remarks>
        /// if startTrace and endTrace are default for this method, all traces in the file will be read</remarks>
        /// </summary>
        /// <param name="progress">A progress handler</param>
        /// <param name="ct">A cancellation token</param>
        /// <param name="startTrace">The 0 based starting trace index to read</param>
        /// <param name="endTrace">The 0 based ending trace index to read</param>
        /// <returns>A collection of segy trace headers</returns>
        public IEnumerable<SegyTraceHeader> ReadTraceHeaders(IProgress<int> progress = null, CancellationToken ct = default(CancellationToken), long startTrace = 0, long nTraces = Int64.MaxValue)
        {
            var lastTraceToReadIndex = startTrace + nTraces;
            if (lastTraceToReadIndex > TraceCount) lastTraceToReadIndex = TraceCount;
            if (lastTraceToReadIndex != Int64.MaxValue) CodeContract.Requires(lastTraceToReadIndex <= TraceCount, "Ending trace index must be less than the number of traces in the file.");
            CodeContract.Requires(startTrace >= 0, "Cannot read a negative number of traces.");
            CodeContract.Requires(startTrace >= 0, "Starting trace index must be greater than 0.");

            var islilEndian = IsLittleEndian;

            var dataStartIndex = 3600 + (3200 * (FileTextualHeaders.Length - 1));
            var sampleFormat = (FormatCode)FileBinaryHeader.DataSampleFormatCode;
            int sampleSz = SizeFrom(sampleFormat);

            // as per rev 1, all data values are assumed big endian
            //BigList<SgyTraceHeader> trHdrs;
            BigArray<SegyTraceHeader> trHArr;
            if (nTraces == long.MaxValue) trHArr = new BigArray<SegyTraceHeader>(TraceCount - startTrace);
            else trHArr = new BigArray<SegyTraceHeader>(nTraces);
            var streamLen = _stream.Length;
            var streamPos = dataStartIndex + startTrace * (240 + sampleSz * FileBinaryHeader.SamplesPerTraceOfFile);

            CodeContract.Assume(streamPos <= _stream.Length, "initial trace index exceeds file length.");

            _stream.Seek(streamPos, SeekOrigin.Begin);
            int ns = FileBinaryHeader.SamplesPerTraceOfFile; // Assume that the binary header has num samples per trace. dont trust trace headers
            var traceDataBytesSz = ns * sampleSz;

            int progPercent = 0;

            for (long i = startTrace; (i < lastTraceToReadIndex) && (streamPos < streamLen); i++)
            {
                var traceHeaderByteArr = _reader.ReadBytes(TraceHeaderBytesCount);
                var trHdr = SegyTraceHeader.From(traceHeaderByteArr, islilEndian);
                trHArr[i - startTrace] = trHdr;
                _stream.Seek(traceDataBytesSz, SeekOrigin.Current);

                streamPos += 240 + traceDataBytesSz;

                if (ct.IsCancellationRequested) break;
                if (progress == null) continue;
                var percent = (int)(100 * (double)_stream.Position / _stream.Length);
                if (progPercent == percent) continue;
                progress.Report(percent);
                progPercent = percent;
            }
            return trHArr;
        }

        #region Disposable members

        /// <summary>
        /// Closes the file stream
        /// </summary>
        public void Close()
        {
            _reader.Close();
        }

        /// <summary>
        /// Disposes managed resources
        /// </summary>
        protected override void DisposeManagedResources()
        {
            _stream.Close();
            _reader.Close();
            _stream.Dispose();
            _reader.Dispose();
        }

        #endregion Disposable members

        #region Utility

        /// <summary>
        /// Gets an array of floats from a big endian encoded byte array
        /// </summary>
        /// <param name="trBytes">The byte array to interpret</param>
        /// <param name="sampleFormat">The sample format</param>
        /// <param name="sampleCount">the number of samples to read</param>
        /// <returns>The float array</returns>
        private static float[] GetData(byte[] trBytes, FormatCode sampleFormat, int sampleCount)
        {
            float[] traceData = new float[sampleCount];
            switch (sampleFormat)
            {
                case FormatCode.IbmFloatingPoint4:
                    Buffer.BlockCopy(trBytes, 0, traceData, 0, trBytes.Length);
                    IbmConverter.ibm_to_float(traceData, traceData, traceData.Length);
                    break;
                case FormatCode.IeeeFloatingPoint4:
                    ByteConverter.ToSingles(trBytes, ref traceData);
                    break;
                case FormatCode.TwosComplementInteger1:
                    for (int j = 0; j < sampleCount; j++)
                        traceData[j] = trBytes[j] < 128 ? trBytes[j] : trBytes[j] - 256;
                    break;
                case FormatCode.TwosComplementInteger2:
                    for (int j = 0; j < sampleCount; j++)
                        traceData[j] = IbmConverter.ToInt16(new[] { trBytes[(j * 2) + 1], trBytes[(j * 2) + 0] });
                    break;
                case FormatCode.TwosComplementInteger4:
                    for (int j = 0; j < sampleCount; j++)
                        traceData[j] = BitConverter.ToInt32(new[] { trBytes[(j * 4) + 3], trBytes[(j * 4) + 2], trBytes[(j * 4) + 1], trBytes[(j * 4) + 0] }, 0);
                    break;
                default: throw new NotSupportedException($"Unsupported sample format: {sampleFormat}");
            }
            return traceData;
        }

        /// <summary>
        /// Gets the size in bytes of the sample given an encoding format
        /// </summary>
        private static int SizeFrom(FormatCode format)
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

        #endregion Utility
    }
}

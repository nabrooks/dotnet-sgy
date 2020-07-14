using NbApps.Seismic.FileIo.Utility;
using Seismic.SegyFileIo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Utility;
using Utility.Extensions;
using Utility.Io.Encodings;
using Utility.Io.Serialization;

namespace Hess.Seismic.SegyFileIo
{
    /// <summary>
    /// A file accessor for the sgy file format supports static methods for instantiation only
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

        private Stream stream;
        private FileHeader header;
        private IBinaryReader binaryReader;
        private IBinaryWriter binaryWriter;
        private int dataSampleSize;

        private Func<int, float[]> traceDataReadingFunc;
        private Action<float[]> traceDataWritingFunc;

        /// <summary>
        /// Private constructor.  The point of this is to limit the number of ways a sgy file can be opened.  Either it must be created from scratch with header properties as
        /// input arguments, or it must be opened from a preexisting file and header information must be inferred from the contents.
        /// </summary>
        /// <param name="stream">The file stream</param>
        /// <param name="textHeader">The ebcdic or ascii encoded (worthless to have different encodings) text header </param>
        /// <param name="textHeaderEncoding">The encoding type for the text header</param>
        /// <param name="header">The file binary header</param>
        /// <param name="endianBitconverter">A converter to accomodate little endian vs big endian bit conversion</param>
        protected SgyFile(FileStream stream, IEnumerable<string> textHeaders, Encoding textHeaderEncoding, FileHeader header, EndianBitConverter endianBitconverter)
        {
            // Reset stream
            stream.Seek(0, SeekOrigin.Begin);
            this.stream = stream;
            this.FileInfo = new FileInfo(stream.Name);
            this.TextHeaders = textHeaders;
            this.TextHeaderEncoding = textHeaderEncoding;
            this.header = header;
            this.BitConverter = endianBitconverter;
            this.binaryReader = endianBitconverter.Endianness == Endianness.BigEndian ? (IBinaryReader)new BigEndianBinaryReader(stream) : (IBinaryReader)new LittleEndianBinaryReader(stream);
            this.binaryWriter = endianBitconverter.Endianness == Endianness.BigEndian ? (IBinaryWriter)new BigEndianBinaryWriter(stream) : (IBinaryWriter)new LittleEndianBinaryWriter(stream);
            this.dataSampleSize = SizeFrom((FormatCode)this.header.DataSampleFormatCode);
            this.TraceCount = (stream.Length - (3600 + ((TextHeaders.Count() - 1) * TextHeaderBytesCount))) / (240 + (BinaryFileHeader.SamplesPerTraceOfFile * this.dataSampleSize));
            switch ((FormatCode)this.header.DataSampleFormatCode)
            {
                case FormatCode.IbmFloatingPoint4:
                    traceDataReadingFunc = (count) => binaryReader.ReadIbmSingles(count);
                    traceDataWritingFunc = (floats) => binaryWriter.WriteIbm(floats);
                    break;
                case FormatCode.IeeeFloatingPoint4:
                    traceDataReadingFunc = (count) => binaryReader.ReadSingles(count);
                    traceDataWritingFunc = (floats) => binaryWriter.Write(floats);
                    break;
                default: throw new NotImplementedException($"Bit converter not implemented yet for format code: {this.header.DataSampleFormatCode}");
            }
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
        /// <returns></returns>
        public static SgyFile Open(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException("File info cannot be null");
            if (fileInfo.Exists == false) throw new FileNotFoundException($"File {fileInfo.FullName} does not exist");
            if (fileInfo.Length < TextHeaderBytesCount + BinaryHeaderBytesCount) throw new ArgumentException($"File {fileInfo.FullName} does not have enough bytes in its content to infer file metadata.  This implies the file is corrupt, or not a real sgy file. Try creating a new sgy file isntead.");

            // Local variables
            byte[] buffer = new byte[65536];
            FileStream fileStream;
            Encoding textHeaderEncoding;
            List<string> textHeaders = new List<string>();
            EndianBitConverter endianBitConverter;
            FileHeader binaryHeader;
            EndianBitConverter bigEBitConverter = new BigEndianBitConverter();
            EndianBitConverter lilEBitConverter = new LittleEndianBitConverter();

            // Get text header bytes and binary header bytes
            fileStream = new FileStream(fileInfo.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            fileStream.Read(buffer, 0, TextHeaderBytesCount + BinaryHeaderBytesCount);

            // Evaluate text encoding (ascii and ebcdic supported by sgy....for no modern day reason)
            textHeaderEncoding = buffer[0] == 'C' ? Encoding.ASCII : EbcdicEncoding.GetEncoding("EBCDIC-US");
            textHeaders.Add(textHeaderEncoding.GetString(buffer, 0, TextHeaderBytesCount));

            // Endianness and format code
            short bigEFormatCode = bigEBitConverter.ToInt16(buffer, 3224);
            short lilEFormatCode = lilEBitConverter.ToInt16(buffer, 3224);
            if (lilEFormatCode >= 0 || lilEFormatCode <= 8) endianBitConverter = new LittleEndianBitConverter();
            if (bigEFormatCode >= 0 || bigEFormatCode <= 8) endianBitConverter = new BigEndianBitConverter();
            else throw new Exception("Cannot infer endianess from the format code");

            // File Binary Header
            binaryHeader = FileHeader.From(buffer, TextHeaderBytesCount, endianBitConverter);

            // Extended text headers
            fileStream.Seek(3600, SeekOrigin.Begin);
            fileStream.Read(buffer, 0, binaryHeader.ExtendedTextHeadersCount * TextHeaderBytesCount);
            for (int i = 0; i < binaryHeader.ExtendedTextHeadersCount; i++)
                textHeaders.Add(textHeaderEncoding.GetString(buffer, i * TextHeaderBytesCount, TextHeaderBytesCount));

            return new SgyFile(fileStream, textHeaders, textHeaderEncoding, binaryHeader, endianBitConverter);
        }

        #region Creation Methods

        /// <summary>
        /// Creates a new Sgy file
        /// </summary>
        /// <param name="fileInfo">The file info</param>
        /// <param name="header">The header information of the file</param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public static SgyFile Create(FileInfo fileInfo, FileHeader header, bool overwrite = false)
        {
            return Create(fileInfo, new string(new char[TextHeaderBytesCount]), EbcdicEncoding.GetEncoding("EBCDIC-US"), header, new BigEndianBitConverter(), overwrite);
        }

        /// <summary>
        /// Creates a new Sgy file
        /// </summary>
        /// <param name="fileInfo">The file info</param>
        /// <param name="textHeader">The ebcdic or ascii encoded (worthless to have different encodings) text header </param>
        /// <param name="header">The file binary header</param>
        /// <param name="overwrite">Should overwrite file on creation?</param>
        /// <returns>An instance of Sgy file</returns>
        public static SgyFile Create(FileInfo fileInfo, string textHeader, FileHeader header, bool overwrite = false)
        {
            return Create(fileInfo, textHeader, EbcdicEncoding.GetEncoding("EBCDIC-US"), header, new BigEndianBitConverter(), overwrite);
        }

        /// <summary>
        /// Creates a new Sgy file
        /// </summary>
        /// <param name="fileInfo">The file info</param>
        /// <param name="textHeader">The ebcdic or ascii encoded (worthless to have different encodings) text header </param>
        /// <param name="textHeaderEncoding">The encoding type for the text header</param>
        /// <param name="header">The file binary header</param>
        /// <param name="overwrite">Should overwrite file on creation?</param>
        /// <returns>An instance of Sgy file</returns>
        public static SgyFile Create(FileInfo fileInfo, string textHeader, Encoding textHeaderEncoding, FileHeader header, bool overwrite = false)
        {
            return Create(fileInfo, textHeader, textHeaderEncoding, header, new BigEndianBitConverter(), overwrite);
        }

        /// <summary>
        /// Creates a new Sgy file
        /// </summary>
        /// <param name="fileInfo">The file info</param>
        /// <param name="textHeader">The ebcdic or ascii encoded (worthless to have different encodings) text header </param>
        /// <param name="header">The file binary header</param>
        /// <param name="endianBitconverter">A converter to accomodate little endian vs big endian bit conversion</param>
        /// <param name="overwrite">Should overwrite file on creation?</param>
        /// <returns>An instance of Sgy file</returns>
        public static SgyFile Create(FileInfo fileInfo, string textHeader, FileHeader header, EndianBitConverter endianBitconverter, bool overwrite = false)
        {
            return Create(fileInfo, textHeader, EbcdicEncoding.GetEncoding("EBCDIC-US"), header, endianBitconverter, overwrite);
        }

        /// <summary>
        /// Creates a new Sgy file
        /// </summary>
        /// <param name="fileInfo">The file info</param>
        /// <param name="textHeader">The ebcdic or ascii encoded (worthless to have different encodings) text header </param>
        /// <param name="textHeaderEncoding">The encoding type for the text header</param>
        /// <param name="header">The file binary header</param>
        /// <param name="endianBitConverter">A converter to accomodate little endian vs big endian bit conversion</param>
        /// <param name="overwrite">Should overwrite file on creation?</param>
        /// <returns>An instance of Sgy file</returns>
        public static SgyFile Create(FileInfo fileInfo, string textHeader, Encoding textHeaderEncoding, FileHeader header, EndianBitConverter endianBitConverter, bool overwrite = false)
        {
            if (fileInfo == null) throw new ArgumentNullException("File info cannot be null");
            if (textHeader == null) throw new ArgumentNullException("Text header cannot be null");
            if (textHeaderEncoding == null) throw new ArgumentNullException("Text header encoding cannot be null");
            if (textHeader.Length > TextHeaderBytesCount) throw new ArgumentException("The text header contains more than 3200 characters");
            if (endianBitConverter == null) throw new ArgumentNullException("Endian bit converter cannot be null");
            if (fileInfo.Exists && overwrite == false) throw new ArgumentException($"File {fileInfo.FullName} already exists. Please delete first, or select overwrite = true");
            if (overwrite == true) fileInfo.Delete();

            var fileStream = new FileStream(fileInfo.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            var conditionedTextHeader = HandleTextHeader(textHeader);
            var textHeaderBytes = textHeaderEncoding.GetBytes(conditionedTextHeader);
            var binHeaderBytes = header.ToBytes(endianBitConverter);
            fileStream.Write(textHeaderBytes, 0, TextHeaderBytesCount);
            fileStream.Write(binHeaderBytes, 0, BinaryHeaderBytesCount);
            fileStream.Flush();
            SgyFile file = new SgyFile(fileStream, new string[] { conditionedTextHeader }, textHeaderEncoding, header, endianBitConverter);
            return file;
        }

        /// <summary>
        /// Creates a new Sgy file
        /// </summary>
        /// <param name="fileInfo">The file info</param>
        /// <param name="textHeader">The ebcdic or ascii encoded (worthless to have different encodings) text header </param>
        /// <param name="textHeaderEncoding">The encoding type for the text header</param>
        /// <param name="header">The file binary header</param>
        /// <param name="endianBitConverter">A converter to accomodate little endian vs big endian bit conversion</param>
        /// <param name="traceHeaders">A set of trace headers to populate the file with</param>
        /// <param name="sampleValue">Populates each trace header with corresponding samplevalues, the count of which defined in the binary header</param>
        /// <returns></returns>
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
        /// <param name="newSgyFileInfo">The file info of the new file to copy to</param>
        /// <param name="oldSgyFileInfo">The old Sgy file info to copy headers from</param>
        /// <param name="sampleValue">The sample value to populate all trace data with</param>
        /// <returns>The new sgy file</returns>
        public static SgyFile CopyPopulated(FileInfo oldSgyFileInfo, FileInfo newSgyFileInfo, float sampleValue = 0.0f)
        {
            if (newSgyFileInfo.Exists == true)
                newSgyFileInfo.Delete();

            newSgyFileInfo = oldSgyFileInfo.FastCopy(newSgyFileInfo);

            SgyFile newSgyFile = SgyFile.Open(newSgyFileInfo);
            BigArray<TraceHeader> allTraceHeaders = newSgyFile.ReadTraceHeaders().ToBigArray();
            float[] buffer = new float[newSgyFile.BinaryFileHeader.SamplesPerTraceOfFile];

            if (sampleValue != 0)
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = sampleValue;

            for (long ti = 0; ti < allTraceHeaders.Length; ti++)
                newSgyFile.Write(new Trace(allTraceHeaders[ti], buffer), ti);
            return newSgyFile;
        }

        #endregion Creation Methods

        #region Read Methods

        /// <summary>
        /// Reads a trace from a specified trace index
        /// </summary>
        /// <param name="traceIndex">The trace index from which to read</param>
        /// <returns>A sgy trace</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Trace ReadTrace(int traceIndex)
        {
            return ReadTrace((long)traceIndex);
        }

        /// <summary>
        /// Reads a trace from a specified trace index
        /// </summary>
        /// <param name="traceIndex">The trace index from which to read</param>
        /// <returns>A sgy trace</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Trace ReadTrace(long traceIndex)
        {
            if (traceIndex > TraceCount) throw new ArgumentException($"Cannot read a trace with trace index: {traceIndex}, when file only contains {TraceCount} traces");

            var dataStartIndex = TextHeaderBytesCount + BinaryHeaderBytesCount + TextHeaderBytesCount * (TextHeaders.Count() - 1);
            var initStreamPosition = dataStartIndex + (240 + BinaryFileHeader.SamplesPerTraceOfFile * this.dataSampleSize) * traceIndex;
            binaryReader.BaseStream.Seek(initStreamPosition, SeekOrigin.Begin);

            var traceHeaderByteArr = binaryReader.ReadBytes(TraceHeaderBytesCount);
            var trHeader = TraceHeader.From(traceHeaderByteArr, BitConverter);
            var traceDataBytesSz = BinaryFileHeader.SamplesPerTraceOfFile * this.dataSampleSize;
            var traceData = traceDataReadingFunc(BinaryFileHeader.SamplesPerTraceOfFile);
            var seismicTrace = new Trace(trHeader, traceData);
            return seismicTrace;
        }

        /// <summary>
        /// Reads a trace header at a specified trace index
        /// </summary>
        /// <param name="traceIndex">The trace index from which to read a trace header</param>
        /// <returns>The trace header</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TraceHeader ReadTraceHeader(int traceIndex)
        {
            return ReadTraceHeader((long)traceIndex);
        }

        /// <summary>
        /// Reads a trace header at a specified trace index
        /// </summary>
        /// <param name="traceIndex">The trace index from which to read a trace header</param>
        /// <returns>The trace header</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TraceHeader ReadTraceHeader(long traceIndex)
        {
            if (traceIndex > TraceCount) throw new ArgumentException($"Cannot read a trace with trace index: {traceIndex}, when file only contains {TraceCount} traces");

            var dataStartIndex = TextHeaderBytesCount + BinaryHeaderBytesCount + TextHeaderBytesCount * (TextHeaders.Count() - 1);
            var initStreamPosition = dataStartIndex + (240 + BinaryFileHeader.SamplesPerTraceOfFile * this.dataSampleSize) * traceIndex;
            this.binaryReader.BaseStream.Seek(initStreamPosition, SeekOrigin.Begin);

            var traceHeaderByteArr = this.binaryReader.ReadBytes(TraceHeaderBytesCount);
            var trHeader = TraceHeader.From(traceHeaderByteArr, BitConverter);
            return trHeader;
        }

        /// <summary>
        /// Reads all trace headers in this file
        /// </summary>
        /// <returns>An IEnumerable of Trace Headers</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TraceHeader> ReadTraceHeaders()
        {
            return ReadTraceHeaders(0, TraceCount);
        }

        /// <summary>
        /// Read "n" trace headers from a starting position
        /// </summary>
        /// <param name="startTraceIndex">The trace index to start reading from</param>
        /// <param name="traceCount">The number of sequential trace headers to read</param>
        /// <returns>A set of trace headers read sequentially</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TraceHeader> ReadTraceHeaders(long startTraceIndex, long traceCount)
        {
            if (startTraceIndex > TraceCount) throw new ArgumentException($"Cannot read a trace header with trace index: {startTraceIndex}, when file only contains {TraceCount} traces");
            var streamLen = binaryReader.BaseStream.Length;

            int traceDataBytesSz = BinaryFileHeader.SamplesPerTraceOfFile * dataSampleSize;
            var dataStartIndex = TextHeaderBytesCount + BinaryHeaderBytesCount + TextHeaderBytesCount * (TextHeaders.Count() - 1);
            var initStreamPosition = dataStartIndex + (240 + BinaryFileHeader.SamplesPerTraceOfFile * dataSampleSize) * startTraceIndex;
            binaryReader.BaseStream.Seek(initStreamPosition, SeekOrigin.Begin);
            var streamPosition = initStreamPosition;

            BigArray<TraceHeader> traceHeaders = new BigArray<TraceHeader>(traceCount);
            for (long tid = 0; tid < traceCount && (streamPosition < streamLen); tid++)
            {
                var traceHeaderByteArr = binaryReader.ReadBytes(TraceHeaderBytesCount);
                var trHeader = TraceHeader.From(traceHeaderByteArr, BitConverter);
                binaryReader.BaseStream.Seek(traceDataBytesSz, SeekOrigin.Current);
                streamPosition += 240 + traceDataBytesSz;
                traceHeaders[tid] = trHeader;
            }
            return traceHeaders;
        }

        /// <summary>
        /// Read all traces (careful if there a memory considerations)
        /// </summary>
        /// <returns>All traces</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Trace> ReadTraces()
        {
            return ReadTraces(0, TraceCount);
        }

        /// <summary>
        /// Read "n" traces from a starting position
        /// </summary>
        /// <param name="startTraceIndex">The trace index to start reading from</param>
        /// <param name="traceCount">The number of sequential traces to read</param>
        /// <returns>A set of traces read sequentially</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Trace> ReadTraces(long startTraceIndex, long traceCount)
        {
            if (startTraceIndex > TraceCount) throw new ArgumentException($"Cannot read a trace with trace index: {startTraceIndex}, when file only contains {TraceCount} traces");
            var initStreamPosition = (3600 + ((TextHeaders.Count() - 1) * TextHeaderBytesCount)) + (240 + BinaryFileHeader.SamplesPerTraceOfFile * dataSampleSize) * startTraceIndex;

            binaryReader.BaseStream.Seek(initStreamPosition, SeekOrigin.Begin);

            BigList<Trace> traces = new BigList<Trace>(traceCount);
            for (long tid = 0; tid < traceCount; tid++)
            {
                var traceHeaderByteArr = binaryReader.ReadBytes(TraceHeaderBytesCount);
                var traceData = traceDataReadingFunc(BinaryFileHeader.SamplesPerTraceOfFile);// _binaryreader.ReadIbmSingles(Header.SamplesPerTraceOfFile);
                var trHeader = TraceHeader.From(traceHeaderByteArr, BitConverter);
                var seismicTrace = new Trace(trHeader, traceData);
                traces.Add(seismicTrace);
            }
            return traces;
        }

        #endregion Read Methods

        #region Write Methods

        /// <summary>
        /// Appends a trace to the end of the file
        /// </summary>
        /// <param name="trace">The trace to append</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(Trace trace)
        {
            if (trace == null) throw new ArgumentNullException("Trace cannot be null when appending");
            if (trace.Data == null) throw new ArgumentNullException("Trace data cannot be null when appending");
            if (trace.Data.Length != BinaryFileHeader.SamplesPerTraceOfFile) throw new ArgumentException($"Trace (Length:{trace.Data.Length}) must have {BinaryFileHeader.SamplesPerTraceOfFile}");

            binaryWriter.BaseStream.Seek(0, SeekOrigin.End);
            var traceHeaderBytes = trace.Header.ToBytes(this.BitConverter);
            binaryWriter.Write(traceHeaderBytes);
            traceDataWritingFunc(trace.Data);
            TraceCount++;
        }

        /// <summary>
        /// Appends a set of traces to the end of the file
        /// </summary>
        /// <param name="traces">The traces to append</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(IEnumerable<Trace> traces)
        {
            binaryWriter.BaseStream.Seek(0, SeekOrigin.End);
            byte[] traceHeaderBuffer = new byte[TraceHeaderBytesCount];

            foreach (var trace in traces)
            {
                trace.Header.ToBytes(traceHeaderBuffer, 0, this.BitConverter);
                binaryWriter.Write(traceHeaderBuffer);
                traceDataWritingFunc(trace.Data);
                TraceCount++;
            }
        }

        /// <summary>
        /// Writes a trace at a specified trace index (assumes constant trace length file)
        /// </summary>
        /// <param name="trace">The trace to write</param>
        /// <param name="traceIndex">The trace index at which to write</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Trace trace, int traceIndex)
        {
            Write(trace, (long)traceIndex);
        }

        /// <summary>
        /// Writes a trace at a specified trace index (assumes constant trace length file)
        /// </summary>
        /// <param name="trace">The trace to write</param>
        /// <param name="traceIndex">The trace index at which to write</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Trace trace, long traceIndex)
        {
            if (trace == null) throw new ArgumentNullException("Trace cannot be null when appending");
            if (trace.Data == null) throw new ArgumentNullException("Trace data cannot be null when appending");
            if (trace.Data.Length != BinaryFileHeader.SamplesPerTraceOfFile) throw new ArgumentException($"Trace (Length:{trace.Data.Length}) must have {BinaryFileHeader.SamplesPerTraceOfFile}");

            var traceStartBytePosition = (TextHeaders.Count() * TextHeaderBytesCount) + BinaryHeaderBytesCount + (traceIndex * (TraceHeaderBytesCount + (BinaryFileHeader.SamplesPerTraceOfFile * dataSampleSize)));
            binaryWriter.BaseStream.Seek(traceStartBytePosition, SeekOrigin.Begin);
            var traceHeaderBytes = trace.Header.ToBytes(this.BitConverter);
            binaryWriter.Write(traceHeaderBytes);
            traceDataWritingFunc(trace.Data);
        }

        /// <summary>
        /// Writes a set of traces at a specified trace index (assumes constant trace length file)
        /// </summary>
        /// <param name="traces">The traces to write</param>
        /// <param name="startTraceIndex">The strarting trace index at which to write</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IEnumerable<Trace> traces, int startTraceIndex)
        {
            Write(traces, (long)startTraceIndex);
        }

        /// <summary>
        /// Writes a set of traces at a specified trace index (assumes constant trace length file)
        /// </summary>
        /// <param name="traces">The traces to write</param>
        /// <param name="startTraceIndex">The strarting trace index at which to write</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IEnumerable<Trace> traces, long startTraceIndex)
        {
            var traceStartBytePosition = (TextHeaders.Count() * TextHeaderBytesCount) + BinaryHeaderBytesCount + (startTraceIndex * (TraceHeaderBytesCount + (BinaryFileHeader.SamplesPerTraceOfFile * dataSampleSize)));
            binaryWriter.BaseStream.Seek(traceStartBytePosition, SeekOrigin.Begin);
            foreach (var trace in traces)
            {
                var traceHeaderBytes = trace.Header.ToBytes(this.BitConverter);
                binaryWriter.Write(traceHeaderBytes);
                traceDataWritingFunc(trace.Data);
            }
        }

        /// <summary>
        /// Writes an array of floats to file
        /// </summary>
        /// <param name="traceData">The array of data representing a trace's data</param>
        /// <param name="traceIndex">The index of the trace to write data to</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float[] traceData, int traceIndex)
        {
            Write(traceData, (long)traceIndex);
        }

        /// <summary>
        /// Writes an array of floats to file
        /// </summary>
        /// <param name="traceData">The array of data representing a trace's data</param>
        /// <param name="traceIndex">The index of the trace to write data to</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float[] traceData, long traceIndex)
        {
            if (traceData == null) throw new ArgumentNullException("Trace data cannot be null when appending");
            if (traceData.Length != BinaryFileHeader.SamplesPerTraceOfFile) throw new ArgumentException($"Trace (Length:{traceData.Length}) must have {BinaryFileHeader.SamplesPerTraceOfFile}");

            var traceStartBytePosition = (TextHeaders.Count() * TextHeaderBytesCount) + BinaryHeaderBytesCount + (traceIndex * (TraceHeaderBytesCount + (BinaryFileHeader.SamplesPerTraceOfFile * dataSampleSize)));
            binaryWriter.BaseStream.Seek(traceStartBytePosition + TraceHeaderBytesCount, SeekOrigin.Begin);
            traceDataWritingFunc(traceData);
        }

        /// <summary>
        /// Writes a set of arrays of floats to file
        /// </summary>
        /// <param name="traceDatas">The set of arrays of data representing a trace's data</param>
        /// <param name="startTraceIndex">The index of the trace to write data to</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IEnumerable<float[]> traceDatas, int startTraceIndex)
        {
            Write(traceDatas, (long)startTraceIndex);
        }

        /// <summary>
        /// Writes a set of arrays of floats to file
        /// </summary>
        /// <param name="traceDatas">The set of arrays of data representing a trace's data</param>
        /// <param name="startTraceIndex">The index of the trace to write data to</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IEnumerable<float[]> traceDatas, long startTraceIndex)
        {
            var traceStartBytePosition = (TextHeaders.Count() * TextHeaderBytesCount) + BinaryHeaderBytesCount + (startTraceIndex * (TraceHeaderBytesCount + (BinaryFileHeader.SamplesPerTraceOfFile * dataSampleSize)));
            binaryWriter.BaseStream.Seek(traceStartBytePosition, SeekOrigin.Begin);
            foreach (float[] traceData in traceDatas)
            {
                binaryWriter.BaseStream.Seek(TraceHeaderBytesCount, SeekOrigin.Current);
                traceDataWritingFunc(traceData);
            }
        }

        /// <summary>
        /// Writes a set of trace headers at a specified trace index (assumes constant trace length file and the trace exists)
        /// </summary>
        /// <param name="traceHeader">The trace header to write</param>
        /// <param name="traceIndex">The strarting trace index at which to write</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(TraceHeader traceHeader, int traceIndex)
        {
            Write(traceHeader, (long)traceIndex);
        }

        /// <summary>
        /// Writes a set of trace headers at a specified trace index (assumes constant trace length file and the trace exists)
        /// </summary>
        /// <param name="traceHeader">The trace header to write</param>
        /// <param name="traceIndex">The strarting trace index at which to write</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(TraceHeader traceHeader, long traceIndex)
        {
            if (TraceCount < traceIndex) throw new ArgumentException("There are fewer traces in the file then intended trace headers to write");

            var traceStartBytePosition = (TextHeaders.Count() * TextHeaderBytesCount) + BinaryHeaderBytesCount + (traceIndex * (TraceHeaderBytesCount + (BinaryFileHeader.SamplesPerTraceOfFile * dataSampleSize)));
            binaryWriter.BaseStream.Seek(traceStartBytePosition, SeekOrigin.Begin);
            var traceHeaderBytes = traceHeader.ToBytes(this.BitConverter);
            binaryWriter.Write(traceHeaderBytes);
        }

        /// <summary>
        /// Writes a set of trace headers at a specified trace index (assumes constant trace length file and the trace exists)
        /// </summary>
        /// <param name="traceHeaders">The trace headers to write</param>
        /// <param name="startTraceIndex">The strarting trace index at which to write</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IEnumerable<TraceHeader> traceHeaders, int startTraceIndex)
        {
            Write(traceHeaders, (long)startTraceIndex);
        }

        /// <summary>
        /// Writes a set of trace headers at a specified trace index (assumes constant trace length file and the trace exists)
        /// </summary>
        /// <param name="traceHeaders">The trace headers to write</param>
        /// <param name="startTraceIndex">The strarting trace index at which to write</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(IEnumerable<TraceHeader> traceHeaders, long startTraceIndex)
        {
            if (TraceCount < traceHeaders.Count() + startTraceIndex) throw new ArgumentException("There are fewer traces in the file then intended trace headers to write");

            var traceStartBytePosition = (TextHeaders.Count() * TextHeaderBytesCount) + BinaryHeaderBytesCount + (startTraceIndex * (TraceHeaderBytesCount + (BinaryFileHeader.SamplesPerTraceOfFile * dataSampleSize)));
            binaryWriter.BaseStream.Seek(traceStartBytePosition, SeekOrigin.Begin);
            foreach (var traceHeader in traceHeaders)
            {
                var traceHeaderBytes = traceHeader.ToBytes(this.BitConverter);
                binaryWriter.Write(traceHeaderBytes);
                binaryWriter.BaseStream.Seek(BinaryFileHeader.SamplesPerTraceOfFile * dataSampleSize, SeekOrigin.Current);
            }
        }

        #endregion Write Methods

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
        /// <param name="textHeader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string HandleTextHeader(string textHeader)
        {
            // if header already contains "C" prepended strings
            if (textHeader[0] == 'C' && (textHeader[1] == '1' || textHeader[2] == '1'))
                return textHeader.PadRight(40 * 77);

            var textHeaderPadded = textHeader.PadRight(3200);

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
            this.binaryReader.Dispose();
        }
    }
}
 
using System;
using Seismic.SegyFileIo;
using Utility.Io.Serialization;

namespace Hess.Seismic.SegyFileIo
{
    /// <summary>
    /// A seismic file "binary" header
    /// </summary>
    public struct FileHeader
    {
        public FileHeader(short sampleRateInMicroUnits, short sampleCountPerTrace)
        {
            JobId = 0;
            LineNum = 0;
            Reelnum = 0;
            DataTracesPerRecord = 0;
            AuxTracesPerRecord = 0;
            SampleIntervalOfFileMicrosec = sampleRateInMicroUnits;
            SampleIntervalOrigRecordingMicrosec = sampleRateInMicroUnits;
            SamplesPerTraceOfFile = sampleCountPerTrace;
            SamplesPerTraceOrigRecording = sampleCountPerTrace;
            DataSampleFormatCode = (short)FormatCode.IbmFloatingPoint4;
            CdpFold = 0;
            TraceSortingCode = 0;
            VerticalSumCode = 0;
            SweepFrequencyStart = 0;
            SweepFrequencyEnd = 0;
            SweepLengthMs = 0;
            SweepType = 0;
            TraceNumSweepChannel = 0;
            SweepTraceTaperLengthStartMs = 0;
            SweepTraceTaperLengthEndMs = 0;
            SweepTaperType = 0;
            CorrelatedDataTraces = 0;
            BinaryGainRecovered = 0;
            AplitudeRecovery = 0;
            UnitSystem = 0;
            ImpulseSignal = 0;
            VibratoryPolarityCode = 0;
            SegyFormatRevisionNum = 1;
            FixedLengthTraceFlag = 1;
            ExtendedTextHeadersCount = 0;
        }
        
        public int JobId;
        public int LineNum;
        public int Reelnum;
        public short DataTracesPerRecord;
        public short AuxTracesPerRecord;
        public short SampleIntervalOfFileMicrosec;
        public short SampleIntervalOrigRecordingMicrosec;
        public short SamplesPerTraceOfFile;
        public short SamplesPerTraceOrigRecording;
        public short DataSampleFormatCode;
        public short CdpFold;
        public short TraceSortingCode;
        public short VerticalSumCode;
        public short SweepFrequencyStart;
        public short SweepFrequencyEnd;
        public short SweepLengthMs;
        public short SweepType;
        public short TraceNumSweepChannel;
        public short SweepTraceTaperLengthStartMs;
        public short SweepTraceTaperLengthEndMs;
        public short SweepTaperType;
        public short CorrelatedDataTraces;
        public short BinaryGainRecovered;
        public short AplitudeRecovery;
        public short UnitSystem;
        public short ImpulseSignal;
        public short VibratoryPolarityCode;
        public short SegyFormatRevisionNum;
        public short FixedLengthTraceFlag;
        public short ExtendedTextHeadersCount;

        public static FileHeader From(byte[] bytes, EndianBitConverter bitConverter)
        {
            return From(bytes, 0, bitConverter);
        }

        public static FileHeader From(byte[] bytes, int offset, EndianBitConverter bitConverter)
        {
            Func<byte[], int, short> bytesToInt16 = (b, o) => bitConverter.ToInt16(b, offset + o);
            Func<byte[], int, int> bytesToInt32 = (b, o) => bitConverter.ToInt32(b, offset + o);
            Func<byte[], int, uint> bytesToUInt32 = (b, o) => bitConverter.ToUInt32(b, offset + o);

            var ns16_20 = bytesToInt16(bytes, 20);
            var uns32_18 = bytesToUInt32(bytes, 18);

            short ns = ns16_20 >= 0 ? ns16_20 : Convert.ToInt16(uns32_18);

            var result = new FileHeader();
            result.JobId = bytesToInt32(bytes, 0);
            result.LineNum = bytesToInt32(bytes, 4);
            result.Reelnum = bytesToInt32(bytes, 8);
            result.DataTracesPerRecord = bytesToInt16(bytes, 12);
            result.AuxTracesPerRecord = bytesToInt16(bytes, 14);
            result.SampleIntervalOfFileMicrosec = bytesToInt16(bytes, 16);
            result.SampleIntervalOrigRecordingMicrosec = bytesToInt16(bytes, 18);
            result.SamplesPerTraceOfFile = ns;
            result.SamplesPerTraceOrigRecording = bytesToInt16(bytes, 22);
            result.DataSampleFormatCode = bytesToInt16(bytes, 24);
            result.CdpFold = bytesToInt16(bytes, 26);
            result.TraceSortingCode = bytesToInt16(bytes, 28);
            result.VerticalSumCode = bytesToInt16(bytes, 30);
            result.SweepFrequencyStart = bytesToInt16(bytes, 32);
            result.SweepFrequencyEnd = bytesToInt16(bytes, 34);
            result.SweepLengthMs = bytesToInt16(bytes, 36);
            result.SweepType = bytesToInt16(bytes, 38);
            result.TraceNumSweepChannel = bytesToInt16(bytes, 40);
            result.SweepTraceTaperLengthStartMs = bytesToInt16(bytes, 42);
            result.SweepTraceTaperLengthEndMs = bytesToInt16(bytes, 44);
            result.SweepTaperType = bytesToInt16(bytes, 46);
            result.CorrelatedDataTraces = bytesToInt16(bytes, 48);
            result.BinaryGainRecovered = bytesToInt16(bytes, 50);
            result.AplitudeRecovery = bytesToInt16(bytes, 52);
            result.UnitSystem = bytesToInt16(bytes, 54);
            result.ImpulseSignal = bytesToInt16(bytes, 56);
            result.VibratoryPolarityCode = bytesToInt16(bytes, 58);
            result.SegyFormatRevisionNum = bytesToInt16(bytes, 300);
            result.FixedLengthTraceFlag = bytesToInt16(bytes, 302);
            result.ExtendedTextHeadersCount = bytesToInt16(bytes, 304);
            if (result.SegyFormatRevisionNum > 0 || result.SegyFormatRevisionNum == 5) result.SegyFormatRevisionNum = 1;
            return result;
        }

        public byte[] ToBytes(EndianBitConverter bitConverter)
        {
            byte[] buffer = new byte[SgyFile.BinaryHeaderBytesCount];
            ToBytes(buffer, 0, bitConverter);
            return buffer;
        }

        public void ToBytes(byte[] buffer, int offset, EndianBitConverter bitConverter)
        {
            if (buffer == null) throw new ArgumentNullException("byte buffer cannot be null");
            if (buffer.Length < offset + 400) throw new ArgumentException("buffer length is too small to write bytes from header to it");
            
            bitConverter.CopyBytes(JobId, buffer, offset);
            bitConverter.CopyBytes(LineNum, buffer, offset + 4);
            bitConverter.CopyBytes(Reelnum, buffer, offset + 8);
            bitConverter.CopyBytes(DataTracesPerRecord, buffer, offset + 12);
            bitConverter.CopyBytes(AuxTracesPerRecord, buffer, offset + 14);
            bitConverter.CopyBytes(SampleIntervalOfFileMicrosec, buffer, offset + 16);
            bitConverter.CopyBytes(SampleIntervalOrigRecordingMicrosec, buffer, offset + 18);
            bitConverter.CopyBytes(SamplesPerTraceOfFile, buffer, offset + 20);
            bitConverter.CopyBytes(SamplesPerTraceOrigRecording, buffer, offset + 22);
            bitConverter.CopyBytes(DataSampleFormatCode, buffer, offset + 24);
            bitConverter.CopyBytes(CdpFold, buffer, offset + 26);
            bitConverter.CopyBytes(TraceSortingCode, buffer, offset + 28);
            bitConverter.CopyBytes(VerticalSumCode, buffer, offset + 30);
            bitConverter.CopyBytes(SweepFrequencyStart, buffer, offset + 32);
            bitConverter.CopyBytes(SweepFrequencyEnd, buffer, offset + 34);
            bitConverter.CopyBytes(SweepLengthMs, buffer, offset + 36);
            bitConverter.CopyBytes(SweepType, buffer, offset + 38);
            bitConverter.CopyBytes(TraceNumSweepChannel, buffer, offset + 40);
            bitConverter.CopyBytes(SweepTraceTaperLengthStartMs, buffer, offset + 42);
            bitConverter.CopyBytes(SweepTraceTaperLengthEndMs, buffer, offset + 44);
            bitConverter.CopyBytes(SweepTaperType, buffer, offset + 46);
            bitConverter.CopyBytes(CorrelatedDataTraces, buffer, offset + 48);
            bitConverter.CopyBytes(BinaryGainRecovered, buffer, offset + 50);
            bitConverter.CopyBytes(AplitudeRecovery, buffer, offset + 52);
            bitConverter.CopyBytes(UnitSystem, buffer, offset + 54);
            bitConverter.CopyBytes(ImpulseSignal, buffer, offset + 56);
            bitConverter.CopyBytes(VibratoryPolarityCode, buffer, offset + 58);
            
            bitConverter.CopyBytes(SegyFormatRevisionNum, buffer, offset + 300);
            bitConverter.CopyBytes(FixedLengthTraceFlag, buffer, offset + 302);
            bitConverter.CopyBytes(ExtendedTextHeadersCount, buffer, offset + 304);
        }
    }
}

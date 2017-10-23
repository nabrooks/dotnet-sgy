using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Serialization;

namespace Seismic.SegyFileIo
{
    /// <summary>
    /// Describes the file header of a Segy file. Contains properties of the file. By Segy standard, should be the first 400 bytes of the file.
    /// </summary>
    public class SegyFileHeader : IEquatable<SegyFileHeader>
    {
        public int JobId { get; set; }
        public int LineNum { get; set; }
        public int Reelnum { get; set; }
        public short DataTracesPerRecord { get; set; }
        public short AuxTracesPerRecord { get; set; }
        public short SampleIntervalOfFileMicrosec { get; set; }
        public short SampleIntervalOrigRecordingMicrosec { get; set; }
        public short SamplesPerTraceOfFile { get; set; }
        public short SamplesPerTraceOrigRecording { get; set; }
        public short DataSampleFormatCode { get; set; } = (short)FormatCode.IbmFloatingPoint4;
        public short CdpFold { get; set; }
        public short TraceSortingCode { get; set; }
        public short VerticalSumCode { get; set; }
        public short SweepFrequencyStart { get; set; }
        public short SweepFrequencyEnd { get; set; }
        public short SweepLengthMs { get; set; }
        public short SweepType { get; set; }
        public short TraceNumSweepChannel { get; set; }
        public short SweepTraceTaperLengthStartMs { get; set; }
        public short SweepTraceTaperLengthEndMs { get; set; }
        public short SweepTaperType { get; set; }
        public short CorrelatedDataTraces { get; set; }
        public short BinaryGainRecovered { get; set; }
        public short AplitudeRecovery { get; set; }
        public short UnitSystem { get; set; }
        public short ImpulseSignal { get; set; }
        public short VibratoryPolarityCode { get; set; }

        public short SegyFormatRevisionNum { get; set; } = 1;
        public short FixedLengthTraceFlag { get; set; } = 1;
        public short ExtendedTextHeadersCount { get; set; }

        /// <summary>
        /// Default Ctor
        /// </summary>
        public SegyFileHeader()
        {

        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sampleRateInMicroUnits">The vertical sample rate of this file in micro units (microseconds if in time, micro distance if in distance).</param>
        /// <param name="sampleCountPerTrace">The number of samples per trace in this file.</param>
        public SegyFileHeader(short sampleRateInMicroUnits, short sampleCountPerTrace)
        {
            SamplesPerTraceOfFile = sampleCountPerTrace;
            SamplesPerTraceOrigRecording = sampleCountPerTrace;
            SampleIntervalOfFileMicrosec = sampleRateInMicroUnits;
            SampleIntervalOrigRecordingMicrosec = sampleRateInMicroUnits;
        }

        /// <summary>
        /// Copy Ctor
        /// </summary>
        /// <param name="hdr"></param>
        protected SegyFileHeader(SegyFileHeader hdr)
        {
            JobId = hdr.JobId;
            LineNum = hdr.LineNum;
            Reelnum = hdr.Reelnum;
            DataTracesPerRecord = hdr.DataTracesPerRecord;
            AuxTracesPerRecord = hdr.AuxTracesPerRecord;
            SampleIntervalOfFileMicrosec = hdr.SampleIntervalOfFileMicrosec;
            SampleIntervalOrigRecordingMicrosec = hdr.SampleIntervalOrigRecordingMicrosec;
            SamplesPerTraceOfFile = hdr.SamplesPerTraceOfFile;
            SamplesPerTraceOrigRecording = hdr.SamplesPerTraceOrigRecording;
            DataSampleFormatCode = hdr.DataSampleFormatCode;
            CdpFold = hdr.CdpFold;
            TraceSortingCode = hdr.TraceSortingCode;
            VerticalSumCode = hdr.VerticalSumCode;
            SweepFrequencyStart = hdr.SweepFrequencyStart;
            SweepFrequencyEnd = hdr.SweepFrequencyEnd;
            SweepLengthMs = hdr.SweepLengthMs;
            SweepType = hdr.SweepType;
            TraceNumSweepChannel = hdr.TraceNumSweepChannel;
            SweepTraceTaperLengthStartMs = hdr.SweepTraceTaperLengthStartMs;
            SweepTraceTaperLengthEndMs = hdr.SweepTraceTaperLengthEndMs;
            SweepTaperType = hdr.SweepTaperType;
            CorrelatedDataTraces = hdr.CorrelatedDataTraces;
            BinaryGainRecovered = hdr.BinaryGainRecovered;
            AplitudeRecovery = hdr.AplitudeRecovery;
            UnitSystem = hdr.UnitSystem;
            ImpulseSignal = hdr.ImpulseSignal;
            VibratoryPolarityCode = hdr.VibratoryPolarityCode;
            SegyFormatRevisionNum = hdr.SegyFormatRevisionNum;
            FixedLengthTraceFlag = hdr.FixedLengthTraceFlag;
            ExtendedTextHeadersCount = hdr.ExtendedTextHeadersCount;
        }

        /// <summary>
        /// Converts a 400 element byte array into a <see cref="SegyFileHeader"/>
        /// </summary>
        /// <param name="bytes">The data intended to convert to a <see cref="SegyFileHeader"/>. Must have 400 or more elements.</param>
        /// <param name="isLittleEndian">Describes what deserialization configuration should be used to properly convert the byte array into a <see cref="SegyFileHeader"/></param>
        /// <returns>A Segy file header</returns>
        public static SegyFileHeader From(byte[] bytes, bool isLittleEndian)
        {
            CodeContract.Requires<NullReferenceException>(bytes != null,"byte array must not be null.");
            CodeContract.Requires<ArgumentException>(bytes.Length >= 400,"byte array length must be greater than or equal to 400 elements.");

            Func<byte[], int, short> bytesToInt16 = isLittleEndian
                ? (Func<byte[], int, short>)(BitConverter.ToInt16)
                : (Func<byte[], int, short>)(IbmConverter.ToInt16);

            Func<byte[], int, int> bytesToInt32 = isLittleEndian
                ? (Func<byte[], int, int>)(BitConverter.ToInt32)
                : (Func<byte[], int, int>)(IbmConverter.ToInt32);

            Func<byte[], int, uint> bytesToUInt32 = isLittleEndian
                ? (Func<byte[], int, uint>)(BitConverter.ToUInt32)
                : (Func<byte[], int, uint>)(IbmConverter.ToUInt32);

            var ns16_20 = bytesToInt16(bytes, 20);
            var uns32_18 = bytesToUInt32(bytes, 18);

            short ns = ns16_20 >= 0 ? ns16_20 : Convert.ToInt16(uns32_18);

            var result = new SegyFileHeader();
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

        /// <summary>
        /// Serializes this into a byte array of 400 elements via big endian encoding.
        /// </summary>
        /// <returns>A byte array of 400 elements.</returns>
        public byte[] GetBytes()
        {
            Func<int, byte[]> intToBytes = IbmConverter.GetBytes;
            Func<short, byte[]> shortToBytes = IbmConverter.GetBytes;
            Func<ushort, byte[]> uShortToBytes = IbmConverter.GetBytes;

            byte[] bytes = new byte[0];
            bytes = bytes.Concat(intToBytes(JobId));
            bytes = bytes.Concat(intToBytes(LineNum));
            bytes = bytes.Concat(intToBytes(Reelnum));
            bytes = bytes.Concat(shortToBytes(DataTracesPerRecord));
            bytes = bytes.Concat(shortToBytes(AuxTracesPerRecord));
            bytes = bytes.Concat(shortToBytes(SampleIntervalOfFileMicrosec));
            bytes = bytes.Concat(shortToBytes(SampleIntervalOrigRecordingMicrosec));
            bytes = bytes.Concat(shortToBytes(SamplesPerTraceOfFile));
            bytes = bytes.Concat(shortToBytes(SamplesPerTraceOrigRecording));
            bytes = bytes.Concat(shortToBytes(DataSampleFormatCode));
            bytes = bytes.Concat(shortToBytes(CdpFold));
            bytes = bytes.Concat(shortToBytes(TraceSortingCode));
            bytes = bytes.Concat(shortToBytes(VerticalSumCode));
            bytes = bytes.Concat(shortToBytes(SweepFrequencyStart));
            bytes = bytes.Concat(shortToBytes(SweepFrequencyEnd));
            bytes = bytes.Concat(shortToBytes(SweepLengthMs));
            bytes = bytes.Concat(shortToBytes(SweepType));
            bytes = bytes.Concat(shortToBytes(TraceNumSweepChannel));
            bytes = bytes.Concat(shortToBytes(SweepTraceTaperLengthStartMs));
            bytes = bytes.Concat(shortToBytes(SweepTraceTaperLengthEndMs));
            bytes = bytes.Concat(shortToBytes(SweepTaperType));
            bytes = bytes.Concat(shortToBytes(CorrelatedDataTraces));
            bytes = bytes.Concat(shortToBytes(BinaryGainRecovered));
            bytes = bytes.Concat(shortToBytes(AplitudeRecovery));
            bytes = bytes.Concat(shortToBytes(UnitSystem));
            bytes = bytes.Concat(shortToBytes(ImpulseSignal));
            bytes = bytes.Concat(shortToBytes(VibratoryPolarityCode));

            var segyRevNum = shortToBytes(SegyFormatRevisionNum);
            var traceFlag = shortToBytes(FixedLengthTraceFlag);
            var extTxtHdrCt = shortToBytes(ExtendedTextHeadersCount);

            byte[] result = new byte[400];
            for (int i = 0; i < bytes.Length; i++)
            {
                result[i] = bytes[i];
            }

            for (int i = 0; i < 2; i++)
            {
                result[300 + i] = segyRevNum[i];
                result[302 + i] = traceFlag[i];
                result[304 + i] = extTxtHdrCt[i];
            }
            return result;
        }
  
        /// <summary>
        /// Creates a "deep" clone of this object.
        /// </summary>
        /// <returns></returns>
        public SegyFileHeader DeepClone() => new SegyFileHeader(this);
       
        #region Operator Overloads

        public static bool operator ==(SegyFileHeader a, SegyFileHeader b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static bool operator !=(SegyFileHeader a, SegyFileHeader b)
        {
            return !(a == b);
        }

        #endregion Operator Overloads

        #region IEquatable<BinaryHeader> members

        public bool Equals(SegyFileHeader other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return JobId == other.JobId && LineNum == other.LineNum && Reelnum == other.Reelnum && DataTracesPerRecord == other.DataTracesPerRecord && AuxTracesPerRecord == other.AuxTracesPerRecord && SampleIntervalOfFileMicrosec == other.SampleIntervalOfFileMicrosec && SampleIntervalOrigRecordingMicrosec == other.SampleIntervalOrigRecordingMicrosec && SamplesPerTraceOfFile == other.SamplesPerTraceOfFile && SamplesPerTraceOrigRecording == other.SamplesPerTraceOrigRecording && DataSampleFormatCode == other.DataSampleFormatCode && CdpFold == other.CdpFold && TraceSortingCode == other.TraceSortingCode && VerticalSumCode == other.VerticalSumCode && SweepFrequencyStart == other.SweepFrequencyStart && SweepFrequencyEnd == other.SweepFrequencyEnd && SweepLengthMs == other.SweepLengthMs && SweepType == other.SweepType && TraceNumSweepChannel == other.TraceNumSweepChannel && SweepTraceTaperLengthStartMs == other.SweepTraceTaperLengthStartMs && SweepTraceTaperLengthEndMs == other.SweepTraceTaperLengthEndMs && SweepTaperType == other.SweepTaperType && CorrelatedDataTraces == other.CorrelatedDataTraces && BinaryGainRecovered == other.BinaryGainRecovered && AplitudeRecovery == other.AplitudeRecovery && UnitSystem == other.UnitSystem && ImpulseSignal == other.ImpulseSignal && VibratoryPolarityCode == other.VibratoryPolarityCode && SegyFormatRevisionNum == other.SegyFormatRevisionNum && FixedLengthTraceFlag == other.FixedLengthTraceFlag && ExtendedTextHeadersCount == other.ExtendedTextHeadersCount;
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SegyFileHeader)obj);
        }

        /// <summary>
        /// Gets a hash code for this file binary header
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = JobId;
                hashCode = (hashCode * 397) ^ LineNum;
                hashCode = (hashCode * 397) ^ Reelnum;
                hashCode = (hashCode * 397) ^ DataTracesPerRecord.GetHashCode();
                hashCode = (hashCode * 397) ^ AuxTracesPerRecord.GetHashCode();
                hashCode = (hashCode * 397) ^ SampleIntervalOfFileMicrosec.GetHashCode();
                hashCode = (hashCode * 397) ^ SampleIntervalOrigRecordingMicrosec.GetHashCode();
                hashCode = (hashCode * 397) ^ SamplesPerTraceOfFile.GetHashCode();
                hashCode = (hashCode * 397) ^ SamplesPerTraceOrigRecording.GetHashCode();
                hashCode = (hashCode * 397) ^ DataSampleFormatCode.GetHashCode();
                hashCode = (hashCode * 397) ^ CdpFold.GetHashCode();
                hashCode = (hashCode * 397) ^ TraceSortingCode.GetHashCode();
                hashCode = (hashCode * 397) ^ VerticalSumCode.GetHashCode();
                hashCode = (hashCode * 397) ^ SweepFrequencyStart.GetHashCode();
                hashCode = (hashCode * 397) ^ SweepFrequencyEnd.GetHashCode();
                hashCode = (hashCode * 397) ^ SweepLengthMs.GetHashCode();
                hashCode = (hashCode * 397) ^ SweepType.GetHashCode();
                hashCode = (hashCode * 397) ^ TraceNumSweepChannel.GetHashCode();
                hashCode = (hashCode * 397) ^ SweepTraceTaperLengthStartMs.GetHashCode();
                hashCode = (hashCode * 397) ^ SweepTraceTaperLengthEndMs.GetHashCode();
                hashCode = (hashCode * 397) ^ SweepTaperType.GetHashCode();
                hashCode = (hashCode * 397) ^ CorrelatedDataTraces.GetHashCode();
                hashCode = (hashCode * 397) ^ BinaryGainRecovered.GetHashCode();
                hashCode = (hashCode * 397) ^ AplitudeRecovery.GetHashCode();
                hashCode = (hashCode * 397) ^ UnitSystem.GetHashCode();
                hashCode = (hashCode * 397) ^ ImpulseSignal.GetHashCode();
                hashCode = (hashCode * 397) ^ VibratoryPolarityCode.GetHashCode();
                hashCode = (hashCode * 397) ^ SegyFormatRevisionNum.GetHashCode();
                hashCode = (hashCode * 397) ^ FixedLengthTraceFlag.GetHashCode();
                hashCode = (hashCode * 397) ^ ExtendedTextHeadersCount.GetHashCode();
                return hashCode;
            }
        }

        #endregion IEquatable<BinaryHeader> members
    }
}

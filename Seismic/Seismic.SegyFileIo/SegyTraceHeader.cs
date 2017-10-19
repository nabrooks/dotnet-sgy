using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Serialization;

namespace Seismic.SegyFileIo
{
    public class SegyTraceHeader : IEquatable<SegyTraceHeader>
    {
        private int _nsStart = 114;

        /// <summary>
        /// Default Empty Constructor.
        /// </summary>
        public SegyTraceHeader() { }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="inline">The inline value of this trace.</param>
        /// <param name="crossline">the crossline value of this trace.</param>
        /// <param name="x">The x coordinate of this trace. Truncate to 2 decimal places (ex. 1.01).</param>
        /// <param name="y">The x coordinate of this trace. Truncate to 2 decimal places (ex. 1.01).</param>
        /// <param name="cdp">The Cdp number of this trace</param>
        /// <param name="sampleCount">The number of samples intended to be written for this trace</param>
        /// <param name="sampleRateInMilliUnits">The sample rate of this trace in milli-units (milliseconds if time, millimeters if distance).</param>
        public SegyTraceHeader(int inline, int crossline, float x = 0, float y = 0, int cdp = 0, ushort sampleCount = 0, ushort sampleRateInMilliUnits = 0)
        {
            ScalarForCoordinates = -100;

            Local1 = inline;
            Local2 = crossline;
            Local3 = (int)x * 100;
            Local4 = (int)y * 100;

            XRecieverGroupCoordinate = (int)x * 100;
            YRecieverGroupCoordinate = (int)y * 100;
            XSourceCoordinate = (int)x * 100;
            YSourceCoordinate = (int)y * 100;

            SampleCount = sampleCount;
            SampleIntervalMs = sampleRateInMilliUnits;

            // elements 1 - 4
            TraceNumInLine = cdp;

            // elements 5-8
            TraceNumInFile = inline;

            // elements 21 - 24
            CdpNum = crossline;
        }

        protected SegyTraceHeader(SegyTraceHeader traceHdr)
        {
            TraceNumInLine = traceHdr.TraceNumInLine;
            TraceNumInFile = traceHdr.TraceNumInFile;
            ShotNumOrStackTraceNum = traceHdr.ShotNumOrStackTraceNum;
            TraceNumInShot = traceHdr.TraceNumInShot;
            EnergySourcePtNum = traceHdr.EnergySourcePtNum;
            CdpNum = traceHdr.CdpNum;
            TraceNumber = traceHdr.TraceNumber;
            TraceId = traceHdr.TraceId;
            NumVerticalStackedTraces = traceHdr.NumVerticalStackedTraces;
            CdpFold = traceHdr.CdpFold;
            DataUse = traceHdr.DataUse;
            SourceReceiverDistance = traceHdr.SourceReceiverDistance;
            RecieverGroupElevation = traceHdr.RecieverGroupElevation;
            SurfaceElevationAtSource = traceHdr.SurfaceElevationAtSource;
            SourceDepthBelowSurf = traceHdr.SourceDepthBelowSurf;
            DatumElevAtRecieverGroup = traceHdr.DatumElevAtRecieverGroup;
            DatumElevationAtSource = traceHdr.DatumElevationAtSource;
            WaterDepthAtSource = traceHdr.WaterDepthAtSource;
            WaterDepthAtRecieverGroup = traceHdr.WaterDepthAtRecieverGroup;
            ScalarForElevationAndDepth = traceHdr.ScalarForElevationAndDepth;
            ScalarForCoordinates = traceHdr.ScalarForCoordinates;
            XSourceCoordinate = traceHdr.XSourceCoordinate;
            YSourceCoordinate = traceHdr.YSourceCoordinate;
            XRecieverGroupCoordinate = traceHdr.XRecieverGroupCoordinate;
            YRecieverGroupCoordinate = traceHdr.YRecieverGroupCoordinate;
            CoordinateUnit = traceHdr.CoordinateUnit;
            WeatheringVelocity = traceHdr.WeatheringVelocity;
            SubweatheringVelocity = traceHdr.SubweatheringVelocity;
            UpholeTimeAtSource = traceHdr.UpholeTimeAtSource;
            UpholeTimeAtReceiverGroup = traceHdr.UpholeTimeAtReceiverGroup;
            SourceStaticCorrection = traceHdr.SourceStaticCorrection;
            ReceiverGroupStaticCorrection = traceHdr.ReceiverGroupStaticCorrection;
            TotalStaticApplied = traceHdr.TotalStaticApplied;
            HeaderTimeBreakLagMs = traceHdr.HeaderTimeBreakLagMs;
            TimeBreakShotLagMs = traceHdr.TimeBreakShotLagMs;
            ShotRecordingLag = traceHdr.ShotRecordingLag;
            MuteTimeStart = traceHdr.MuteTimeStart;
            MuteTimeEnd = traceHdr.MuteTimeEnd;
            SampleCount = traceHdr.SampleCount;
            SampleIntervalMs = traceHdr.SampleIntervalMs;
            GainType = traceHdr.GainType;
            GainConst = traceHdr.GainConst;
            EarlyGainDb = traceHdr.EarlyGainDb;
            Correlated = traceHdr.Correlated;
            SweepFrequencyStart = traceHdr.SweepFrequencyStart;
            SweepFrequencyEnd = traceHdr.SweepFrequencyEnd;
            SweepLengthMs = traceHdr.SweepLengthMs;
            SweepType = traceHdr.SweepType;
            SweepTaperTraceLengthStartMs = traceHdr.SweepTaperTraceLengthStartMs;
            SweepTaperTraceLengthEndMs = traceHdr.SweepTaperTraceLengthEndMs;
            TaperType = traceHdr.TaperType;
            AliasFilterFrequency = traceHdr.AliasFilterFrequency;
            AliasFilterSlope = traceHdr.AliasFilterSlope;
            NotchFilterFrequency = traceHdr.NotchFilterFrequency;
            NotchFilterSlope = traceHdr.NotchFilterSlope;
            LowCutFrequency = traceHdr.LowCutFrequency;
            HighCutFrequency = traceHdr.HighCutFrequency;
            LowCutSlope = traceHdr.LowCutSlope;
            HighCutSlope = traceHdr.HighCutSlope;
            Yr = traceHdr.Yr;
            Day = traceHdr.Day;
            Hour = traceHdr.Hour;
            Minute = traceHdr.Minute;
            Second = traceHdr.Second;
            TimeBasis = traceHdr.TimeBasis;
            TraceWeightFactor = traceHdr.TraceWeightFactor;
            GeophoneGroupNumOfRollSwitchPositionOne = traceHdr.GeophoneGroupNumOfRollSwitchPositionOne;
            GeophoneGroupNumOfFirstTraceOrigRecord = traceHdr.GeophoneGroupNumOfFirstTraceOrigRecord;
            GeophoneGroupNumOfLastTraceOrigRecord = traceHdr.GeophoneGroupNumOfLastTraceOrigRecord;
            GapSize = traceHdr.GapSize;
            TaperOverTravel = traceHdr.TaperOverTravel;
            Local1 = traceHdr.Local1;
            Local2 = traceHdr.Local2;
            Local3 = traceHdr.Local3;
            Local4 = traceHdr.Local4;
            Local5 = traceHdr.Local5;
            Local6 = traceHdr.Local6;
            NumTr = traceHdr.NumTr;
            Mark = traceHdr.Mark;
            ShortPad = traceHdr.ShortPad;
            Local7 = traceHdr.Local7;
            Local8 = traceHdr.Local8;
            Local9 = traceHdr.Local9;
            Local10 = traceHdr.Local10;
            Local11 = traceHdr.Local11;
            Local12 = traceHdr.Local12;
            Local13 = traceHdr.Local13;
        }

        #region Properties

        [DisplayName("Line Trace Number (1-4)")]
        public Int32 TraceNumInLine { get; set; }

        [DisplayName("File Trace Number (5-8)")]
        public Int32 TraceNumInFile { get; set; }

        [DisplayName("Shot or Stacktrace Number (9-12)")]
        public Int32 ShotNumOrStackTraceNum { get; set; }

        [DisplayName("Trace Number in Shot (13-16)")]
        public Int32 TraceNumInShot { get; set; }

        [DisplayName("Energy Source Point Number (17-20)")]
        public Int32 EnergySourcePtNum { get; set; }

        [DisplayName("Cdp Number (21-24)")]
        public Int32 CdpNum { get; set; }

        [DisplayName("Trace Number (25-28)")]
        public Int32 TraceNumber { get; set; }

        [DisplayName("Trace Id (29-30")]
        public Int16 TraceId { get; set; }

        [DisplayName("Vertical Stacked Trace Count (31-32)")]
        public Int16 NumVerticalStackedTraces { get; set; }

        [DisplayName("Cdp Fold (33-34")]
        public Int16 CdpFold { get; set; }

        [DisplayName("Data Use (35-36)")]
        public Int16 DataUse { get; set; }

        [DisplayName("Source-Reciever Distance (37-40)")]
        public Int32 SourceReceiverDistance { get; set; }

        [DisplayName("Reciever Group Elevation (41-44)")]
        public Int32 RecieverGroupElevation { get; set; }

        [DisplayName("Surface Elevation At Source (45-48)")]
        public Int32 SurfaceElevationAtSource { get; set; }

        [DisplayName("Source Depth Below Surface (49-52)")]
        public Int32 SourceDepthBelowSurf { get; set; }

        [DisplayName("Datum Elevation at Reciever Group (53-56)")]
        public Int32 DatumElevAtRecieverGroup { get; set; }

        [DisplayName("Datum Elevation at Source (57-60)")]
        public Int32 DatumElevationAtSource { get; set; }

        [DisplayName("Water Depth at Source (61-64)")]
        public Int32 WaterDepthAtSource { get; set; }

        [DisplayName("Water Depth at Reciever Group (65-68")]
        public Int32 WaterDepthAtRecieverGroup { get; set; }

        [DisplayName("Elevation or Depth Scalar (69-70)")]
        public Int16 ScalarForElevationAndDepth { get; set; }

        [DisplayName("Coordinate Scalar (71-72)")]
        public Int16 ScalarForCoordinates { get; set; }

        [DisplayName("X Source (73-76)")]
        public Int32 XSourceCoordinate { get; set; }

        [DisplayName("Y Source (77-80)")]
        public Int32 YSourceCoordinate { get; set; }

        [DisplayName("X Reciever Group (81-84)")]
        public Int32 XRecieverGroupCoordinate { get; set; }

        [DisplayName("Y Reciever Group (85-88)")]
        public Int32 YRecieverGroupCoordinate { get; set; }

        [DisplayName("Coordinate Unit (89-90)")]
        public Int16 CoordinateUnit { get; set; }

        [DisplayName("Weathering Velocity (91-92)")]
        public Int16 WeatheringVelocity { get; set; }

        [DisplayName("SubWeathering Velocity (93-94)")]
        public Int16 SubweatheringVelocity { get; set; }

        [DisplayName("Uphole Time at Source (95-95)")]
        public Int16 UpholeTimeAtSource { get; set; }

        [DisplayName("Uphole Time at Reciever (97-98)")]
        public Int16 UpholeTimeAtReceiverGroup { get; set; }

        [DisplayName("Source Static Correction (99-100)")]
        public Int16 SourceStaticCorrection { get; set; }

        [DisplayName("Reciever Static Correction (101-102)")]
        public Int16 ReceiverGroupStaticCorrection { get; set; }

        [DisplayName("Total Static Correction (103-104)")]
        public Int16 TotalStaticApplied { get; set; }

        [DisplayName("Header Time Break Lag(ms) (105-106)")]
        public Int16 HeaderTimeBreakLagMs { get; set; }

        [DisplayName("Time Break Shot Lag(ms) (107-108)")]
        public Int16 TimeBreakShotLagMs { get; set; }

        [DisplayName("Shot Record Time Lag(ms) (109-110)")]
        public Int16 ShotRecordingLag { get; set; }

        [DisplayName("Mute Time Start(ms) (111-112)")]
        public Int16 MuteTimeStart { get; set; }

        [DisplayName("Mute Time End(ms) (113-114)")]
        public Int16 MuteTimeEnd { get; set; }

        [DisplayName("Sample Count (115-)")]
        public UInt16 SampleCount { get; set; }

        [DisplayName("Sample Rate (117-)")]
        public UInt16 SampleIntervalMs { get; set; }

        [DisplayName("Gain Type (119-120)")]
        public Int16 GainType { get; set; }

        [DisplayName("Gain Const (121-122)")]
        public Int16 GainConst { get; set; }

        [DisplayName("Early Gain(db) (123-124)")]
        public Int16 EarlyGainDb { get; set; }

        [DisplayName("Correlated (125-126)")]
        public Int16 Correlated { get; set; }

        [DisplayName("Sweep Frequency Start (127-128)")]
        public Int16 SweepFrequencyStart { get; set; }

        [DisplayName("Sweep Frequency End (129-130)")]
        public Int16 SweepFrequencyEnd { get; set; }

        [DisplayName("Sweep Length(ms) (131-132)")]
        public Int16 SweepLengthMs { get; set; }

        [DisplayName("Sweep Type (133-134)")]
        public Int16 SweepType { get; set; }

        [DisplayName("Sweep Taper Trace Length Start(ms) (135-136)")]
        public Int16 SweepTaperTraceLengthStartMs { get; set; }

        [DisplayName("Sweep Taper Trace Length End(ms) (137-138)")]
        public Int16 SweepTaperTraceLengthEndMs { get; set; }

        [DisplayName("Taper Type (139-140)")]
        public Int16 TaperType { get; set; }

        [DisplayName("Alias Filter Frequency (141-142)")]
        public Int16 AliasFilterFrequency { get; set; }

        [DisplayName("Alias Filter Slope (143-144)")]
        public Int16 AliasFilterSlope { get; set; }

        [DisplayName("Notch Filter Frequency (145-146)")]
        public Int16 NotchFilterFrequency { get; set; }

        [DisplayName("Notch Filter Slope (147-148)")]
        public Int16 NotchFilterSlope { get; set; }

        [DisplayName("Low Cut Frequency (149-150)")]
        public Int16 LowCutFrequency { get; set; }

        [DisplayName("High Cut Frequency (151-152)")]
        public Int16 HighCutFrequency { get; set; }

        [DisplayName("Low Cut Slope (153-154)")]
        public Int16 LowCutSlope { get; set; }

        [DisplayName("High Cut Slope (155-156)")]
        public Int16 HighCutSlope { get; set; }

        [DisplayName("Year (157-158)")]
        public Int16 Yr { get; set; }

        [DisplayName("Day (159-160)")]
        public Int16 Day { get; set; }

        [DisplayName("Hour (161-162)")]
        public Int16 Hour { get; set; }

        [DisplayName("Minute (163-164)")]
        public Int16 Minute { get; set; }

        [DisplayName("Second (165-166)")]
        public Int16 Second { get; set; }

        [DisplayName("Time Basis (167-168)")]
        public Int16 TimeBasis { get; set; }

        [DisplayName("Trace Weight Factor (169-170)")]
        public Int16 TraceWeightFactor { get; set; }

        [DisplayName("Geophone Group Number of Roll Switch Position One (171-172)")]
        public Int16 GeophoneGroupNumOfRollSwitchPositionOne { get; set; }

        [DisplayName("Geophone Group Number of First Trace Original Rec (173-174)")]
        public Int16 GeophoneGroupNumOfFirstTraceOrigRecord { get; set; }

        [DisplayName("Geophone Group Number of Last Trace Original Rec (175-176)")]
        public Int16 GeophoneGroupNumOfLastTraceOrigRecord { get; set; }

        [DisplayName("Gap Size (177-178)")]
        public Int16 GapSize { get; set; }

        [DisplayName("Tape Over Travel (179-180)")]
        public Int16 TaperOverTravel { get; set; }

        [DisplayName("Local1 (179-180)")]
        public Int32 Local1 { get; set; }

        [DisplayName("Local2 (181-184)")]
        public Int32 Local2 { get; set; }

        [DisplayName("Local3 (185-188)")]
        public Int32 Local3 { get; set; }

        [DisplayName("Local4 (189-192)")]
        public Int32 Local4 { get; set; }

        [DisplayName("Local5 (193-196)")]
        public Int32 Local5 { get; set; }

        [DisplayName("Local6 (197-200)")]
        public Int32 Local6 { get; set; }

        [DisplayName("Trace Number (201-204)")]
        public Int32 NumTr { get; set; }

        [DisplayName("Mark (205-208)")]
        public Int16 Mark { get; set; }

        [DisplayName("Short Pad (209-212)")]
        public Int16 ShortPad { get; set; }

        [DisplayName("Local7 (213-216)")]
        public Int32 Local7 { get; set; }

        [DisplayName("Local8 (217-220)")]
        public Int32 Local8 { get; set; }

        [DisplayName("Local9 (221-224)")]
        public Int32 Local9 { get; set; }

        [DisplayName("Local10 (225-228)")]
        public Int32 Local10 { get; set; }

        [DisplayName("Local11 (229-232)")]
        public Int32 Local11 { get; set; }

        [DisplayName("Local12 (233-236)")]
        public Int32 Local12 { get; set; }

        [DisplayName("Local13 (237-240)")]
        public Int32 Local13 { get; set; }

        #endregion Properties

        public static SegyTraceHeader From(byte[] bytes, bool isLittleEndian = false)
        {
            CodeContract.Requires<NullReferenceException>(bytes != null, "byte array must not be null.");
            CodeContract.Requires<ArgumentException>(bytes.Length >= 240, "byte array length must be greater than or equal to 240 elements.");

            SegyTraceHeader hdr = new SegyTraceHeader();

            Func<byte[], int, short> bytesToInt16 = isLittleEndian
                ? (Func<byte[], int, short>)(BitConverter.ToInt16)
                : (Func<byte[], int, short>)(IbmConverter.ToInt16);

            Func<byte[], int, ushort> bytesToUInt16 = isLittleEndian
                ? (Func<byte[], int, ushort>)(BitConverter.ToUInt16)
                : (Func<byte[], int, ushort>)(IbmConverter.ToUInt16);

            Func<byte[], int, int> bytesToInt32 = isLittleEndian
                ? (Func<byte[], int, int>)(BitConverter.ToInt32)
                : (Func<byte[], int, int>)(IbmConverter.ToInt32);

            Func<byte[], int, uint> bytesToUInt32 = isLittleEndian
                ? (Func<byte[], int, uint>)(BitConverter.ToUInt32)
                : (Func<byte[], int, uint>)(IbmConverter.ToUInt32);

            //var lilendian_ns_16b_int_at_114 = BitConverter.ToInt16(bytes, 114);
            //var lilendian_ns_32b_int_at_112 = BitConverter.ToInt32(bytes, 112);
            //var bigendian_ns_16b_int_at_114 = IbmConverter.ToInt16(bytes, 114);
            //var bigendian_ns_32b_int_at_112 = IbmConverter.ToInt32(bytes, 112);

            //ushort ns;
            //short ns_16b_at_114 = 0;
            ////ushort ns_16b_at_114 = bytesToUInt16(bytes, 114);
            //uint ns_u32b_at_112 = bytesToUInt32(bytes, 112);
            //if(ns_u32b_at_112 > 1000000)
            //if (ns_16b_at_114 >= 0)
            //{
            //    ns = (ushort)ns_16b_at_114;
            //}
            //else
            //{
            //    ns = Convert.ToUInt16(ns_u32b_at_112);
            //    hdr._nsStart = 112;
            //}

            //ns_16b_at_114 = bytesToInt16(bytes, 114);

            //ns = (ushort)bytesToInt16(bytes, 114); ;

            hdr.TraceNumInLine = bytesToInt32(bytes, 0);
            hdr.TraceNumInFile = bytesToInt32(bytes, 4);
            hdr.ShotNumOrStackTraceNum = bytesToInt32(bytes, 8);
            hdr.TraceNumInShot = bytesToInt32(bytes, 12);
            hdr.EnergySourcePtNum = bytesToInt32(bytes, 16);
            hdr.CdpNum = bytesToInt32(bytes, 20);
            hdr.TraceNumber = bytesToInt32(bytes, 24);
            hdr.TraceId = bytesToInt16(bytes, 28);
            hdr.NumVerticalStackedTraces = bytesToInt16(bytes, 30);
            hdr.CdpFold = bytesToInt16(bytes, 32);
            hdr.DataUse = bytesToInt16(bytes, 34);
            hdr.SourceReceiverDistance = bytesToInt32(bytes, 36);
            hdr.RecieverGroupElevation = bytesToInt32(bytes, 40);
            hdr.SurfaceElevationAtSource = bytesToInt32(bytes, 44);
            hdr.SourceDepthBelowSurf = bytesToInt32(bytes, 48);
            hdr.DatumElevAtRecieverGroup = bytesToInt32(bytes, 52);
            hdr.DatumElevationAtSource = bytesToInt32(bytes, 56);
            hdr.WaterDepthAtSource = bytesToInt32(bytes, 60);
            hdr.WaterDepthAtRecieverGroup = bytesToInt32(bytes, 64);
            hdr.ScalarForElevationAndDepth = bytesToInt16(bytes, 68);
            hdr.ScalarForCoordinates = bytesToInt16(bytes, 70);
            hdr.XSourceCoordinate = bytesToInt32(bytes, 72);
            hdr.YSourceCoordinate = bytesToInt32(bytes, 76);
            hdr.XRecieverGroupCoordinate = bytesToInt32(bytes, 80);
            hdr.YRecieverGroupCoordinate = bytesToInt32(bytes, 84);
            hdr.CoordinateUnit = bytesToInt16(bytes, 88);
            hdr.WeatheringVelocity = bytesToInt16(bytes, 90);
            hdr.SubweatheringVelocity = bytesToInt16(bytes, 92);
            hdr.UpholeTimeAtSource = bytesToInt16(bytes, 94);
            hdr.UpholeTimeAtReceiverGroup = bytesToInt16(bytes, 96);
            hdr.SourceStaticCorrection = bytesToInt16(bytes, 98);
            hdr.ReceiverGroupStaticCorrection = bytesToInt16(bytes, 100);
            hdr.TotalStaticApplied = bytesToInt16(bytes, 102);
            hdr.HeaderTimeBreakLagMs = bytesToInt16(bytes, 104);
            hdr.TimeBreakShotLagMs = bytesToInt16(bytes, 106);
            hdr.ShotRecordingLag = bytesToInt16(bytes, 108);
            hdr.MuteTimeStart = bytesToInt16(bytes, 110);
            hdr.MuteTimeEnd = bytesToInt16(bytes, 112);
            hdr.SampleCount = (ushort)bytesToInt16(bytes, 114);
            //hdr.SampleCount = ns;
            hdr.SampleIntervalMs = (ushort)bytesToInt16(bytes, 116);
            hdr.GainType = bytesToInt16(bytes, 118);
            hdr.GainConst = bytesToInt16(bytes, 120);
            hdr.EarlyGainDb = bytesToInt16(bytes, 122);
            hdr.Correlated = bytesToInt16(bytes, 124);
            hdr.SweepFrequencyStart = bytesToInt16(bytes, 126);
            hdr.SweepFrequencyEnd = bytesToInt16(bytes, 128);
            hdr.SweepLengthMs = bytesToInt16(bytes, 130);
            hdr.SweepType = bytesToInt16(bytes, 132);
            hdr.SweepTaperTraceLengthStartMs = bytesToInt16(bytes, 134);
            hdr.SweepTaperTraceLengthEndMs = bytesToInt16(bytes, 136);
            hdr.TaperType = bytesToInt16(bytes, 138);
            hdr.AliasFilterFrequency = bytesToInt16(bytes, 140);
            hdr.AliasFilterSlope = bytesToInt16(bytes, 142);
            hdr.NotchFilterFrequency = bytesToInt16(bytes, 144);
            hdr.NotchFilterSlope = bytesToInt16(bytes, 146);
            hdr.LowCutFrequency = bytesToInt16(bytes, 148);
            hdr.HighCutFrequency = bytesToInt16(bytes, 150);
            hdr.LowCutSlope = bytesToInt16(bytes, 152);
            hdr.HighCutSlope = bytesToInt16(bytes, 154);
            hdr.Yr = bytesToInt16(bytes, 156);
            hdr.Day = bytesToInt16(bytes, 158);
            hdr.Hour = bytesToInt16(bytes, 160);
            hdr.Minute = bytesToInt16(bytes, 162);
            hdr.Second = bytesToInt16(bytes, 164);
            hdr.TimeBasis = bytesToInt16(bytes, 166);
            hdr.TraceWeightFactor = bytesToInt16(bytes, 168);
            hdr.GeophoneGroupNumOfRollSwitchPositionOne = bytesToInt16(bytes, 170);
            hdr.GeophoneGroupNumOfFirstTraceOrigRecord = bytesToInt16(bytes, 172);
            hdr.GeophoneGroupNumOfLastTraceOrigRecord = bytesToInt16(bytes, 174);
            hdr.GapSize = bytesToInt16(bytes, 176);
            hdr.TaperOverTravel = bytesToInt16(bytes, 178);
            hdr.Local1 = bytesToInt32(bytes, 180);
            hdr.Local2 = bytesToInt32(bytes, 184);
            hdr.Local3 = bytesToInt32(bytes, 188);
            hdr.Local4 = bytesToInt32(bytes, 192);
            hdr.Local5 = bytesToInt32(bytes, 196);
            hdr.Local6 = bytesToInt32(bytes, 200);
            hdr.NumTr = bytesToInt32(bytes, 204);
            hdr.Mark = bytesToInt16(bytes, 208);
            hdr.ShortPad = bytesToInt16(bytes, 210);
            hdr.Local7 = bytesToInt32(bytes, 212);
            hdr.Local8 = bytesToInt32(bytes, 216);
            hdr.Local9 = bytesToInt32(bytes, 220);
            hdr.Local10 = bytesToInt32(bytes, 224);
            hdr.Local11 = bytesToInt32(bytes, 228);
            hdr.Local12 = bytesToInt32(bytes, 232);
            hdr.Local13 = bytesToInt32(bytes, 236);
            return hdr;
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[240];

            // TODO : performance optimize this by removing all "Concat" statements.
            Insert(bytes, IbmConverter.GetBytes(TraceNumInLine), 0);
            Insert(bytes, IbmConverter.GetBytes(TraceNumInFile), 4);
            Insert(bytes, IbmConverter.GetBytes(ShotNumOrStackTraceNum), 8);
            Insert(bytes, IbmConverter.GetBytes(TraceNumInShot), 12);
            Insert(bytes, IbmConverter.GetBytes(EnergySourcePtNum), 16);
            Insert(bytes, IbmConverter.GetBytes(CdpNum), 20);
            Insert(bytes, IbmConverter.GetBytes(TraceNumber), 24);
            Insert(bytes, IbmConverter.GetBytes((Int16)TraceId), 28);
            Insert(bytes, IbmConverter.GetBytes((Int16)NumVerticalStackedTraces), 30);
            Insert(bytes, IbmConverter.GetBytes((Int16)CdpFold), 32);
            Insert(bytes, IbmConverter.GetBytes((Int16)DataUse), 34);
            Insert(bytes, IbmConverter.GetBytes(SourceReceiverDistance), 36);
            Insert(bytes, IbmConverter.GetBytes(RecieverGroupElevation), 40);
            Insert(bytes, IbmConverter.GetBytes(SurfaceElevationAtSource), 44);
            Insert(bytes, IbmConverter.GetBytes(SourceDepthBelowSurf), 48);
            Insert(bytes, IbmConverter.GetBytes(DatumElevAtRecieverGroup), 52);
            Insert(bytes, IbmConverter.GetBytes(DatumElevationAtSource), 56);
            Insert(bytes, IbmConverter.GetBytes(WaterDepthAtSource), 60);
            Insert(bytes, IbmConverter.GetBytes(WaterDepthAtRecieverGroup), 64);
            Insert(bytes, IbmConverter.GetBytes((Int16)ScalarForElevationAndDepth), 68);
            Insert(bytes, IbmConverter.GetBytes((Int16)ScalarForCoordinates), 70);
            Insert(bytes, IbmConverter.GetBytes(XSourceCoordinate), 72);
            Insert(bytes, IbmConverter.GetBytes(YSourceCoordinate), 76);
            Insert(bytes, IbmConverter.GetBytes(XRecieverGroupCoordinate), 80);
            Insert(bytes, IbmConverter.GetBytes(YRecieverGroupCoordinate), 84);
            Insert(bytes, IbmConverter.GetBytes((Int16)CoordinateUnit), 88);
            Insert(bytes, IbmConverter.GetBytes((Int16)WeatheringVelocity), 90);
            Insert(bytes, IbmConverter.GetBytes((Int16)SubweatheringVelocity), 92);
            Insert(bytes, IbmConverter.GetBytes((Int16)UpholeTimeAtSource), 94);
            Insert(bytes, IbmConverter.GetBytes((Int16)UpholeTimeAtReceiverGroup), 96);
            Insert(bytes, IbmConverter.GetBytes((Int16)SourceStaticCorrection), 98);
            Insert(bytes, IbmConverter.GetBytes((Int16)ReceiverGroupStaticCorrection), 100);
            Insert(bytes, IbmConverter.GetBytes((Int16)TotalStaticApplied), 102);
            Insert(bytes, IbmConverter.GetBytes((Int16)HeaderTimeBreakLagMs), 104);
            Insert(bytes, IbmConverter.GetBytes((Int16)TimeBreakShotLagMs), 106);
            Insert(bytes, IbmConverter.GetBytes((Int16)ShotRecordingLag), 108);
            Insert(bytes, IbmConverter.GetBytes((Int16)MuteTimeStart), 110);
            Insert(bytes, IbmConverter.GetBytes((Int16)MuteTimeEnd), 112);
            Insert(bytes, IbmConverter.GetBytes((UInt16)SampleCount), _nsStart);
            Insert(bytes, IbmConverter.GetBytes((Int16)SampleIntervalMs), 116);
            Insert(bytes, IbmConverter.GetBytes((Int16)GainType), 118);
            Insert(bytes, IbmConverter.GetBytes((Int16)GainConst), 120);
            Insert(bytes, IbmConverter.GetBytes((Int16)EarlyGainDb), 122);
            Insert(bytes, IbmConverter.GetBytes((Int16)Correlated), 124);
            Insert(bytes, IbmConverter.GetBytes((Int16)SweepFrequencyStart), 126);
            Insert(bytes, IbmConverter.GetBytes((Int16)SweepFrequencyEnd), 128);
            Insert(bytes, IbmConverter.GetBytes((Int16)SweepLengthMs), 130);
            Insert(bytes, IbmConverter.GetBytes((Int16)SweepType), 132);
            Insert(bytes, IbmConverter.GetBytes((Int16)SweepTaperTraceLengthStartMs), 134);
            Insert(bytes, IbmConverter.GetBytes((Int16)SweepTaperTraceLengthEndMs), 136);
            Insert(bytes, IbmConverter.GetBytes((Int16)TaperType), 138);
            Insert(bytes, IbmConverter.GetBytes((Int16)AliasFilterFrequency), 140);
            Insert(bytes, IbmConverter.GetBytes((Int16)AliasFilterSlope), 142);
            Insert(bytes, IbmConverter.GetBytes((Int16)NotchFilterFrequency), 144);
            Insert(bytes, IbmConverter.GetBytes((Int16)NotchFilterSlope), 146);
            Insert(bytes, IbmConverter.GetBytes((Int16)LowCutFrequency), 148);
            Insert(bytes, IbmConverter.GetBytes((Int16)HighCutFrequency), 150);
            Insert(bytes, IbmConverter.GetBytes((Int16)LowCutSlope), 152);
            Insert(bytes, IbmConverter.GetBytes((Int16)HighCutSlope), 154);
            Insert(bytes, IbmConverter.GetBytes((Int16)Yr), 156);
            Insert(bytes, IbmConverter.GetBytes((Int16)Day), 158);
            Insert(bytes, IbmConverter.GetBytes((Int16)Hour), 160);
            Insert(bytes, IbmConverter.GetBytes((Int16)Minute), 162);
            Insert(bytes, IbmConverter.GetBytes((Int16)Second), 164);
            Insert(bytes, IbmConverter.GetBytes((Int16)TimeBasis), 166);
            Insert(bytes, IbmConverter.GetBytes((Int16)TraceWeightFactor), 168);
            Insert(bytes, IbmConverter.GetBytes((Int16)GeophoneGroupNumOfRollSwitchPositionOne), 170);
            Insert(bytes, IbmConverter.GetBytes((Int16)GeophoneGroupNumOfFirstTraceOrigRecord), 172);
            Insert(bytes, IbmConverter.GetBytes((Int16)GeophoneGroupNumOfLastTraceOrigRecord), 174);
            Insert(bytes, IbmConverter.GetBytes((Int16)GapSize), 176);
            Insert(bytes, IbmConverter.GetBytes((Int16)TaperOverTravel), 178);
            Insert(bytes, IbmConverter.GetBytes(Local1), 180);
            Insert(bytes, IbmConverter.GetBytes(Local2), 184);
            Insert(bytes, IbmConverter.GetBytes(Local3), 188);
            Insert(bytes, IbmConverter.GetBytes(Local4), 192);
            Insert(bytes, IbmConverter.GetBytes(Local5), 196);
            Insert(bytes, IbmConverter.GetBytes(Local6), 200);
            Insert(bytes, IbmConverter.GetBytes(NumTr), 204);
            Insert(bytes, IbmConverter.GetBytes((Int16)Mark), 208);
            Insert(bytes, IbmConverter.GetBytes((Int16)ShortPad), 210);
            Insert(bytes, IbmConverter.GetBytes(Local7), 212);
            Insert(bytes, IbmConverter.GetBytes(Local8), 216);
            Insert(bytes, IbmConverter.GetBytes(Local9), 220);
            Insert(bytes, IbmConverter.GetBytes(Local10), 224);
            Insert(bytes, IbmConverter.GetBytes(Local11), 228);
            Insert(bytes, IbmConverter.GetBytes(Local12), 232);
            Insert(bytes, IbmConverter.GetBytes(Local13), 236);
            return bytes;
        }

        public bool Equals(SegyTraceHeader other)
        {
            if (other == null) return false;
            return TraceNumInLine == other.TraceNumInLine &&
                   TraceNumInFile == other.TraceNumInFile &&
                   ShotNumOrStackTraceNum == other.ShotNumOrStackTraceNum &&
                   TraceNumInShot == other.TraceNumInShot &&
                   EnergySourcePtNum == other.EnergySourcePtNum &&
                   CdpNum == other.CdpNum &&
                   TraceNumber == other.TraceNumber &&
                   TraceId == other.TraceId &&
                   NumVerticalStackedTraces == other.NumVerticalStackedTraces &&
                   CdpFold == other.CdpFold &&
                   DataUse == other.DataUse &&
                   SourceReceiverDistance == other.SourceReceiverDistance &&
                   RecieverGroupElevation == other.RecieverGroupElevation &&
                   SurfaceElevationAtSource == other.SurfaceElevationAtSource &&
                   SourceDepthBelowSurf == other.SourceDepthBelowSurf &&
                   DatumElevAtRecieverGroup == other.DatumElevAtRecieverGroup &&
                   DatumElevationAtSource == other.DatumElevationAtSource &&
                   WaterDepthAtSource == other.WaterDepthAtSource &&
                   WaterDepthAtRecieverGroup == other.WaterDepthAtRecieverGroup &&
                   ScalarForElevationAndDepth == other.ScalarForElevationAndDepth &&
                   ScalarForCoordinates == other.ScalarForCoordinates &&
                   XSourceCoordinate == other.XSourceCoordinate &&
                   YSourceCoordinate == other.YSourceCoordinate &&
                   XRecieverGroupCoordinate == other.XRecieverGroupCoordinate &&
                   YRecieverGroupCoordinate == other.YRecieverGroupCoordinate &&
                   CoordinateUnit == other.CoordinateUnit &&
                   WeatheringVelocity == other.WeatheringVelocity &&
                   SubweatheringVelocity == other.SubweatheringVelocity &&
                   UpholeTimeAtSource == other.UpholeTimeAtSource &&
                   UpholeTimeAtReceiverGroup == other.UpholeTimeAtReceiverGroup &&
                   SourceStaticCorrection == other.SourceStaticCorrection &&
                   ReceiverGroupStaticCorrection == other.ReceiverGroupStaticCorrection &&
                   TotalStaticApplied == other.TotalStaticApplied &&
                   HeaderTimeBreakLagMs == other.HeaderTimeBreakLagMs &&
                   TimeBreakShotLagMs == other.TimeBreakShotLagMs &&
                   ShotRecordingLag == other.ShotRecordingLag &&
                   MuteTimeStart == other.MuteTimeStart &&
                   MuteTimeEnd == other.MuteTimeEnd &&
                   SampleCount == other.SampleCount &&
                   SampleIntervalMs == other.SampleIntervalMs &&
                   GainType == other.GainType &&
                   GainConst == other.GainConst &&
                   EarlyGainDb == other.EarlyGainDb &&
                   Correlated == other.Correlated &&
                   SweepFrequencyStart == other.SweepFrequencyStart &&
                   SweepFrequencyEnd == other.SweepFrequencyEnd &&
                   SweepLengthMs == other.SweepLengthMs &&
                   SweepType == other.SweepType &&
                   SweepTaperTraceLengthStartMs == other.SweepTaperTraceLengthStartMs &&
                   SweepTaperTraceLengthEndMs == other.SweepTaperTraceLengthEndMs &&
                   TaperType == other.TaperType &&
                   AliasFilterFrequency == other.AliasFilterFrequency &&
                   AliasFilterSlope == other.AliasFilterSlope &&
                   NotchFilterFrequency == other.NotchFilterFrequency &&
                   NotchFilterSlope == other.NotchFilterSlope &&
                   LowCutFrequency == other.LowCutFrequency &&
                   HighCutFrequency == other.HighCutFrequency &&
                   LowCutSlope == other.LowCutSlope &&
                   HighCutSlope == other.HighCutSlope &&
                   Yr == other.Yr &&
                   Day == other.Day &&
                   Hour == other.Hour &&
                   Minute == other.Minute &&
                   Second == other.Second &&
                   TimeBasis == other.TimeBasis &&
                   TraceWeightFactor == other.TraceWeightFactor &&
                   GeophoneGroupNumOfRollSwitchPositionOne == other.GeophoneGroupNumOfRollSwitchPositionOne &&
                   GeophoneGroupNumOfFirstTraceOrigRecord == other.GeophoneGroupNumOfFirstTraceOrigRecord &&
                   GeophoneGroupNumOfLastTraceOrigRecord == other.GeophoneGroupNumOfLastTraceOrigRecord &&
                   GapSize == other.GapSize &&
                   TaperOverTravel == other.TaperOverTravel &&
                   Local1 == other.Local1 &&
                   Local2 == other.Local2 &&
                   Local3 == other.Local3 &&
                   Local4 == other.Local4 &&
                   Local5 == other.Local5 &&
                   Local6 == other.Local6 &&
                   NumTr == other.NumTr &&
                   Mark == other.Mark &&
                   ShortPad == other.ShortPad &&
                   Local7 == other.Local7 &&
                   Local8 == other.Local8 &&
                   Local9 == other.Local9 &&
                   Local10 == other.Local10 &&
                   Local11 == other.Local11 &&
                   Local12 == other.Local12 &&
                   Local13 == other.Local13;

        }

        private static void Insert(byte[] into, byte[] from, int startingIndex)
        {
            for (int i = 0; i < from.Length; i++)
                into[startingIndex + i] = from[i];
        }

        #region IDeepCloneable<SegyTraceHeader> Members

        public SegyTraceHeader DeepClone() => new SegyTraceHeader(this);

        #endregion
    }
}

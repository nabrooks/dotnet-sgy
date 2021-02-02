using System;
using Utility.Io.Serialization;

namespace Hess.Seismic.SegyFileIo
{
    /// <summary>
    /// Represents the header to a seismic trace
    /// </summary>
    public struct TraceHeader
    {
        public static TraceHeader From(byte[] bytes, EndianBitConverter bitConverter)
        {
            TraceHeader hdr = new TraceHeader();

            Func<byte[], int, short> bytesToInt16 = bitConverter.ToInt16;
            Func<byte[], int, int> bytesToInt32 = bitConverter.ToInt32;

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
            hdr.XCdp = bytesToInt32(bytes, 180);
            hdr.YCdp = bytesToInt32(bytes, 184);
            hdr.Inline = bytesToInt32(bytes, 188);
            hdr.Crossline = bytesToInt32(bytes, 192);
            hdr.ShotPoint = bytesToInt32(bytes, 196);
            hdr.ShotPointScalar = bytesToInt16(bytes, 200);
            hdr.TraceValueMeasurementUnit = bytesToInt16(bytes, 202);
            hdr.TransductionConstantMantissa = bytesToInt32(bytes, 204);
            hdr.TransductionConstantPower = bytesToInt16(bytes, 208);
            hdr.TransductionUnit = bytesToInt16(bytes, 210);
            hdr.TraceIdentifier = bytesToInt16(bytes, 212);
            hdr.ScalarTraceHeader = bytesToInt16(bytes, 214);
            hdr.SourceType = bytesToInt16(bytes, 216);
            hdr.SourceEnergyDirectionMantissa = bytesToInt32(bytes, 218);
            hdr.SourceEnergyDirectionExponent = bytesToInt16(bytes, 222);
            hdr.SourceMeasurementMantissa = bytesToInt32(bytes, 224);
            hdr.SourceMeasurementExponent = bytesToInt16(bytes, 228);
            hdr.SourceMeasurementUnit = bytesToInt16(bytes, 230);
            hdr.UnassignedInt1 = bytesToInt32(bytes, 232);
            hdr.UnassignedInt2 = bytesToInt32(bytes, 236);
            return hdr;
        }

        #region Properties

        public Int32 TraceNumInLine;                                // "Line Trace Number (1-4)"
        public Int32 TraceNumInFile;                                // "File Trace Number (5-8)"
        public Int32 ShotNumOrStackTraceNum;                        // "Shot or Stacktrace Number (9-12)"
        public Int32 TraceNumInShot;                                // "Trace Number in Shot (13-16)"
        public Int32 EnergySourcePtNum;                             // "Energy Source Point Number (17-20)"
        public Int32 CdpNum;                                        // "Cdp Number (21-24)"
        public Int32 TraceNumber;                                   // "Trace Number (25-28)"
        public Int16 TraceId;                                       // "Trace Id (29-30)"
        public Int16 NumVerticalStackedTraces;                      // "Vertical Stacked Trace Count (31-32)"
        public Int16 CdpFold;                                       // "Cdp Fold (33-34)"
        public Int16 DataUse;                                       // "Data Use (35-36)"
        public Int32 SourceReceiverDistance;                        // "Source-Reciever Distance (37-40)"
        public Int32 RecieverGroupElevation;                        // "Reciever Group Elevation (41-44)"
        public Int32 SurfaceElevationAtSource;                      // "Surface Elevation At Source (45-48)"
        public Int32 SourceDepthBelowSurf;                          // "Source Depth Below Surface (49-52)"
        public Int32 DatumElevAtRecieverGroup;                      // "Datum Elevation at Reciever Group (53-56)"
        public Int32 DatumElevationAtSource;                        // "Datum Elevation at Source (57-60)"
        public Int32 WaterDepthAtSource;                            // "Water Depth at Source (61-64)"
        public Int32 WaterDepthAtRecieverGroup;                     // "Water Depth at Reciever Group (65-68"
        public Int16 ScalarForElevationAndDepth;                    // "Elevation or Depth Scalar (69-70)"
        public Int16 ScalarForCoordinates;                          // "Coordinate Scalar (71-72)"
        public Int32 XSourceCoordinate;                             // "X Source (73-76)"
        public Int32 YSourceCoordinate;                             // "Y Source (77-80)"
        public Int32 XRecieverGroupCoordinate;                      // "X Reciever Group (81-84)"
        public Int32 YRecieverGroupCoordinate;                      // "Y Reciever Group (85-88)"
        public Int16 CoordinateUnit;                                // "Coordinate Unit (89-90)"
        public Int16 WeatheringVelocity;                            // "Weathering Velocity (91-92)"
        public Int16 SubweatheringVelocity;                         // "SubWeathering Velocity (93-94)"
        public Int16 UpholeTimeAtSource;                            // "Uphole Time at Source (95-95)"
        public Int16 UpholeTimeAtReceiverGroup;                     // "Uphole Time at Reciever (97-98)"
        public Int16 SourceStaticCorrection;                        // "Source Static Correction (99-100)"
        public Int16 ReceiverGroupStaticCorrection;                 // "Reciever Static Correction (101-102)"
        public Int16 TotalStaticApplied;                            // "Total Static Correction (103-104)"
        public Int16 HeaderTimeBreakLagMs;                          // "Header Time Break Lag(ms) (105-106)"
        public Int16 TimeBreakShotLagMs;                            // "Time Break Shot Lag(ms) (107-108)"
        public Int16 ShotRecordingLag;                              // "Shot Record Time Lag(ms) (109-110)"
        public Int16 MuteTimeStart;                                 // "Mute Time Start(ms) (111-112)"
        public Int16 MuteTimeEnd;                                   // "Mute Time End(ms) (113-114)"
        public UInt16 SampleCount;                                  // "Sample Count (115-)"
        public UInt16 SampleIntervalMs;                             // "Sample Rate (117-)"
        public Int16 GainType;                                      // "Gain Type (119-120)"
        public Int16 GainConst;                                     // "Gain Const (121-122)"
        public Int16 EarlyGainDb;                                   // "Early Gain(db) (123-124)"
        public Int16 Correlated;                                    // "Correlated (125-126)"
        public Int16 SweepFrequencyStart;                           // "Sweep Frequency Start (127-128)"
        public Int16 SweepFrequencyEnd;                             // "Sweep Frequency End (129-130)"
        public Int16 SweepLengthMs;                                 // "Sweep Length(ms) (131-132)"
        public Int16 SweepType;                                     // "Sweep Type (133-134)"
        public Int16 SweepTaperTraceLengthStartMs;                  // "Sweep Taper Trace Length Start(ms) (135-136)"
        public Int16 SweepTaperTraceLengthEndMs;                    // "Sweep Taper Trace Length End(ms) (137-138)"
        public Int16 TaperType;                                     // "Taper Type (139-140)"
        public Int16 AliasFilterFrequency;                          // "Alias Filter Frequency (141-142)"
        public Int16 AliasFilterSlope;                              // "Alias Filter Slope (143-144)"
        public Int16 NotchFilterFrequency;                          // "Notch Filter Frequency (145-146)"
        public Int16 NotchFilterSlope;                              // "Notch Filter Slope (147-148)"
        public Int16 LowCutFrequency;                               // "Low Cut Frequency (149-150)"
        public Int16 HighCutFrequency;                              // "High Cut Frequency (151-152)"
        public Int16 LowCutSlope;                                   // "Low Cut Slope (153-154)"
        public Int16 HighCutSlope;                                  // "High Cut Slope (155-156)"
        public Int16 Yr;                                            // "Year (157-158)"
        public Int16 Day;                                           // "Day (159-160)"
        public Int16 Hour;                                          // "Hour (161-162)"
        public Int16 Minute;                                        // "Minute (163-164)"
        public Int16 Second;                                        // "Second (165-166)"
        public Int16 TimeBasis;                                     // "Time Basis (167-168)"
        public Int16 TraceWeightFactor;                             // "Trace Weight Factor (169-170)"
        public Int16 GeophoneGroupNumOfRollSwitchPositionOne;       // "Geophone Group Number of Roll Switch Position One (171-172)"
        public Int16 GeophoneGroupNumOfFirstTraceOrigRecord;        // "Geophone Group Number of First Trace Original Rec (173-174)"
        public Int16 GeophoneGroupNumOfLastTraceOrigRecord;         // "Geophone Group Number of Last Trace Original Rec (175-176)"
        public Int16 GapSize;                                       // "Gap Size (177-178)"
        public Int16 TaperOverTravel;                               // "Tape Over Travel (179-180)"
        public Int32 XCdp;                                          // (181-184)
        public Int32 YCdp;                                          // (185-188)
        public Int32 Inline;                                        // (189-192)
        public Int32 Crossline;                                     // (193-196)
        public Int32 ShotPoint;                                     // (197-200)
        public Int16 ShotPointScalar;                               // (201-202)
        public Int16 TraceValueMeasurementUnit;                     // (203-204)
        public Int32 TransductionConstantMantissa;                  // (205-208)
        public Int16 TransductionConstantPower;                     // (209-210)
        public Int16 TransductionUnit;                              // (211-212)
        public Int16 TraceIdentifier;                               // (213-214)
        public Int16 ScalarTraceHeader;                             // (215-216)
        public Int16 SourceType;                                    // (217-218)
        public Int32 SourceEnergyDirectionMantissa;                 // (219-222)
        public Int16 SourceEnergyDirectionExponent;                 // (223-224)
        public Int32 SourceMeasurementMantissa;                     // (225-228)
        public Int16 SourceMeasurementExponent;                     // (229-230)
        public Int16 SourceMeasurementUnit;                         // (231-232)
        public Int32 UnassignedInt1;                                // (233-236)
        public Int32 UnassignedInt2;                                // (237-240)

        #endregion Properties

        /// <summary>
        /// Gets the trace header property value based on the zero based index of 
        /// that property (petrel usually uses one based indexing for trace headers).
        /// </summary>
        /// <param name="index">The zero based index of the property intended to get</param>
        /// <returns>An integer</returns>
        public Int32 this[int index]
        {
            get
            {
                if (index == 0) return TraceNumInLine;
                else if (index == 4) return TraceNumInFile;
                else if (index == 8) return ShotNumOrStackTraceNum;
                else if (index == 12) return TraceNumInShot;
                else if (index == 16) return EnergySourcePtNum;
                else if (index == 20) return CdpNum;
                else if (index == 24) return TraceNumber;
                else if (index == 28) return TraceId;
                else if (index == 30) return NumVerticalStackedTraces;
                else if (index == 32) return CdpFold;
                else if (index == 34) return DataUse;                                        // "Data Use (35-36)"
                else if (index == 36) return SourceReceiverDistance;                         // "Source-Reciever Distance (37-40)"
                else if (index == 40) return RecieverGroupElevation;                         // "Reciever Group Elevation (41-44)"
                else if (index == 44) return SurfaceElevationAtSource;                       // "Surface Elevation At Source (45-48)"
                else if (index == 48) return SourceDepthBelowSurf;                           // "Source Depth Below Surface (49-52)"
                else if (index == 52) return DatumElevAtRecieverGroup;                       // "Datum Elevation at Reciever Group (53-56)"
                else if (index == 56) return DatumElevationAtSource;                         // "Datum Elevation at Source (57-60)"
                else if (index == 60) return WaterDepthAtSource;                             // "Water Depth at Source (61-64)"
                else if (index == 64) return WaterDepthAtRecieverGroup;                      // "Water Depth at Reciever Group (65-68"
                else if (index == 68) return ScalarForElevationAndDepth;                     // "Elevation or Depth Scalar (69-70)"
                else if (index == 70) return ScalarForCoordinates;                           // "Coordinate Scalar (71-72)"
                else if (index == 72) return XSourceCoordinate;                              // "X Source (73-76)"
                else if (index == 76) return YSourceCoordinate;                              // "Y Source (77-80)"
                else if (index == 80) return XRecieverGroupCoordinate;                       // "X Reciever Group (81-84)"
                else if (index == 84) return YRecieverGroupCoordinate;                       // "Y Reciever Group (85-88)"
                else if (index == 88) return CoordinateUnit;                                 // "Coordinate Unit (89-90)"
                else if (index == 90) return WeatheringVelocity;                             // "Weathering Velocity (91-92)"
                else if (index == 92) return SubweatheringVelocity;                          // "SubWeathering Velocity (93-94)"
                else if (index == 94) return UpholeTimeAtSource;                             // "Uphole Time at Source (95-95)"
                else if (index == 96) return UpholeTimeAtReceiverGroup;                      // "Uphole Time at Reciever (97-98)"
                else if (index == 98) return SourceStaticCorrection;                         // "Source Static Correction (99-100)"
                else if (index == 100) return ReceiverGroupStaticCorrection;                 // "Reciever Static Correction (101-102)"
                else if (index == 102) return TotalStaticApplied;                            // "Total Static Correction (103-104)"
                else if (index == 104) return HeaderTimeBreakLagMs;                          // "Header Time Break Lag(ms) (105-106)"
                else if (index == 106) return TimeBreakShotLagMs;                            // "Time Break Shot Lag(ms) (107-108)"
                else if (index == 108) return ShotRecordingLag;                              // "Shot Record Time Lag(ms) (109-110)"
                else if (index == 110) return MuteTimeStart;                                 // "Mute Time Start(ms) (111-112)"
                else if (index == 112) return MuteTimeEnd;                                   // "Mute Time End(ms) (113-114)"
                else if (index == 114) return SampleCount;                                   // "Sample Count (115-)"
                else if (index == 116) return SampleIntervalMs;                              // "Sample Rate (117-)"
                else if (index == 118) return GainType;                                      // "Gain Type (119-120)"
                else if (index == 120) return GainConst;                                     // "Gain Const (121-122)"
                else if (index == 122) return EarlyGainDb;                                   // "Early Gain(db) (123-124)"
                else if (index == 124) return Correlated;                                    // "Correlated (125-126)"
                else if (index == 126) return SweepFrequencyStart;                           // "Sweep Frequency Start (127-128)"
                else if (index == 128) return SweepFrequencyEnd;                             // "Sweep Frequency End (129-130)"
                else if (index == 130) return SweepLengthMs;                                 // "Sweep Length(ms) (131-132)"
                else if (index == 132) return SweepType;                                     // "Sweep Type (133-134)"
                else if (index == 134) return SweepTaperTraceLengthStartMs;                  // "Sweep Taper Trace Length Start(ms) (135-136)"
                else if (index == 136) return SweepTaperTraceLengthEndMs;                    // "Sweep Taper Trace Length End(ms) (137-138)"
                else if (index == 138) return TaperType;                                     // "Taper Type (139-140)"
                else if (index == 140) return AliasFilterFrequency;                          // "Alias Filter Frequency (141-142)"
                else if (index == 142) return AliasFilterSlope;                              // "Alias Filter Slope (143-144)"
                else if (index == 144) return NotchFilterFrequency;                          // "Notch Filter Frequency (145-146)"
                else if (index == 146) return NotchFilterSlope;                              // "Notch Filter Slope (147-148)"
                else if (index == 148) return LowCutFrequency;                               // "Low Cut Frequency (149-150)"
                else if (index == 150) return HighCutFrequency;                              // "High Cut Frequency (151-152)"
                else if (index == 152) return LowCutSlope;                                   // "Low Cut Slope (153-154)"
                else if (index == 154) return HighCutSlope;                                  // "High Cut Slope (155-156)"
                else if (index == 156) return Yr;                                            // "Year (157-158)"
                else if (index == 158) return Day;                                           // "Day (159-160)"
                else if (index == 160) return Hour;                                          // "Hour (161-162)"
                else if (index == 162) return Minute;                                        // "Minute (163-164)"
                else if (index == 164) return Second;                                        // "Second (165-166)"
                else if (index == 166) return TimeBasis;                                     // "Time Basis (167-168)"
                else if (index == 168) return TraceWeightFactor;                             // "Trace Weight Factor (169-170)"
                else if (index == 170) return GeophoneGroupNumOfRollSwitchPositionOne;       // "Geophone Group Number of Roll Switch Position One (171-172)"
                else if (index == 172) return GeophoneGroupNumOfFirstTraceOrigRecord;        // "Geophone Group Number of First Trace Original Rec (173-174)"
                else if (index == 174) return GeophoneGroupNumOfLastTraceOrigRecord;         // "Geophone Group Number of Last Trace Original Rec (175-176)"
                else if (index == 176) return GapSize;                                       // "Gap Size (177-178)"
                else if (index == 178) return TaperOverTravel;                               // "Tape Over Travel (179-180)"
                else if (index == 180) return XCdp;                                          // (181-184)
                else if (index == 184) return YCdp;                                          // (185-188)
                else if (index == 188) return Inline;                                        // (189-192)
                else if (index == 192) return Crossline;                                     // (193-196)
                else if (index == 196) return ShotPoint;                                     // (197-200)
                else if (index == 200) return ShotPointScalar;                               // (201-202)
                else if (index == 202) return TraceValueMeasurementUnit;                     // (203-204)
                else if (index == 204) return TransductionConstantMantissa;                  // (205-208)
                else if (index == 208) return TransductionConstantPower;                     // (209-210)
                else if (index == 210) return TransductionUnit;                              // (211-212)
                else if (index == 212) return TraceIdentifier;                               // (213-214)
                else if (index == 214) return ScalarTraceHeader;                             // (215-216)
                else if (index == 216) return SourceType;                                    // (217-218)
                else if (index == 218) return SourceEnergyDirectionMantissa;                 // (219-222)
                else if (index == 222) return SourceEnergyDirectionExponent;                 // (223-224)
                else if (index == 224) return SourceMeasurementMantissa;                     // (225-228)
                else if (index == 228) return SourceMeasurementExponent;                     // (229-230)
                else if (index == 230) return SourceMeasurementUnit;                         // (231-232)
                else if (index == 232) return UnassignedInt1;                                // (233-236)
                else if (index == 236) return UnassignedInt2;                                // (237-240)
                else
                {
                    throw new Exception($"trace header property starting index {index} does not exist");
                }
            }
        }

        public byte[] ToBytes(EndianBitConverter bitConverter)
        {
            byte[] buffer = new byte[SgyFile.TraceHeaderBytesCount];
            ToBytes(buffer, 0, bitConverter);
            return buffer;
        }

        public void ToBytes(byte[] buffer, int offset, EndianBitConverter bitConverter)
        {
            bitConverter.CopyBytes(TraceNumInLine, buffer, offset + offset + 0);
            bitConverter.CopyBytes(TraceNumInFile, buffer, offset + offset + 4);
            bitConverter.CopyBytes(ShotNumOrStackTraceNum, buffer, offset + offset + 8);
            bitConverter.CopyBytes(TraceNumInShot, buffer, offset + 12);
            bitConverter.CopyBytes(EnergySourcePtNum, buffer, offset + 16);
            bitConverter.CopyBytes(CdpNum, buffer, offset + 20);
            bitConverter.CopyBytes(TraceNumber, buffer, offset + 24);
            bitConverter.CopyBytes((Int16)TraceId, buffer, offset + 28);
            bitConverter.CopyBytes((Int16)NumVerticalStackedTraces, buffer, offset + 30);
            bitConverter.CopyBytes((Int16)CdpFold, buffer, offset + 32);
            bitConverter.CopyBytes((Int16)DataUse, buffer, offset + 34);
            bitConverter.CopyBytes(SourceReceiverDistance, buffer, offset + 36);
            bitConverter.CopyBytes(RecieverGroupElevation, buffer, offset + 40);
            bitConverter.CopyBytes(SurfaceElevationAtSource, buffer, offset + 44);
            bitConverter.CopyBytes(SourceDepthBelowSurf, buffer, offset + 48);
            bitConverter.CopyBytes(DatumElevAtRecieverGroup, buffer, offset + 52);
            bitConverter.CopyBytes(DatumElevationAtSource, buffer, offset + 56);
            bitConverter.CopyBytes(WaterDepthAtSource, buffer, offset + 60);
            bitConverter.CopyBytes(WaterDepthAtRecieverGroup, buffer, offset + 64);
            bitConverter.CopyBytes((Int16)ScalarForElevationAndDepth, buffer, offset + 68);
            bitConverter.CopyBytes((Int16)ScalarForCoordinates, buffer, offset + 70);
            bitConverter.CopyBytes(XSourceCoordinate, buffer, offset + 72);
            bitConverter.CopyBytes(YSourceCoordinate, buffer, offset + 76);
            bitConverter.CopyBytes(XRecieverGroupCoordinate, buffer, offset + 80);
            bitConverter.CopyBytes(YRecieverGroupCoordinate, buffer, offset + 84);
            bitConverter.CopyBytes((Int16)CoordinateUnit, buffer, offset + 88);
            bitConverter.CopyBytes((Int16)WeatheringVelocity, buffer, offset + 90);
            bitConverter.CopyBytes((Int16)SubweatheringVelocity, buffer, offset + 92);
            bitConverter.CopyBytes((Int16)UpholeTimeAtSource, buffer, offset + 94);
            bitConverter.CopyBytes((Int16)UpholeTimeAtReceiverGroup, buffer, offset + 96);
            bitConverter.CopyBytes((Int16)SourceStaticCorrection, buffer, offset + 98);
            bitConverter.CopyBytes((Int16)ReceiverGroupStaticCorrection, buffer, offset + 100);
            bitConverter.CopyBytes((Int16)TotalStaticApplied, buffer, offset + 102);
            bitConverter.CopyBytes((Int16)HeaderTimeBreakLagMs, buffer, offset + 104);
            bitConverter.CopyBytes((Int16)TimeBreakShotLagMs, buffer, offset + 106);
            bitConverter.CopyBytes((Int16)ShotRecordingLag, buffer, offset + 108);
            bitConverter.CopyBytes((Int16)MuteTimeStart, buffer, offset + 110);
            bitConverter.CopyBytes((Int16)MuteTimeEnd, buffer, offset + 112);
            bitConverter.CopyBytes((UInt16)SampleCount, buffer, offset + 114);
            bitConverter.CopyBytes((Int16)SampleIntervalMs, buffer, offset + 116);
            bitConverter.CopyBytes((Int16)GainType, buffer, offset + 118);
            bitConverter.CopyBytes((Int16)GainConst, buffer, offset + 120);
            bitConverter.CopyBytes((Int16)EarlyGainDb, buffer, offset + 122);
            bitConverter.CopyBytes((Int16)Correlated, buffer, offset + 124);
            bitConverter.CopyBytes((Int16)SweepFrequencyStart, buffer, offset + 126);
            bitConverter.CopyBytes((Int16)SweepFrequencyEnd, buffer, offset + 128);
            bitConverter.CopyBytes((Int16)SweepLengthMs, buffer, offset + 130);
            bitConverter.CopyBytes((Int16)SweepType, buffer, offset + 132);
            bitConverter.CopyBytes((Int16)SweepTaperTraceLengthStartMs, buffer, offset + 134);
            bitConverter.CopyBytes((Int16)SweepTaperTraceLengthEndMs, buffer, offset + 136);
            bitConverter.CopyBytes((Int16)TaperType, buffer, offset + 138);
            bitConverter.CopyBytes((Int16)AliasFilterFrequency, buffer, offset + 140);
            bitConverter.CopyBytes((Int16)AliasFilterSlope, buffer, offset + 142);
            bitConverter.CopyBytes((Int16)NotchFilterFrequency, buffer, offset + 144);
            bitConverter.CopyBytes((Int16)NotchFilterSlope, buffer, offset + 146);
            bitConverter.CopyBytes((Int16)LowCutFrequency, buffer, offset + 148);
            bitConverter.CopyBytes((Int16)HighCutFrequency, buffer, offset + 150);
            bitConverter.CopyBytes((Int16)LowCutSlope, buffer, offset + 152);
            bitConverter.CopyBytes((Int16)HighCutSlope, buffer, offset + 154);
            bitConverter.CopyBytes((Int16)Yr, buffer, offset + 156);
            bitConverter.CopyBytes((Int16)Day, buffer, offset + 158);
            bitConverter.CopyBytes((Int16)Hour, buffer, offset + 160);
            bitConverter.CopyBytes((Int16)Minute, buffer, offset + 162);
            bitConverter.CopyBytes((Int16)Second, buffer, offset + 164);
            bitConverter.CopyBytes((Int16)TimeBasis, buffer, offset + 166);
            bitConverter.CopyBytes((Int16)TraceWeightFactor, buffer, offset + 168);
            bitConverter.CopyBytes((Int16)GeophoneGroupNumOfRollSwitchPositionOne, buffer, offset + 170);
            bitConverter.CopyBytes((Int16)GeophoneGroupNumOfFirstTraceOrigRecord, buffer, offset + 172);
            bitConverter.CopyBytes((Int16)GeophoneGroupNumOfLastTraceOrigRecord, buffer, offset + 174);
            bitConverter.CopyBytes((Int16)GapSize, buffer, offset + 176);
            bitConverter.CopyBytes((Int16)TaperOverTravel, buffer, offset + 178);
            bitConverter.CopyBytes(XCdp, buffer, offset + 180);
            bitConverter.CopyBytes(YCdp, buffer, offset + 184);
            bitConverter.CopyBytes(Inline, buffer, offset + 188);
            bitConverter.CopyBytes(Crossline, buffer, offset + 192);
            bitConverter.CopyBytes(ShotPoint, buffer, offset + 196);
            bitConverter.CopyBytes((Int16)ShotPointScalar, buffer, offset + 200);
            bitConverter.CopyBytes((Int16)TraceValueMeasurementUnit, buffer, offset + 202);
            bitConverter.CopyBytes(TransductionConstantMantissa, buffer, offset + 204);
            bitConverter.CopyBytes((Int16)TransductionConstantPower, buffer, offset + 208);
            bitConverter.CopyBytes((Int16)TransductionUnit, buffer, offset + 210);
            bitConverter.CopyBytes((Int16)TraceIdentifier, buffer, offset + 212);
            bitConverter.CopyBytes((Int16)ScalarTraceHeader, buffer, offset + 214);
            bitConverter.CopyBytes((Int16)SourceType, buffer, offset + 216);
            bitConverter.CopyBytes(SourceEnergyDirectionMantissa, buffer, offset + 218);
            bitConverter.CopyBytes((Int16)SourceEnergyDirectionExponent, buffer, offset + 222);
            bitConverter.CopyBytes(SourceMeasurementMantissa, buffer, offset + 224);
            bitConverter.CopyBytes((Int16)SourceMeasurementExponent, buffer, offset + 228);
            bitConverter.CopyBytes((Int16)SourceMeasurementUnit, buffer, offset + 230);
            bitConverter.CopyBytes(UnassignedInt1, buffer, offset + 232);
            bitConverter.CopyBytes(UnassignedInt2, buffer, offset + 236);
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Utility;
using Seismic.SegyFileIo;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Test.Seismic.SegyFileIo
{
    [TestClass]
    public class SegyReaderTest
    {
        static FileInfo teapotDome3DFileInfo = new FileInfo(@"..\..\Data\TeapotDome3D\filt_mig_small.sgy");

        [TestMethod]
        public void ReadTeapotDome3D()
        {
            using (SegyReader reader = new SegyReader(teapotDome3DFileInfo))
            {
                var fileTextualHeaders = reader.FileTextualHeaders;

                CodeContract.Assume(fileTextualHeaders.Length == 1, "There should be at least 1 text file header in the file.");
                CodeContract.Assume(reader.TraceCount > 0);
                CodeContract.Assume(reader.FileBinaryHeader.SamplesPerTraceOfFile > 0);

                var traces = reader.ReadTraces();
                var traceCount = traces.Count();
                var minmax = reader.GetAmplitudeRange();

                CodeContract.Assume(traceCount == reader.TraceCount);
            }
        }

        [TestMethod]
        public void TestCopyNewFileCreated()
        {
            FileInfo fileInfo = new FileInfo(@"test_segy_file.sgy");
            using (SegyWriter writer = new SegyWriter(fileInfo))
            {

                short dt = 3;
                ushort ns = 4001;
                int traceCount = 21230;
                SegyFileHeader fileHeader = new SegyFileHeader(dt, (short)ns);

                StringBuilder fileTextHeaderSb = new StringBuilder();
                fileTextHeaderSb.Append("This is an overflowing header - nick brooks");
                fileTextHeaderSb.Append(new String('-', 4500));
                writer.Write(fileTextHeaderSb.ToString());
                writer.Write(fileHeader);

                // test single trace write
                for (int i = 0; i < traceCount/2; i++)
                {
                    SegyTrace trace = new SegyTrace(ns);
                    for (int si = 0; si < ns; si++)
                    {
                        trace.Data[si] = si * i;
                    }
                    writer.Write(trace);
                }

                // test bulk trace write
                List<SegyTrace> traces = new List<SegyTrace>();
                for (int i = traceCount/2; i < traceCount; i++)
                {
                    SegyTrace trace = new SegyTrace(ns);
                    for (int si = 0; si < ns; si++)
                    {
                        trace.Data[si] = si * i;
                    }
                    traces.Add(trace);
                }
                writer.Write(traces);
            }

            CopySegyFileAndCompareToOriginal(fileInfo);

            fileInfo.Delete();
        }

        [TestMethod]
        public void TestCopyFilteredMigrationTeapotDome3D()
        {
            CopySegyFileAndCompareToOriginal(teapotDome3DFileInfo);
        }

        public void CopySegyFileAndCompareToOriginal(FileInfo oldFileInfo)
        {
            var newFile = oldFileInfo.FullName.Replace(".sgy", "_tmp.sgy");

            FileInfo newFileInfo = new FileInfo($@"{newFile}");
            using (SegyReader reader = new SegyReader(oldFileInfo))
            {
                using (SegyWriter writer = new SegyWriter(newFileInfo))
                {
                    writer.Write(reader.FileTextualHeaders[0]);
                    writer.Write(reader.FileBinaryHeader);
                    writer.Write(reader.ReadTraces());
                }
            }

            using (SegyReader oldFileReader = new SegyReader(oldFileInfo))
            {
                using (SegyReader newFileReader = new SegyReader(newFile))
                {
                    CodeContract.Assume(oldFileReader.FileTextualHeaders[0] == newFileReader.FileTextualHeaders[0]);
                    CodeContract.Assume(oldFileReader.FileBinaryHeader.Equals(newFileReader.FileBinaryHeader));

                    var oldTraces = oldFileReader.ReadTraces().ToArray();
                    var newTraces = newFileReader.ReadTraces().ToArray();

                    CodeContract.Assume(oldTraces.Length == newTraces.Length);

                    var traceCount = oldTraces.Length;
                    for (int i = 0; i < traceCount; i++)
                    {
                        var oldTrace = oldTraces[i];
                        var newTrace = newTraces[i];

                        CodeContract.Assume(oldTrace.Equals(newTrace));
                    }
                }
            }
            newFileInfo.Delete();
        }
    }
}

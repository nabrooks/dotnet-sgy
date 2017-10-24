using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Utility;
using Seismic.SegyFileIo;
using System.Linq;

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


    }
}

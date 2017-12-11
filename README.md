# Seismic

A set of utilities related to reading and writing seg-y files (.sgy or .segy society of exploration geophysicists "y" file format for seismic data).
The file format structure can be referenced at : https://en.wikipedia.org/wiki/SEG-Y.

# Tests
```C#
FileInfo teapotDome3DFileInfo = new FileInfo(@"..\..\Data\TeapotDome3D\filt_mig_small.sgy");
using (SegyReader reader = new SegyReader(teapotDome3DFileInfo))
{
    var fileTextualHeaders = reader.FileTextualHeaders;
    var fileBinaryHeader = reader.FileBinaryHeader;
    var isLittleEndian = reader.IsLittleEndian
    
    CodeContract.Assume(fileTextualHeaders.Length == 1, "There should be at least 1 text file header in the file.");
    CodeContract.Assume(reader.TraceCount > 0);
    CodeContract.Assume(reader.FileBinaryHeader.SamplesPerTraceOfFile > 0);

    var traces = reader.ReadTraces();
    var traceCount = traces.Count();
    var minmax = reader.GetAmplitudeRange();

    CodeContract.Assume(traceCount == reader.TraceCount);
}
```

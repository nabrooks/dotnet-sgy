using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismic.SegyFileIo
{
    /// <summary>
    /// The data format code of a Segy file. Data sample values may be written in any of 7 different encodings.
    /// This format code is used to describe how the file sample values were written.
    /// </summary>
    public enum FormatCode : short
    {
        None = 0,
        IbmFloatingPoint4 = 1,
        TwosComplementInteger4 = 2,
        TwosComplementInteger2 = 3,
        FixedPointWithGain4 = 4,
        IeeeFloatingPoint4 = 5,
        Unused1 = 6,
        Unused2 = 7,
        TwosComplementInteger1 = 8
    };
}

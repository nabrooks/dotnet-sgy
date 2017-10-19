using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeismicFileIo
{
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

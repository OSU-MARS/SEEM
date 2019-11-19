using System;

namespace Osu.Cof.Organon.Test
{
    [Flags]
    public enum OrganonWarnings
    {
        None = 0x0,
        LessThan50TreeRecords = 0x1,
        MortalitySiteIndex = 0x2
    }
}

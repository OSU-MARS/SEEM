using System;

namespace Osu.Cof.Ferm.Test
{
    [Flags]
    public enum OrganonWarnings
    {
        None = 0x0,
        LessThan50TreeRecords = 0x1,
        HemlockSiteIndex = 0x2
    }
}

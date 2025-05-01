using System;

namespace Mars.Seem.Test
{
    [Flags]
    public enum OrganonWarnings : byte
    {
        None = 0x00,
        LessThan50TreeRecords = 0x01,
        HemlockSiteIndex = 0x02
    }
}

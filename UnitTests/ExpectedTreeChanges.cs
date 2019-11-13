using System;

namespace Osu.Cof.Organon.Test
{
    [Flags]
    public enum ExpectedTreeChanges
    {
        NoDiameterOrHeightGrowth = 0x0,
        DiameterGrowth = 0x1,
        HeightGrowth = 0x2,
        HeightGrowthOrNoChange = 0x4
    }
}

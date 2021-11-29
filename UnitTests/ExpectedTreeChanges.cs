using System;

namespace Mars.Seem.Test
{
    [Flags]
    public enum ExpectedTreeChanges
    {
        NoDiameterOrHeightGrowth = 0x0,
        DiameterGrowth = 0x1,
        DiameterGrowthOrNoChange = 0x2,
        ExpansionFactorConservedOrIncreased = 0x4,
        HeightGrowth = 0x8,
        HeightGrowthOrNoChange = 0x10
    }
}

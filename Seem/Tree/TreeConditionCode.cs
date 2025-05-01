using System;

namespace Mars.Seem.Tree
{
    [Flags]
    public enum TreeConditionCode : byte
    {
        None = 0x00,
        Live = 0x01,
        BrokenTop = 0x02,
        Cull = 0x04,
        Defect = 0x08,
        Marginal = 0x10,
        Reserve = 0x20,
        SiteTree = 0x40,
        Snag = 0x80
    }
}

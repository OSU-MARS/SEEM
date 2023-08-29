using Mars.Seem.Tree;
using System;

namespace Mars.Seem.Extensions
{
    internal static class TreeConditionCodeExtensions
    {
        public static TreeConditionCode Parse(string code)
        {
            return code switch
            {
                ".." => TreeConditionCode.Live,
                "DE" => TreeConditionCode.Defect,
                "OS" => TreeConditionCode.SiteTree,
                "BT" => TreeConditionCode.BrokenTop,
                "RT" => TreeConditionCode.Reserve,
                "C." => TreeConditionCode.Cull,
                "M." => TreeConditionCode.Marginal,
                _ => throw new NotSupportedException("Unhandled tree condition code '" + code + "'.")
            };
        }
    }
}

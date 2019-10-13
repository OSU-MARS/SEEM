namespace Osu.Cof.Organon
{
    internal static class Constant
    {
        public static class Maximum
        {
            public const float MSDI = 1000.0F;
            public const float SiteIndexInFeet = 300.0F;
        }

        public static class TreeIndex
        {
            public const int ShadowCrownRatio = 0;

            public static class Float
            {
                public const int DbhInInches = 0;
                public const int HeightInFeet = 1;
                public const int CrownRatio = 2;
                public const int ExpansionFactor = 3;
            }

            public static class Growth
            {
                public const int Height = 0;
                public const int Diameter = 1;
                public const int AccumulatedHeight = 2;
                public const int AccumulatedDiameter = 3;
            }

            public static class Integer
            {
                public const int Species = 0;
                public const int SpeciesGroup = 1;
                public const int User = 2;
            }
        }
    }
}

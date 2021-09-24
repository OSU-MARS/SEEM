namespace Osu.Cof.Ferm.Extensions
{
    // extensions to Math and MathF for syntactic convenience
    public static class MathE
    {
        public static float Max(float value1, float value2, float value3)
        {
            float maximumValue = value1;
            if (value2 > maximumValue)
            {
                maximumValue = value2;
            }
            if (value3 > maximumValue)
            {
                maximumValue = value3;
            }
            return maximumValue;
        }

        public static float Min(float value1, float value2, float value3)
        {
            float minimumValue = value1;
            if (value2 < minimumValue)
            {
                minimumValue = value2;
            }
            if (value3 < minimumValue)
            {
                minimumValue = value3;
            }
            return minimumValue;
        }

        public static float Min(float value1, float value2, float value3, float value4)
        {
            float minimumValue = value1;
            if (value2 < minimumValue)
            {
                minimumValue = value2;
            }
            if (value3 < minimumValue)
            {
                minimumValue = value3;
            }
            if (value4 < minimumValue)
            {
                minimumValue = value4;
            }
            return minimumValue;
        }

        public static float Min(float value1, float value2, float value3, float value4, float value5)
        {
            float minimumValue = value1;
            if (value2 < minimumValue)
            {
                minimumValue = value2;
            }
            if (value3 < minimumValue)
            {
                minimumValue = value3;
            }
            if (value4 < minimumValue)
            {
                minimumValue = value4;
            }
            if (value5 < minimumValue)
            {
                minimumValue = value5;
            }
            return minimumValue;
        }

        public static float Min(float value1, float value2, float value3, float value4, float value5, float value6)
        {
            float minimumValue = value1;
            if (value2 < minimumValue)
            {
                minimumValue = value2;
            }
            if (value3 < minimumValue)
            {
                minimumValue = value3;
            }
            if (value4 < minimumValue)
            {
                minimumValue = value4;
            }
            if (value5 < minimumValue)
            {
                minimumValue = value5;
            }
            if (value6 < minimumValue)
            {
                minimumValue = value6;
            }
            return minimumValue;
        }
    }
}

namespace Mars.Seem
{
    public enum SimdInstructions
    {
        /// <summary>
        /// AVX, AVX2, or (not currently used) FMA instructions at 256 bit width.
        /// </summary>
        Avx = 256,

        /// <summary>
        /// AVX10/256 instructions (EVEX).
        /// </summary>
        Avx10 = 10,

        /// <summary>
        /// AVX10/512 instructions (EVEX).
        /// </summary>
        Avx512 = 512,

        /// <summary>
        /// SSE, AVX, AVX2, or (not currently used) FMA VEX instructions at 128 bit width.
        /// </summary>
        Vex128 = 128
    }
}

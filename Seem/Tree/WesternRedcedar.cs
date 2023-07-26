using System;

namespace Mars.Seem.Tree
{
    internal class WesternRedcedar
    {
        public static TreeSpeciesProperties Properties { get; private set; }

        static WesternRedcedar()
        {
            // Miles PD, Smith BW. 2009. Specific gravity and other properties of wood and bark for 156 tree species found in North
            //   America (No. NRS-RN-38). Northern Research Station, US Forest Service. https://doi.org/10.2737/NRS-RN-38
            WesternRedcedar.Properties = new TreeSpeciesProperties(woodDensity: 433.0F, // kg/m³
                barkFraction: 0.106F,
                barkDensity: 577.0F, // kg/m³
                processingBarkLoss: 0.30F, // loss with spiked feed rollers
                yardingBarkLoss: 0.15F); // dragging abrasion loss over full corridor (if needed, this could be reparameterized to a function of corridor length)
        }

        public static float GetDiameterInsideBark(float dbhInCm, float heightInM, float evaluationHeightInM)
        {
            // Kozak A. 1988. A variable-exponent taper equation. Canadian Journal of Forest Research 18:1363-1368. https://doi.org/10.1139/x88-213
            // simplified from Table 5 for Equation 8, see R
            if ((dbhInCm < 0.0F) || (dbhInCm > 160.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(dbhInCm), "Diameter of " + dbhInCm.ToString(Constant.Default.DiameterInCmFormat) + " cm is either negative or exceeds regression limit of 135.0 cm.");
            }
            if ((heightInM < 0.0F) || (heightInM > 75.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(heightInM), "Height of " + heightInM.ToString(Constant.Default.HeightInMFormat) + " m is either less than the Kozak 2004 regression form's minimum of 1.3 m or exceeds regression limit of 75.0 m.");
            }
            if ((evaluationHeightInM < 0.0F) || (evaluationHeightInM > heightInM))
            {
                throw new ArgumentOutOfRangeException(nameof(evaluationHeightInM), "Evaluation height of " + evaluationHeightInM.ToString(Constant.Default.HeightInMFormat) + " m is negative or exceeds tree height of " + heightInM.ToString(Constant.Default.HeightInMFormat) + " m.");
            }
            if (evaluationHeightInM == heightInM)
            {
                return 0.0F; // requires special casing since z = 1 ⇒ z = 0 ⇒ lnX = -∞ ⇒ dibInCM = NaN
            }

            float z = evaluationHeightInM / heightInM;
            float x = 2.211032225F * (1.0F - MathF.Sqrt(z)); // 1 / (1 - sqrt(0.30)) = 2.211032225007380
            float lnX = MathF.Log(x);
            float dibInCm = 1.21697F * MathF.Exp(0.84256F * MathF.Log(dbhInCm) + 9.99995E-6F * dbhInCm + 1.55322F * lnX * z * z - 0.39719F * lnX * MathF.Log(z + 0.001F) + 2.11018F * lnX * MathF.Sqrt(z) - 1.11416F * lnX * MathF.Exp(z) + 0.09420F * lnX * (dbhInCm / heightInM));
            return dibInCm;
        }

        public static float GetNeiloidHeight(float dbhInCm, float heightInM)
        {
            // approximation from plotting families of Kozak 1988 dib curves in R and fitting the neiloid inflection point
            // from linear regressions in WesternRedcedar.R
            float heightDiameterRatio = heightInM / (0.01F * dbhInCm);
            float neiloidHeightInM = -0.35F - 1.0F / (0.026F * heightDiameterRatio) + 0.01F * (2.0F + 0.038F * heightDiameterRatio) * dbhInCm;
            return MathF.Max(neiloidHeightInM, Constant.Bucking.DefaultStumpHeightInM);
        }
    }
}

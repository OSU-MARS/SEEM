using System;

namespace Osu.Cof.Organon
{
    public class Stand
    {
        // time since last stand replacing disturbance
        public int AgeInYears { get; set; }

        // time since oldest cohort of trees in the stand reached breast height (4.5 feet) (DOUG?)
        public int BreastHeightAgeInYears { get; set; }

        // accumulated expansion factor of dead trees from mortality chipping? (DOUG?)
        public float[] DeadExpansionFactor { get; private set; }

        // TDATAR[, 5] is variously assigned in rundll and wood quality to DBH, CR, and EF but is never consumed
        //   this suggests it's an output trace
        //     0    1       2            3                 4                     5  6              7
        //     DBH, height, crown ratio, expansion factor, crown ratio? (DOUG?), ?, crown ratio 2, expansion factor 2
        //                                                 expansion factor?
        // { { DBH, HT,     CR,          EF,               unused?,              ?, CR t + 1?,     CR t + 1? } }
        // TODO: can this be reduced to width 4 rather than 8?
        public float[,] Float { get; private set; }

        //     0,             1                2,                         3
        //     height growth, diameter growth, accumulated height growth, accumulated diameter growth 
        // { { HGRO,          DGRO,            GROWTH + GROWTH,           GROWTH + GROWTH } }
        public float[,] Growth { get; private set; }

        // valid range for species group is [0, 17]
        //     0        1              2
        //     species, species group, user data passthrough, not used by Organon
        // { { ISP,     ISPGRP,        USER } }
        public int[,] Integer { get; private set; }

        // IB, sometimes also named IIB
        public int MaxBigSixSpeciesGroupIndex { get; private set; }

        // must be greater than zero when thinning is enabled? (DOUG?)
        public float[] MGExpansionFactor { get; private set; }

        // number of plots? (DOUG?)
        public int NPTS { get; private set; }

        // primary site index
        public float PrimarySiteIndex { get; private set; }

        // secondary site index used for additional mortality calculations
        public float MortalitySiteIndex { get; private set; }

        // shadown crown ratio, apparently unused (DOUG?)
        public float[,] ShadowCrownRatio { get; private set; }

        public int[] StandWarnings { get; private set; }

        public int[] TreeWarnings { get; private set; }

        public int TreeRecordsInUse { get; set; }

        protected Stand(Stand other)
            : this(other.AgeInYears, other.MaximumTreeRecords, other.PrimarySiteIndex, other.MortalitySiteIndex, other.MaxBigSixSpeciesGroupIndex)
        {
            this.BreastHeightAgeInYears = other.BreastHeightAgeInYears;

            other.DeadExpansionFactor.CopyTo(this.DeadExpansionFactor, 0);
            Buffer.BlockCopy(other.Float, 0, this.Float, 0, sizeof(float) * other.Float.Length);
            Buffer.BlockCopy(other.Growth, 0, this.Growth, 0, sizeof(float) * other.Growth.Length);
            Buffer.BlockCopy(other.Integer, 0, this.Integer, 0, sizeof(int) * other.Integer.Length);
            other.MGExpansionFactor.CopyTo(this.MGExpansionFactor, 0);
            this.NPTS = other.NPTS;
            Buffer.BlockCopy(other.ShadowCrownRatio, 0, this.ShadowCrownRatio, 0, sizeof(float) * other.ShadowCrownRatio.Length);
            this.TreeRecordsInUse = other.TreeRecordsInUse;
            other.StandWarnings.CopyTo(this.StandWarnings, 0);
            other.TreeWarnings.CopyTo(this.TreeWarnings, 0);
        }

        protected Stand(int ageInYears, int treeCount, float primarySiteIndex, float mortalitySiteIndex, int maxBigSixSpeciesGroupIndex)
        {
            this.AgeInYears = ageInYears;
            this.BreastHeightAgeInYears = ageInYears;
            this.DeadExpansionFactor = new float[treeCount];
            this.Float = new float[treeCount, 8];
            this.Growth = new float[treeCount, 4];
            this.Integer = new int[treeCount, 3];
            this.MaxBigSixSpeciesGroupIndex = maxBigSixSpeciesGroupIndex;
            this.MGExpansionFactor = new float[treeCount];
            this.NPTS = 1;
            this.PrimarySiteIndex = primarySiteIndex;
            this.MortalitySiteIndex = mortalitySiteIndex;
            this.ShadowCrownRatio = new float[treeCount, 3];
            this.StandWarnings = new int[9];
            this.TreeWarnings = new int[treeCount];
            this.TreeRecordsInUse = 0;
        }

        public int MaximumTreeRecords
        {
            get { return this.MGExpansionFactor.Length; }
        }

        public bool IsBigSixSpecies(int treeIndex)
        {
            return this.Integer[treeIndex, Constant.TreeIndex.Integer.SpeciesGroup] <= this.MaxBigSixSpeciesGroupIndex;
        }
    }
}
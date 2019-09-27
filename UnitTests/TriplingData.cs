using System;

namespace Osu.Cof.Organon.Test
{
    public class TriplingData
    {
        // pruning
        // PRAGE
        public int[,] PruningAge { get; private set; }
        // PRCR
        public float[,] PruningCrownRatio { get; private set; }
        // PRDBH
        public float[,] PruningDbhInInches { get; private set; }
        // PREXP (DOUG? expansion factor of trees pruned?)
        public float[,] PREXP { get; private set; }
        // PRHT
        public float[,] PruningHeightInFeet { get; private set; }
        // PRLH (DOUG?)
        public float[,] PruningLH { get; private set; }

        // wood quality
        // BRCNT
        public int[,] BranchCount { get; private set; }
        // BRDIA
        public int[,] BranchDiameter { get; private set; }
        // BRHT
        public int[,] BranchHeight { get; private set; }
        // JCORE
        public int[,] JuvenileCore { get; private set; }

        // volumes
        // NPR
        public int[] NPR { get; private set; }
        // POINT
        public int[] POINT { get; private set; }
        // SYTVOL
        public float[,] SYTVOL { get; private set; }
        // TREENO
        public int[] TREENO { get; private set; }
        // VOLTR
        public float[,] VOLTR { get; private set; }

        public TriplingData(int treeCount)
        {
            this.PruningAge = new int[treeCount, 3];
            this.PruningCrownRatio = new float[treeCount, 3];
            this.PruningDbhInInches = new float[treeCount, 3];
            this.PREXP = new float[treeCount, 3];
            this.PruningHeightInFeet = new float[treeCount, 3];
            this.PruningLH = new float[treeCount, 3];

            this.BranchCount = new int[treeCount, 3];
            this.BranchDiameter = new int[treeCount, 40];
            this.BranchHeight = new int[treeCount, 40];
            this.JuvenileCore = new int[treeCount, 40];

            this.NPR = new int[treeCount];
            this.POINT = new int[treeCount];
            this.SYTVOL = new float[treeCount, 3];
            this.TREENO = new int[treeCount];
            this.VOLTR = new float[treeCount, 4];
        }

        public TriplingData(TriplingData other)
            : this(other.PruningAge.Length)
        {
            Buffer.BlockCopy(other.PruningAge, 0, this.PruningAge, 0, sizeof(int) * other.PruningAge.Length);
            Buffer.BlockCopy(other.PruningCrownRatio, 0, this.PruningCrownRatio, 0, sizeof(float) * other.PruningCrownRatio.Length);
            Buffer.BlockCopy(other.PruningDbhInInches, 0, this.PruningDbhInInches, 0, sizeof(float) * other.PruningDbhInInches.Length);
            Buffer.BlockCopy(other.PREXP, 0, this.PREXP, 0, sizeof(float) * other.PREXP.Length);
            Buffer.BlockCopy(other.PruningHeightInFeet, 0, this.PruningHeightInFeet, 0, sizeof(float) * other.PruningHeightInFeet.Length);
            Buffer.BlockCopy(other.PruningLH, 0, this.PruningLH, 0, sizeof(float) * other.PruningLH.Length);

            Buffer.BlockCopy(other.BranchCount, 0, this.BranchCount, 0, sizeof(int) * other.BranchCount.Length);
            Buffer.BlockCopy(other.BranchDiameter, 0, this.BranchDiameter, 0, sizeof(int) * other.BranchDiameter.Length);
            Buffer.BlockCopy(other.BranchHeight, 0, this.BranchHeight, 0, sizeof(int) * other.BranchHeight.Length);
            Buffer.BlockCopy(other.JuvenileCore, 0, this.JuvenileCore, 0, sizeof(int) * other.JuvenileCore.Length);

            other.NPR.CopyTo(this.NPR, 0);
            other.POINT.CopyTo(this.POINT, 0);
            Buffer.BlockCopy(other.SYTVOL, 0, this.SYTVOL, 0, sizeof(float) * other.SYTVOL.Length);
            other.TREENO.CopyTo(this.TREENO, 0);
            Buffer.BlockCopy(other.VOLTR, 0, this.VOLTR, 0, sizeof(float) * other.VOLTR.Length);
        }
    }
}

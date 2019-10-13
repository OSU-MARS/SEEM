using System;

namespace Osu.Cof.Organon
{
    internal class Triple
    {
        /// <summary>
        /// Clones entry for tree twice and sets pruning, wood quality, and volume variables.
        /// </summary>
        /// <param name="sourceTreeIndex">Index of tree to clone.</param>
        /// <param name="pruningAndVolumeDestinationTreeIndex">Index to assign clone of tree K to. Tree J + 1 is also set to a copy of tree M.</param>
        /// <param name="triplingMethod">Flag indicating whether space is available in the tree data arrays for cloning the tree?</param>
        /// <param name="ON">Unused unless M = 2, in which case ON = 0 bypasses tripling.</param>
        /// <param name="IWQ">Initial number of tree records of big six species.</param>
        /// <param name="WOODQ"></param>
        /// <param name="stand">Stand data.</param>
        /// <param name="bigSixDestinationTreeIndex">Location to clone wood quality variables to and total number of trees of big six species after tripling.</param>
        /// <param name="POINT"></param>
        /// <param name="TREENO"></param>
        /// <param name="PRAGE"></param>
        /// <param name="BRCNT"></param>
        /// <param name="BRHT"></param>
        /// <param name="BRDIA"></param>
        /// <param name="JCORE"></param>
        /// <param name="NPR"></param>
        /// <param name="PRLH"></param>
        /// <param name="PRDBH"></param>
        /// <param name="PRHT"></param>
        /// <param name="PRCR"></param>
        /// <param name="PREXP"></param>
        /// <param name="SCR"></param>
        /// <param name="VOLTR"></param>
        /// <param name="SYTVOL"></param>
        public static void XTRIP(int sourceTreeIndex, int pruningAndVolumeDestinationTreeIndex, int triplingMethod, int ON, int IWQ, bool WOODQ, Stand stand, int bigSixDestinationTreeIndex, int[] POINT, int[] TREENO,
                                 int[,] PRAGE, int[,] BRCNT, int[,] BRHT, int[,] BRDIA, int[,] JCORE, int[] NPR, float[,] PRLH,
                                 float[,] PRDBH, float[,] PRHT, float[,] PRCR, float[,] PREXP, float[,] SCR, float[,] VOLTR, float[,] SYTVOL)
        {
            // tripling of values other than basic tree attributes
            if (triplingMethod == 0 || (triplingMethod == 2 && ON == 0))
            {
                return;
            }

            // BUGBUG doesn't increment BIG6, so has implicit assumptions of a big six species and being paired with a DGTRIP() or HGTRIP() call
            if (pruningAndVolumeDestinationTreeIndex > PRLH.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(pruningAndVolumeDestinationTreeIndex));
            }


            // pruning variables
            for (int pruningIndex = 0; pruningIndex < 3; ++pruningIndex)
            {
                PRLH[pruningAndVolumeDestinationTreeIndex, pruningIndex] = PRLH[sourceTreeIndex, pruningIndex];
                PRLH[pruningAndVolumeDestinationTreeIndex + 1, pruningIndex] = PRLH[sourceTreeIndex, pruningIndex];
                PRAGE[pruningAndVolumeDestinationTreeIndex, pruningIndex] = PRAGE[sourceTreeIndex, pruningIndex];
                PRAGE[pruningAndVolumeDestinationTreeIndex + 1, pruningIndex] = PRAGE[sourceTreeIndex, pruningIndex];
                PRDBH[pruningAndVolumeDestinationTreeIndex, pruningIndex] = PRDBH[sourceTreeIndex, pruningIndex];
                PRDBH[pruningAndVolumeDestinationTreeIndex + 1, pruningIndex] = PRDBH[sourceTreeIndex, pruningIndex];
                PRHT[pruningAndVolumeDestinationTreeIndex, pruningIndex] = PRHT[sourceTreeIndex, pruningIndex];
                PRHT[pruningAndVolumeDestinationTreeIndex + 1, pruningIndex] = PRHT[sourceTreeIndex, pruningIndex];
                PRCR[pruningAndVolumeDestinationTreeIndex, pruningIndex] = PRCR[sourceTreeIndex, pruningIndex];
                PRCR[pruningAndVolumeDestinationTreeIndex + 1, pruningIndex] = PRCR[sourceTreeIndex, pruningIndex];
                PREXP[pruningAndVolumeDestinationTreeIndex, pruningIndex] = PREXP[sourceTreeIndex, pruningIndex];
                PREXP[pruningAndVolumeDestinationTreeIndex + 1, pruningIndex] = PREXP[sourceTreeIndex, pruningIndex];
                SCR[pruningAndVolumeDestinationTreeIndex, pruningIndex] = SCR[sourceTreeIndex, pruningIndex];
                SCR[pruningAndVolumeDestinationTreeIndex + 1, pruningIndex] = SCR[sourceTreeIndex, pruningIndex];
            }

            // wood quality varaibles
            if (WOODQ && stand.IsBigSixSpecies(sourceTreeIndex))
            {
                for (int woodQualityIndex = 0; woodQualityIndex < 3; ++woodQualityIndex)
                {
                    BRCNT[bigSixDestinationTreeIndex, woodQualityIndex] = BRCNT[IWQ, woodQualityIndex];
                    BRCNT[bigSixDestinationTreeIndex + 1, woodQualityIndex] = BRCNT[IWQ, woodQualityIndex];
                }

                for (int woodQualityIndex = 0; woodQualityIndex < 40; ++woodQualityIndex)
                {
                    BRHT[bigSixDestinationTreeIndex, woodQualityIndex] = BRHT[IWQ, woodQualityIndex];
                    BRHT[bigSixDestinationTreeIndex + 1, woodQualityIndex] = BRHT[IWQ, woodQualityIndex];
                    BRDIA[bigSixDestinationTreeIndex, woodQualityIndex] = BRDIA[IWQ, woodQualityIndex];
                    BRDIA[bigSixDestinationTreeIndex + 1, woodQualityIndex] = BRDIA[IWQ, woodQualityIndex];
                    JCORE[bigSixDestinationTreeIndex, woodQualityIndex] = JCORE[IWQ, woodQualityIndex];
                    JCORE[bigSixDestinationTreeIndex + 1, woodQualityIndex] = JCORE[IWQ, woodQualityIndex];
                }
            }

            // TRIPLING VOLUMES
            VOLTR[pruningAndVolumeDestinationTreeIndex, 0] = VOLTR[sourceTreeIndex, 1];
            VOLTR[pruningAndVolumeDestinationTreeIndex, 3] = VOLTR[sourceTreeIndex, 3];
            VOLTR[pruningAndVolumeDestinationTreeIndex + 1, 1] = VOLTR[sourceTreeIndex, 1];
            VOLTR[pruningAndVolumeDestinationTreeIndex + 1, 2] = VOLTR[sourceTreeIndex, 3];
            SYTVOL[pruningAndVolumeDestinationTreeIndex, 0] = SYTVOL[sourceTreeIndex, 0];
            SYTVOL[pruningAndVolumeDestinationTreeIndex + 1, 0] = SYTVOL[sourceTreeIndex, 0];
            SYTVOL[pruningAndVolumeDestinationTreeIndex, 1] = SYTVOL[sourceTreeIndex, 1];
            SYTVOL[pruningAndVolumeDestinationTreeIndex + 1, 1] = SYTVOL[sourceTreeIndex, 1];
            NPR[pruningAndVolumeDestinationTreeIndex] = NPR[sourceTreeIndex];
            NPR[pruningAndVolumeDestinationTreeIndex + 1] = NPR[sourceTreeIndex];
            POINT[pruningAndVolumeDestinationTreeIndex] = POINT[sourceTreeIndex];
            POINT[pruningAndVolumeDestinationTreeIndex + 1] = POINT[sourceTreeIndex];
            TREENO[pruningAndVolumeDestinationTreeIndex] = TREENO[sourceTreeIndex];
            TREENO[pruningAndVolumeDestinationTreeIndex + 1] = TREENO[sourceTreeIndex];
        }

        /// <summary>
        /// Clones entry for tree twice with no change to the tree's expansion factor totalled across all three entries. Sets diameter growth.
        /// </summary>
        /// <param name="sourceTreeIndex">Index of tree to clone.</param>
        /// <param name="destinationTreeIndex">Index to assign clone of tree K to. Tree J + 1 is also set to a copy of tree M.</param>
        /// <param name="triplingMethod">Flag indicating whether space is available in the tree data arrays for cloning the tree?</param>
        /// <param name="ON">Unused unless M = 2, in which case ON = 0 bypasses tripling.</param>
        /// <param name="variant">Organon variant.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="bigSixTreeRecordCount">Total number of trees of big 6 species.</param>
        /// <param name="otherSpeciesTreeRecordCount">Total number of trees of other species.</param>
        /// <param name="TDATAR">Tree data.</param>
        /// <param name="GROWTH">Tree data.</param>
        /// <param name="MGEXP">Tree data.</param>
        /// <param name="DEADEXP">Tree data.</param>
        public static void DGTRIP(int sourceTreeIndex, ref int destinationTreeIndex, int triplingMethod, ref int ON, Variant variant, Stand stand, ref int bigSixTreeRecordCount, ref int otherSpeciesTreeRecordCount, float[,] TDATAR, float[,] GROWTH, float[] MGEXP, float[] DEADEXP)
        {
            // DO TRIPLING
            if (triplingMethod == 0 || (triplingMethod == 2 && ON == 0))
            {
                // BUGBUG ON appears to be a bool declared as an int but is hard coded to 0 in Execute2, so can be removed
                ON = 1;
            }
            else
            {
                if (destinationTreeIndex + 1 > MGEXP.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationTreeIndex));
                }

                // SPECIES, GROUP, UC, DBH, HT, CR, @HT & @DIAM,
                // CUM @HT & @DIAM = ORIGINAL TREE
                for (int integerIndex = 0; integerIndex < 3; ++integerIndex)
                {
                    stand.Integer[destinationTreeIndex, integerIndex] = stand.Integer[sourceTreeIndex, integerIndex];
                    stand.Integer[destinationTreeIndex + 1, integerIndex] = stand.Integer[sourceTreeIndex, integerIndex];
                    TDATAR[destinationTreeIndex, integerIndex] = TDATAR[sourceTreeIndex, integerIndex];
                    TDATAR[destinationTreeIndex + 1, integerIndex] = TDATAR[sourceTreeIndex, integerIndex];
                }

                // EXPANSION FACTORS ARE 1/3
                float A = TDATAR[sourceTreeIndex, 3] / 3.0F;
                TDATAR[destinationTreeIndex, 3] = A;
                TDATAR[destinationTreeIndex + 1, 3] = A;
                TDATAR[sourceTreeIndex, 3] = A;
                A = TDATAR[sourceTreeIndex, 7] / 3.0F;
                TDATAR[destinationTreeIndex, 7] = A;
                TDATAR[destinationTreeIndex + 1, 7] = A;
                TDATAR[sourceTreeIndex, 7] = A;
                A = TDATAR[sourceTreeIndex, 4] / 3.0F;
                TDATAR[destinationTreeIndex, 4] = A;
                TDATAR[destinationTreeIndex + 1, 4] = A;
                TDATAR[sourceTreeIndex, 4] = A;
                A = MGEXP[sourceTreeIndex] / 3.0F;
                MGEXP[destinationTreeIndex] = A;
                MGEXP[destinationTreeIndex + 1] = A;
                MGEXP[sourceTreeIndex] = A;
                A = DEADEXP[sourceTreeIndex] / 3.0F;
                DEADEXP[destinationTreeIndex] = A;
                DEADEXP[destinationTreeIndex + 1] = A;
                DEADEXP[sourceTreeIndex] = A;
                TDATAR[destinationTreeIndex, 5] = TDATAR[sourceTreeIndex, 5];
                TDATAR[destinationTreeIndex + 1, 5] = TDATAR[sourceTreeIndex, 5];
                TDATAR[destinationTreeIndex, 6] = TDATAR[sourceTreeIndex, 6];
                TDATAR[destinationTreeIndex + 1, 6] = TDATAR[sourceTreeIndex, 6];

                // INCREASE SPECIES COUNT
                if (stand.IsBigSixSpecies(sourceTreeIndex))
                {
                    bigSixTreeRecordCount += 2;
                }
                else
                {
                    otherSpeciesTreeRecordCount += 2;
                }

                int speciesGroup = stand.Integer[sourceTreeIndex, Constant.TreeIndex.Integer.SpeciesGroup];
                float LRES;
                float URES;
                switch (variant)
                {
                    // BUGBUG no check ISPGRP indicates a species which is in range for DGRES
                    case Variant.Swo:
                        DGRES_SWO(speciesGroup, out LRES, out URES);
                        break;
                    case Variant.Nwo:
                        DGRES_NWO(speciesGroup, out LRES, out URES);
                        break;
                    case Variant.Smc:
                        DGRES_SMC(speciesGroup, out LRES, out URES);
                        break;
                    case Variant.Rap:
                        DGRES_RAP(speciesGroup, out LRES, out URES);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                float DG = GROWTH[sourceTreeIndex, 1];
                float DGRO = DG - (LRES + URES) * (float)Math.Sqrt(DG);
                if (DGRO < 0.0F)
                {
                    DGRO = 0.0F;
                }
                float DGRO1 = DG + URES * (float)Math.Sqrt(DG);
                float DGRO2 = DG + LRES * (float)Math.Sqrt(DG);
                if (DGRO2 < 0.0F)
                {
                    DGRO2 = 0.0F;
                }
                GROWTH[sourceTreeIndex, 1] = DGRO;
                GROWTH[destinationTreeIndex, 1] = DGRO1;
                GROWTH[destinationTreeIndex, 3] = GROWTH[sourceTreeIndex, 3] + GROWTH[destinationTreeIndex, 1];
                GROWTH[destinationTreeIndex + 1, 1] = DGRO2;
                GROWTH[destinationTreeIndex + 1, 3] = GROWTH[sourceTreeIndex, 3] + GROWTH[destinationTreeIndex + 1, 1];
                GROWTH[destinationTreeIndex, 3] = GROWTH[sourceTreeIndex, 3];
                GROWTH[destinationTreeIndex + 1, 3] = GROWTH[sourceTreeIndex, 3];
                destinationTreeIndex += 2;
                ON = 0;
            }
            GROWTH[sourceTreeIndex, 3] = GROWTH[sourceTreeIndex, 3] + GROWTH[sourceTreeIndex, 1];
        }

        private static void DGRES_SWO(int ISPGRP, out float LRES, out float URES)
        {
            float[] DGLRES = {
                 -0.440F    , -0.4246576F, -0.3934351F, -0.4750919F,      // DF,GW,PP,SP
                 -0.3819283F, -0.4629461F, -0.3733441F, -0.2690408F,      // IC,WH,RC,PY
                 -0.3155211F, -0.3281635F, -0.3407614F, -0.2963305F,      // MD,GC,TA,CL
                 -0.3663556F, -0.38681203F,-0.2698731F, -0.4947154F,      // BL,WO,BO,RA
                 -0.3679208F, -0.3679208F                              // PD,WI
            };
            float[] DGURES = {
                  0.487F    ,  0.4822343F,  0.4619852F,  0.4977667F,      // DF,GW,PP,SP
                  0.4446445F,  0.5300564F,  0.4250894F,  0.3367567F,      // IC,WH,RC,PY
                  0.3843792F,  0.3975281F,  0.4190495F,  0.3788400F,      // MD,GC,TA,CL
                  0.4157338F,  0.46098924F, 0.3292178F,  0.6171431F,      // BL,WO,BO,RA
                  0.4444550F,  0.4444550F                              // PD,WI
                };

            LRES = DGLRES[ISPGRP];
            URES = DGURES[ISPGRP];
        }

        private static void DGRES_NWO(int ISPGRP, out float LRES, out float URES)
        {
            float[] DGLRES = {
                -0.440F, -0.60689712F, -0.51832840F, -0.3733441F,  // DF,GF,WH,RC
                -0.2690408F, -0.3155211F, -0.3663556F, -0.38681203F, // PY,MD,BL,WO
                -0.4947154F, -0.3679208F, -0.3679208F               // RA,PD,WI
            };
            float[] DGURES = {
                0.487F, 0.66701064F, 0.7452303F, 0.4250894F,      // DF,GF,WH,RC
                0.3367567F, 0.3843792F, 0.4157338F, 0.46098924F,     // PY,MD,BL,WO
                0.6171431F, 0.4444550F, 0.4444550F                  // RA,PD,WI
            };
            LRES = DGLRES[ISPGRP];
            URES = DGURES[ISPGRP];
        }

        private static void DGRES_SMC(int ISPGRP, out float LRES, out float URES)
        {
            float[] DGLRES = {
                -0.440F, -0.60689712F, -0.50104700F, -0.3733441F,  // DF,GF,WH,RC
                -0.2690408F, -0.3155211F, -0.3663556F, -0.38681203F, // PY,MD,BL,WO
                -0.4947154F, -0.3679208F, -0.3679208F                // RA,PD,WI
            };
            float[] DGURES = {
                0.487F, 0.66701064F, 0.59381592F, 0.4250894F,      // DF,GF,WH,RC
                0.3367567F, 0.3843792F, 0.4157338F, 0.46098924F,     // PY,MD,BL,WO
                0.6171431F, 0.4444550F, 0.4444550F                   // RA,PD,WI
            };
            LRES = DGLRES[ISPGRP];
            URES = DGURES[ISPGRP];
        }

        private static void DGRES_RAP(int ISPGRP, out float LRES, out float URES)
        {
            float[] DGLRES = {
                -0.20610321F, -0.1967740F, -0.22407503F, -0.1669646F,  // RA,DF,WH,RC
                -0.16383920F, -0.16453918F, -0.16453918F              // BL,PD,WI
            };
            float[] DGURES = {
                0.21278486F, 0.2177930F, 0.26556256F, 0.19010576F,     // RA,DF,WH,RC
                0.18592181F, 0.19876632F, 0.19876632F                 // BL,PD,WI
            };
            LRES = DGLRES[ISPGRP];
            URES = DGURES[ISPGRP];
        }

        /// <summary>
        /// Clones entry for tree twice with no change to the tree's expansion factor totalled across all three entries. Sets height growth.
        /// </summary>
        /// <param name="K">Index of tree to clone.</param>
        /// <param name="J">Index to assign clone of tree K to. Tree J + 1 is also set to a copy of tree M.</param>
        /// <param name="M">Flag indicating whether space is available in the tree data arrays for cloning the tree?</param>
        /// <param name="ON">Unused unless M = 2, in which case ON = 0 bypasses tripling.</param>
        /// <param name="VERSION">Organon variant.</param>
        /// <param name="IB">Threshold for determining if tree is of a big 6 or falls into the other species category.</param>
        /// <param name="BIG6">Total number of trees of big 6 species.</param>
        /// <param name="OTHER">Unused. (BUGBUG)</param>
        /// <param name="TDATAI">Tree data.</param>
        /// <param name="TDATAR">Tree data.</param>
        /// <param name="GROWTH">Tree data.</param>
        /// <param name="MGEXP">Tree data.</param>
        /// <param name="DEADEXP">Tree data.</param>
        public static void HGTRIP(int K, ref int J, int M, ref int ON, Variant VERSION, ref int BIG6, int[,] TDATAI,
                                  float[,] TDATAR, float[,] GROWTH, float[] MGEXP, float[] DEADEXP)
        {
            // BUGBUG doesn't have IB as an input so always treats tree as a big six species?
            if (M == 0 || (M == 2 && ON == 0))
            {
                // BUGBUG ON appears to be a bool declared as an int
                ON = 1;
            }
            else
            {
                if (J > MGEXP.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(J));
                }

                // DUPLICATE SPECIES, GROUP, CC, DBH, HT, CR, @HT & @DIAM
                // CUM @HT & @DIAM = ORIGINAL TREE
                float LRES;
                float URES;
                switch (VERSION)
                {
                    // BUGBUG no check ISPGRP indicates a species which is in range for HGRES
                    case Variant.Swo:
                        HGRES_SWO(TDATAI[K, 1], GROWTH[K, 0], out LRES, out URES);
                        break;
                    case Variant.Nwo:
                        HGRES_NWO(TDATAI[K, 1], GROWTH[K, 0], out LRES, out URES);
                        break;
                    case Variant.Smc:
                        HGRES_SMC(TDATAI[K, 1], GROWTH[K, 0], out LRES, out URES);
                        break;
                    case Variant.Rap:
                        HGRES_RAP(TDATAI[K, 1], GROWTH[K, 0], out LRES, out URES);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                for (int I = 0; I < 3; ++I)
                {
                    TDATAI[J, I] = TDATAI[K, I];
                    TDATAI[J + 1, I] = TDATAI[K, I];
                    TDATAR[J, I] = TDATAR[K, I];
                    TDATAR[J + 1, I] = TDATAR[K, I];
                }

                GROWTH[J, 0] = GROWTH[K, 0] + URES;
                GROWTH[J + 1, 0] = GROWTH[K, 0] + LRES;
                if (GROWTH[J + 1, 0] < 0.0F)
                {
                    GROWTH[J + 1, 0] = 0.0F;
                }
                GROWTH[K, 0] = GROWTH[K, 0] - (URES + LRES);
                if (GROWTH[K, 0] < 0.0F)
                {
                    GROWTH[K, 0] = 0.0F;
                }
                GROWTH[J, 2] = GROWTH[K, 2] + GROWTH[J, 0];
                GROWTH[J + 1, 2] = GROWTH[K, 2] + GROWTH[J + 1, 0];
                // GROWTH[K,3]=GROWTH[K,3]+GROWTH[K,0];
                GROWTH[J, 3] = GROWTH[K, 3];
                GROWTH[J + 1, 3] = GROWTH[K, 3];
                GROWTH[J, 1] = GROWTH[K, 1];
                GROWTH[J + 1, 1] = GROWTH[K, 1];

                // EXPANSION FACTOR IS 1/3
                float A = TDATAR[K, 3] / 3.0F;
                TDATAR[J, 3] = A;
                TDATAR[J + 1, 3] = A;
                TDATAR[K, 3] = A;
                A = TDATAR[K, 7] / 3.0F;
                TDATAR[J, 7] = A;
                TDATAR[J + 1, 7] = A;
                TDATAR[K, 7] = A;
                A = TDATAR[K, 4] / 3.0F;
                TDATAR[J, 4] = A;
                TDATAR[J + 1, 4] = A;
                TDATAR[K, 4] = A;
                A = MGEXP[K] / 3.0F;
                MGEXP[J] = A;
                MGEXP[J + 1] = A;
                MGEXP[K] = A;
                A = DEADEXP[K] / 3.0F;
                DEADEXP[J] = A;
                DEADEXP[J + 1] = A;
                DEADEXP[K] = A;
                TDATAR[J, 5] = TDATAR[K, 5];
                TDATAR[J + 1, 5] = TDATAR[K, 5];
                TDATAR[J, 6] = TDATAR[K, 6];
                TDATAR[J + 1, 6] = TDATAR[K, 6];

                // increase tree record count
                BIG6 += 2;
                J += 2;
                ON = 0;
            }
            GROWTH[K, 2] = GROWTH[K, 2] + GROWTH[K, 0];
        }

        private static void HGRES_NWO(int ISPGRP, float HG, out float LRES, out float URES)
        {
            // HEIGHT GROWTH LOWER AND UPPER RESIDUALS(3 parameters each - big 3 only)
            float[,] HGLRES = {
                { 0.0F, -0.4961F,  0.02173F }, // DF
                { 0.0F, -0.4156F,  0.00997F }, // GF
                { 0.0F, -0.35487F, 0.0F } // WH
            };
            float[,] HGURES = {
                { 0.4956F, 0.5758F,-0.03378F }, // DF
                { 0.0F, 0.9094F, -0.0554F }, // GF
                { 0.0F, 0.2682203F, 0.0F } // WH
            };

            float A0 = HGLRES[ISPGRP, 0];
            float A1 = HGLRES[ISPGRP, 1];
            float A2 = HGLRES[ISPGRP, 2];
            float C0 = HGURES[ISPGRP, 0];
            float C1 = HGURES[ISPGRP, 1];
            float C2 = HGURES[ISPGRP, 2];
            LRES = A0 + A1 * HG + A2 * HG * HG;
            URES = C0 + C1 * HG + C2 * HG * HG;
            if (LRES > 0.0F)
            {
                LRES = 0.0F;
            }
            if (URES < 0.0F)
            {
                URES = 0.0F;
            }
        }

        private static void HGRES_RAP(int ISPGRP, float HG, out float LRES, out float URES)
        {
            // HEIGHT GROWTH LOWER AND UPPER RESIDUALS(3 parameters each - big 3 only)
            float[,] HGLRES = {
                { -0.565597958F, -0.259956282F, 0.0276968137F }, // RA
                { 0.0F, -0.4961F, 0.02173F }, // DF
                { 0.0F, -0.4392F, 0.02751F } // WH
            };
            float[,] HGURES = {
                { 0.866609308F, 0.100265352F, -0.0135706039F }, // RA
                { 0.4956F, 0.5758F, -0.03378F }, // DF
                { 0.4388F, 0.5098F, -0.02991F } // WH
            };

            float A0 = HGLRES[ISPGRP, 0];
            float A1 = HGLRES[ISPGRP, 1];
            float A2 = HGLRES[ISPGRP, 2];
            float C0 = HGURES[ISPGRP, 0];
            float C1 = HGURES[ISPGRP, 1];
            float C2 = HGURES[ISPGRP, 2];
            LRES = A0 + A1 * HG + A2 * HG * HG;
            URES = C0 + C1 * HG + C2 * HG * HG;
            if (LRES > 0.0F)
            {
                LRES = 0.0F;
            }
            if (URES < 0.0F)
            {
                URES = 0.0F;
            }
        }

        private static void HGRES_SMC(int ISPGRP, float HG, out float LRES, out float URES)
        {
            // HEIGHT GROWTH LOWER AND UPPER RESIDUALS(3 parameters each - big 3 only)
            float[,] HGLRES = {
                { 0.0F, -0.4961F, 0.02173F }, // DF
                { 0.0F, -0.4156F, 0.00997F }, // GF
                { 0.0F, -0.4392F, 0.02751F }  // WH
            };
            float[,] HGURES = {
                { 0.4956F, 0.5758F, -0.03378F }, // DF
                { 0.0F, 0.9094F, -0.0554F }, // GF
                { 0.4388F, 0.5098F, -0.02991F }  // WH
            };

            float A0 = HGLRES[ISPGRP, 0];
            float A1 = HGLRES[ISPGRP, 1];
            float A2 = HGLRES[ISPGRP, 2];
            float C0 = HGURES[ISPGRP, 0];
            float C1 = HGURES[ISPGRP, 1];
            float C2 = HGURES[ISPGRP, 2];
            LRES = A0 + A1 * HG + A2 * HG * HG;
            URES = C0 + C1 * HG + C2 * HG * HG;
            if (LRES > 0.0F)
            {
                LRES = 0.0F;
            }
            if (URES < 0.0F)
            {
                URES = 0.0F;
            }
        }

        private static void HGRES_SWO(int ISPGRP, float HG, out float LRES, out float URES)
        {
            // HEIGHT GROWTH LOWER AND UPPER RESIDUALS(3 parameters each - big 5 only)
            float[,] HGLRES = {
                { -0.51303F, -0.35755F, 0.01357F }, // DF
                { -0.46026F, -0.31335F, 0.00000F }, // GW
                {  0.00000F, -0.53151F, 0.02492F }, // PP
                {  0.00000F, -0.59648F, 0.03244F }, // SP
                {  0.00000F, -0.80800F, 0.07429F }  // IC
            };
            float[,] HGURES = {
                { 1.09084F, 0.31540F, -0.02196F }, // DF
                { 1.32726F, 0.17331F,  0.00000F }, // GW
                { 1.20195F, 0.11159F,  0.00000F }, // PP
                { 0.00000F, 0.84916F, -0.07683F }, // SP
                { 1.29032F, 0.12008F,  0.00000F } // IC
            };

            float A0 = HGLRES[ISPGRP, 0];
            float A1 = HGLRES[ISPGRP, 1];
            float A2 = HGLRES[ISPGRP, 2];
            float C0 = HGURES[ISPGRP, 0];
            float C1 = HGURES[ISPGRP, 1];
            float C2 = HGURES[ISPGRP, 2];
            LRES = A0 + A1 * HG + A2 * HG * HG;
            URES = C0 + C1 * HG + C2 * HG * HG;
            if (LRES > 0.0F)
            {
                LRES = 0.0F;
            }
            if (URES < 0.0F)
            {
                URES = 0.0F;
            }
        }
    }
}
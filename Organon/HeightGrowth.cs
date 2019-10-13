using System;
using System.Diagnostics;

namespace Osu.Cof.Organon
{
    internal class HeightGrowth
    {
        /// <summary>
        /// Estimate growth effective age for Douglas-fir and grand fir using Bruce's (1981) dominant height model.
        /// </summary>
        /// <param name="SI"></param>
        /// <param name="HT"></param>
        /// <param name="GP"></param>
        /// <param name="GEAGE"></param>
        /// <param name="PHTGRO"></param>
        /// <remarks>
        /// Bruce. 1981. Forest Science 27:711-725.
        /// </remarks>
        public static void B_HG(float SI, float HT, float GP, out float GEAGE, out float PHTGRO)
        {
            float X1 = 13.25F - SI / 20.0F;
            float X2 = 63.25F - SI / 20.0F;
            float B2 = -0.447762F - 0.894427F * SI / 100.0F + 0.793548F * (float)Math.Pow(SI / 100.0, 2.0) - 0.171666F * (float)Math.Pow(SI / 100.0, 3.0);
            float B1 = (float)(Math.Log(4.5 / SI) / (Math.Pow(X1, B2) - Math.Pow(X2, B2)));
            float XX1 = (float)(Math.Log(HT / SI) / B1 + Math.Pow(X2, B2));
            if (XX1 > 0.0F)
            {
                GEAGE = (float)Math.Pow(XX1, 1.0 / B2) - X1;
            }
            else
            {
                GEAGE = 500.0F;
            }
            float PHT = SI * (float)Math.Exp(B1 * (Math.Pow(GEAGE + GP + X1, B2) - Math.Pow(X2, B2)));
            PHTGRO = PHT - HT;
        }

        /// <summary>
        /// Calculate western hemlock growth effective age and potential height growth using Flewelling's model for dominant individuals.
        /// </summary>
        /// <param name="SI">Site index (feet) from ground.</param>
        /// <param name="HT">Height of tree.</param>
        /// <param name="GP"></param>
        /// <param name="GEAGE">Growth effective age of tree.</param>
        /// <param name="PHTGRO">Potential height growth increment in feet.</param>
        public static void F_HG(float SI, float HT, float GP, out float GEAGE, out float PHTGRO)
        {
            // For Western Hemlock compute Growth Effective Age and 5-year potential
            // or 1-year height growth using the western hemlock top height curves of
            // Flewelling.These subroutines are required:
            // SITECV_F   computes top height from site and age
            // SITEF_C computes model parameters
            // SITEF_SI   calculates an approximate psi for a given site
            // Note: Flewelling's curves are metric.
            // Site Index is not adjusted for stump height.
            float SIM = SI * 0.3048F;
            float HTM = HT * 0.3048F;

            // Compute growth effective age
            float HTOP;
            float AGE = 1.0F;
            for (int I = 0; I < 4; ++I)
            {
                do
                {
                    AGE += 100.0F / (float)Math.Pow(10.0, I);
                    if (AGE > 500.0F)
                    {
                        GEAGE = 500.0F;
                        WesternHemlockHeight.SITECV_F(SIM, GEAGE, out float XHTOP1);
                        WesternHemlockHeight.SITECV_F(SIM, GEAGE + GP, out float XHTOP2);
                        PHTGRO = 3.2808F * (XHTOP2 - XHTOP1);
                        return;
                    }
                    WesternHemlockHeight.SITECV_F(SIM, AGE, out HTOP);
                }
                while (HTOP < HTM);
                AGE -= 100.0F / (float)Math.Pow(10.0, I);
            }
            GEAGE = AGE;

            // Compute top height and potential height growth
            WesternHemlockHeight.SITECV_F(SIM, GEAGE + GP, out HTOP);
            float PHT = HTOP * 3.2808F;
            PHTGRO = PHT - HT;
        }

        /// <summary>
        /// Predict height from DBH for southwest Oregon species.
        /// </summary>
        /// <param name="ISPGRP">Species group.</param>
        /// <param name="DBH">Diameter at breast height (inches).</param>
        /// <param name="PRDHT">Predicted height (feet).</param>
        public static void HD_SWO(int ISPGRP, float DBH, out float PRDHT)
        {
            // NEW HEIGHT/DIAMETER PARAMETERS FOR UNDAMAGED TREES.EXCEPT RC, WO, AND RA(3 parameters - all species)
            //
            // DF Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // GW Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // PP Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // SP Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // IC Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // WH Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // RC Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
            // PY Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // MD Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // GC Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // TA Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // CL Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // BL Coefficients Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // WO Coefficients from Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
            // BO Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // RA Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #1
            // PD Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            // WI Coefficients from Hanus, Hann and Marshall(1999) FRL Research Contribution 27
            float[,] HDPAR = {
                {
                    7.133682298F, 6.75286569F, 6.27233557F, 5.81876360F, //DF,GW,PP,SP
                    10.04621768F, 6.58804F, 6.14817441F, 5.10707208F,    //IC,WH,RC,PY
                    6.53558288F, 9.2251518F, 8.49655416F, 9.01612971F,   //MD,GC,TA,CL
                    5.20018445F, 4.69753118F, 5.04832439F, 5.59759126F,  //BL,WO,BO,RA
                    7.49095931F, 3.26840527F,                            //PD,WI
                },
                {
                    -5.433744897F, -5.52614439F, -5.57306985F, -5.31082668F, //DF,GW,PP,SP
                    -8.72915115F, -5.25312496F, -5.40092761F, -3.28638769F,  //IC,WH,RC,PY
                    -4.69059053F, -7.65310387F, -6.68904033F, -7.34813829F,  //MD,GC,TA,CL
                    -2.86671078F, -3.51586969F, -3.32715915F, -3.19942952F,  //BL,WO,BO,RA
                    -5.40872209F, -0.95270859F,                              //PD,WI
                },
                {
                    -0.266398088F, -0.33012156F, -0.40384171F, -0.47349388F, //DF,GW,PP,SP
                    -0.14040106F, -0.31895401F, -0.38922036F, -0.24016101F,  //IC,WH,RC,PY
                    -0.24934807F, -0.15480725F, -0.16105112F, -0.134025626F, //MD,GC,TA,CL
                    -0.42255220F, -0.57665068F, -0.43456034F, -0.38783403F,  //BL,WO,BO,RA
                    -0.16874962F, -0.98015696F } //PD,WI
            };

            float B0 = HDPAR[0, ISPGRP];
            float B1 = HDPAR[1, ISPGRP];
            float B2 = HDPAR[2, ISPGRP];
            PRDHT = 4.5F + (float)Math.Exp(B0 + B1 * Math.Pow(DBH, B2));
        }

        public static void HD_NWO(int ISPGRP, float DBH, out float PRDHT)
        {
            // HEIGHT/DIAMETER PARAMETERS(3 parameters - all species)
            //
            // DF Coefficients from Wang and Hann(1988) FRL Research Paper 51
            // GF Coefficients from Wang and Hann(1988) FRL Research Paper 51
            // WH Coefficients from Johnson(2000) Willamette Industries Report
            // RC Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
            // PY Coefficients from Wang and Hann(1988) FRL Research Paper 51
            // MD Coefficients from Wang and Hann(1988) FRL Research Paper 51
            // BL Coefficients from Wang and Hann(1988) FRL Research Paper 51
            // WO Coefficients from Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
            // RA Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #1
            // PD Coefficients from Wang and Hann(1988) FRL Research Paper 51
            // WI Coefficients from Wang and Hann(1988) FRL Research Paper 51
            float[,] HDPAR = {
                {
                    7.04524F, 7.42808F, 5.93792F, 6.14817441F,//DF,GF,WH,RC
                    9.30172F, 5.84487F, 5.21462F, 4.69753118F,//PY,MD,BL,WO
                    5.59759126F, 4.49727F, 4.88361F,             //RA,PD,WI
                },
                {
                    -5.16836F, -5.80832F, -4.43822F, -5.40092761F,//DF,GF,WH,RC
                    -7.50951F, -3.84795F, -2.70252F, -3.51586969F,//PY,MD,BL,WO
                    -3.19942952F, -2.07667F, -2.47605F,             //RA,PD,WI
                },
                {
                    -0.253869F, -0.240317F, -0.411373F, -0.38922036F,//DF,GF,WH,RC
                    -0.100000F, -0.289213F, -0.354756F, -0.57665068F,//PY,MD,BL,WO
                    -0.38783403F, -0.388650F, -0.309050F } //RA,PD,WI
            };

            float B0 = HDPAR[0, ISPGRP];
            float B1 = HDPAR[1, ISPGRP];
            float B2 = HDPAR[2, ISPGRP];
            PRDHT = 4.5F + (float)Math.Exp(B0 + B1 * Math.Pow(DBH, B2));
        }

        public static void HD_RAP(int ISPGRP, float DBH, out float PRDHT)
        {
            // HEIGHT/DIAMETER PARAMETERS(3 parameters - all species)
            //
            // RA Coefficients from Hann, Bluhm, and Hibbs(2011) Forest Biometrics Research Paper 1
            // DF Coefficients from Hanus, Marshall, and Hann(1999) FRL Research Contribution 25
            // WH Coefficients from Hanus, Marshall, and Hann(1999) FRL Research Contribution 25
            // RC Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
            // BL Coefficients from Wang and Hann(1988) FRL Research Paper 51
            // PD Coefficients from Wang and Hann(1988) FRL Research Paper 51
            // WI Coefficients from Wang and Hann(1988) FRL Research Paper 51
            //
            float[,] HDPAR = {
                {
                    6.75650139F, 7.262195456F, 6.555344622F, 6.14817441F,//RA,DF,WH,RC
                    5.21462F, 4.49727F, 4.88361F,             //BL,PD,WI
                },
                {
                    -4.6252377F, -5.899759104F, -5.137174162F, -5.40092761F,//RA,DF,WH,RC
                    -2.70252F, -2.07667F, -2.47605F,             //BL,PD,WI
                },
                {
                    -0.23208200F, -0.287207389F, -0.364550800F, -0.38922036F,//RA,DF,WH,RC
                    -0.354756F, -0.388650F, -0.309050F //BL,PD,WI
                }
            };

            float B0 = HDPAR[0, ISPGRP];
            float B1 = HDPAR[1, ISPGRP];
            float B2 = HDPAR[2, ISPGRP];
            PRDHT = 4.5F + (float)Math.Exp(B0 + B1 * Math.Pow(DBH, B2));
        }

        public static void HD_SMC(int ISPGRP, float DBH, out float PRDHT)
        {
            // HEIGHT/DIAMETER PARAMETERS(3 parameters - all species)
            //
            // DF Coefficients from Hanus, Marshall, and Hann(1999) FRL Research Contribution 25
            // GF Coefficients from Wang and Hann(1988) FRL Research Paper 51
            // WH Coefficients from Hanus, Marshall, and Hann(1999) FRL Research Contribution 25
            // RC Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #2
            // PY Coefficients from Wang and Hann(1988) FRL Research Paper 51
            // MD Coefficients from Wang and Hann(1988) FRL Research Paper 51
            // BL Coefficients from Wang and Hann(1988) FRL Research Paper 51
            // WO Coefficients from Gould, Marshall, and Harrington(2008) West.J.Appl.For. 23: 26-33
            // RA Coefficients from Hann and Hanus(2002) OSU Department of Forest Management Internal Report #1
            // PD Coefficients from Wang and Hann(1988) FRL Research Paper 51
            // WI Coefficients from Wang and Hann(1988) FRL Research Paper 51
            //
            float[,] HDPAR = {
                {
                    7.262195456F, 7.42808F, 6.555344622F, 6.14817441F,//DF,GF,WH,RC
                    9.30172F, 5.84487F, 5.21462F, 4.69753118F,//PY,MD,BL,WO
                    5.59759126F, 4.49727F, 4.88361F,             //RA,PD,WI
                },
                {
                    -5.899759104F, -5.80832F, -5.137174162F, -5.40092761F,//DF,GF,WH,RC
                    -7.50951F, -3.84795F, -2.70252F, -3.51586969F,//PY,MD,BL,WO
                    -3.19942952F, -2.07667F, -2.47605F,             //RA,PD,WI
                },
                {
                    -0.287207389F, -0.240317F, -0.364550800F, -0.38922036F,//DF,GF,WH,RC
                    -0.100000F, -0.289213F, -0.354756F, -0.57665068F,//PY,MD,BL,WO
                    -0.38783403F, -0.388650F, -0.309050F  //RA,PD,WI
                }
            };

            float B0 = HDPAR[0, ISPGRP];
            float B1 = HDPAR[1, ISPGRP];
            float B2 = HDPAR[2, ISPGRP];
            PRDHT = 4.5F + (float)Math.Exp(B0 + B1 * Math.Pow(DBH, B2));
        }

        private static void HG_FERT(int CYCLG, Variant VERSION, int ISPGRP, float SI_1, float[] PN, float[] YF, out float FERTADJ)
        {
            float PF1;
            float PF2;
            float PF3;
            float PF4;
            float PF5;
            if (VERSION <= Variant.Smc)
            {
                if (ISPGRP == 0)
                {
                    PF1 = 1.0F;
                    PF2 = 0.333333333F;
                    PF3 = -1.107409443F;
                    PF4 = -2.133334346F;
                    PF5 = 1.5F;
                }
                else
                {
                    PF1 = 0.0F;
                    PF2 = 1.0F;
                    PF3 = 0.0F;
                    PF4 = 0.0F;
                    PF5 = 1.0F;
                }
            }
            else
            {
                PF1 = 0.0F;
                PF2 = 1.0F;
                PF3 = 0.0F;
                PF4 = 0.0F;
                PF5 = 1.0F;
            }

            float FALDWN = 1.0F;
            float XTIME = (float)CYCLG * 5.0F;
            float FERTX1 = 0.0F;
            for (int I = 1; I < 5; ++I)
            {
                FERTX1 += (PN[I] / 800.0F) * (float)Math.Exp((PF3 / PF2) * (YF[0] - YF[I]));
            }
            float FERTX2 = (float)Math.Exp(PF3 * (XTIME - YF[0]) + PF4 * Math.Pow(SI_1 / 100.0, PF5));
            FERTADJ = 1.0F + PF1 * (float)(Math.Pow(PN[0] / 800.0 + FERTX1, PF2) * FERTX2) * FALDWN;
            Debug.Assert(FERTADJ >= 1.0F);
        }

        private static void HG_NWO(int ISPGRP, float PHTGRO, float CR, float TCCH, out float HG)
        {
            // HEIGHT GROWTH PARAMETERS(8 parameters - big 3 conifers only)
            //
            // DF Coefficients from Hann, Marshall, and Hanus(2006) FRL Research Contribution ??
            // GF Coefficients from Ritchie and Hann(1990) FRL Research Paper 54
            // WH Coefficients from Johnson(2002) Willamette Industries Report
            float[,] HGPAR = {
                { 0.655258886F, -0.006322913F, -0.039409636F, 0.5F, 0.597617316F, 2.0F, 0.631643636F, 1.010018427F }, // DF
                { 1.0F, -0.0328142F, -0.0127851F, 1.0F, 6.19784F, 2.0F, 0.0F, 1.01F }, //GF
                { 1.0F, -0.0384415F, -0.0144139F, 0.5F, 1.04409F, 2.0F, 0.0F, 1.03F } //WH
            };

            float P1 = HGPAR[ISPGRP, 0];
            float P2 = HGPAR[ISPGRP, 1];
            float P3 = HGPAR[ISPGRP, 2];
            float P4 = HGPAR[ISPGRP, 3];
            float P5 = HGPAR[ISPGRP, 4];
            float P6 = HGPAR[ISPGRP, 5];
            float P7 = HGPAR[ISPGRP, 6];
            float P8 = HGPAR[ISPGRP, 7];
            float FCR = (float)(-P5 * Math.Pow(1.0F - CR, P6) * Math.Exp(P7 * Math.Pow(TCCH, 0.5)));
            float B0 = P1 * (float)Math.Exp(P2 * TCCH);
            float B1 = (float)Math.Exp(P3 * Math.Pow(TCCH, P4));
            float MODIFER = P8 * (B0 + (B1 - B0) * (float)Math.Exp(FCR));
            float CRADJ = 1.0F - (float)Math.Exp(-(25.0 * 25.0 * CR * CR));
            HG = PHTGRO * MODIFER * CRADJ;
            Debug.Assert(HG >= 0.0F);
        }

        private static void HG_RAP(int ISPGRP, float PHTGRO, float CR, float TCCH, out float HG)
        {
            // HEIGHT GROWTH PARAMETERS(8 parameters - 3 species only)
            //
            // RA Coefficients from Hann, Bluhm, and Hibbs Red Alder Plantation Analysis
            // DF Coefficients from Hann, Marshall, and Hanus(2006) FRL Research Contribution ??
            // WH Coefficients from Johnson(2002) Willamette Industries Report
            //
            // WEIGHTED SUMMATION PROCEDURE PARAMETERS FOR RED ALDER
            float[,] HGPAR = {
                  { 0.809837005F, -0.0134163653F, -0.0609398629F, 0.5F, 1.0F, 2.0F, 0.1469442410F, 1.0476380753F }, // RA
                  { 0.655258886F, -0.006322913F, -0.039409636F, 0.5F, 0.597617316F, 2.0F, 0.631643636F, 1.010018427F }, // DF
                  { 1.0F, -0.0384415F, -0.0144139F, 0.5F, 1.04409F, 2.0F, 0.0F, 1.03F } // WH
            };
            // WEIGHTED CENTRAL PAI PROCEDURE PARAMETERS FOR RED ALDER
            // float[,] HGPAR = {
            // 0.775118127 ,  0.655258886 ,  1.0         ,   // RA,DF,WH
            // -0.0128743358, -0.006322913 , -0.0384415   ,   // RA,DF,WH
            // -0.070294082 , -0.039409636 , -0.0144139   ,   // RA,DF,WH
            // 0.5         ,  0.5         ,  0.5         ,   // RA,DF,WH
            // 1.0         ,  0.597617316 ,  1.04409     ,   // RA,DF,WH
            // 2.0         ,  2.0         ,  2.0         ,   // RA,DF,WH
            // 0.120539836 ,  0.631643636 ,  0.0         ,   // RA,DF,WH
            // 1.07563185  ,  1.010018427 ,  1.03        /   // RA,DF,WH
            // };
            float P1 = HGPAR[ISPGRP, 0];
            float P2 = HGPAR[ISPGRP, 1];
            float P3 = HGPAR[ISPGRP, 2];
            float P4 = HGPAR[ISPGRP, 3];
            float P5 = HGPAR[ISPGRP, 4];
            float P6 = HGPAR[ISPGRP, 5];
            float P7 = HGPAR[ISPGRP, 6];
            float P8 = HGPAR[ISPGRP, 7];
            float FCR = (float)(-P5 * Math.Pow(1.0 - CR, P6) * Math.Exp(P7 * Math.Pow(TCCH, 0.5)));
            float B0 = P1 * (float)Math.Exp(P2 * TCCH);
            float B1 = (float)Math.Exp(P3 * Math.Pow(TCCH, P4));
            float MODIFER = P8 * (B0 + (B1 - B0) * (float)Math.Exp(FCR));
            float CRADJ = 1.0F - (float)Math.Exp(-(25.0 * 25.0 * CR * CR));
            HG = PHTGRO * MODIFER * CRADJ;
            Debug.Assert(HG >= 0.0F);
        }

        private static void HG_SMC(int ISPGRP, float PHTGRO, float CR, float TCCH, out float HG)
        {
            // HEIGHT GROWTH PARAMETERS(8 parameters - big 3 conifers only)
            //
            // DF Coefficients from Hann, Marshall, and Hanus(2006) FRL Research Contribution ??
            // GF Coefficients from Ritchie and Hann(1990) FRL Research Paper 54
            // WH Coefficients from Hann, Marshall, and Hanus(2003) FRL Research Contribution 40
            //
            // DATA HGPAR/
            // 1           0.655258886,  1.0      ,  1.0         ,               // DF,GF,WH
            // 2          -0.006322913, -0.0328142, -0.0056949357,               // DF,GF,WH
            // 3          -0.039409636, -0.0127851, -0.0018047267,               // DF,GF,WH
            // 4           0.5        ,  1.0      ,  0.5         ,               // DF,GF,WH
            // 5           0.597617316,  6.19784  ,  6.1978      ,               // DF,GF,WH
            // 6           2.0        ,  2.0      ,  2.0         ,               // DF,GF,WH
            // 7           0.631643636,  0.0      ,  0.0         ,               // DF,GF,WH
            // 8           1.010018427,  1.01     ,  1.03        /               // DF,GF,WH
            //
            // HEIGHT GROWTH PARAMETERS(8 parameters - big 3 conifers only)
            //
            // DF Coefficients from Hann, Marshall, and Hanus(2006) FRL Research Contribution ??
            // GF Coefficients from Ritchie and Hann(1990) FRL Research Paper 54
            // WH Coefficients from Johnson(2002) Willamette Industries Report
            //
            float[,] HGPAR = {
                { 0.655258886F, -0.006322913F, -0.039409636F, 0.5F, 0.597617316F, 2.0F, 0.631643636F, 1.010018427F }, // DF
                { 1.0F, -0.0328142F, -0.0127851F, 1.0F, 6.19784F, 2.0F, 0.0F, 1.01F }, // GF
                { 1.0F, -0.0384415F, -0.0144139F, 0.5F, 1.04409F, 2.0F, 0.0F, 1.03F } // WH
            };

            float P1 = HGPAR[ISPGRP, 0];
            float P2 = HGPAR[ISPGRP, 1];
            float P3 = HGPAR[ISPGRP, 2];
            float P4 = HGPAR[ISPGRP, 3];
            float P5 = HGPAR[ISPGRP, 4];
            float P6 = HGPAR[ISPGRP, 5];
            float P7 = HGPAR[ISPGRP, 6];
            float P8 = HGPAR[ISPGRP, 7];
            float FCR = (float)(-P5 * Math.Pow(1.0 - CR, P6) * Math.Exp(P7 * Math.Pow(TCCH, 0.5)));
            float B0 = P1 * (float)Math.Exp(P2 * TCCH);
            float B1 = (float)Math.Exp(P3 * Math.Pow(TCCH, P4));
            float MODIFER = P8 * (B0 + (B1 - B0) * (float)Math.Exp(FCR));
            float CRADJ = (float)(1.0 - Math.Exp(-(25.0 * 25.0 * CR * CR)));
            HG = PHTGRO * MODIFER * CRADJ;
            Debug.Assert(HG >= 0.0F);
        }

        private static void HG_SWO(int ISPGRP, float PHTGRO, float CR, float TCCH, out float HG)
        {
            // HEIGHT GROWTH PARAMETERS(8 parameters - big 5 conifers only)
            //
            // DF Coefficients from Hann and Hanus(2002) FRL Research Contribution 41
            // GW Coefficients from Hann and Hanus(2002) FRL Research Contribution 41
            // PP Coefficients from Hann and Hanus(2002) FRL Research Contribution 41
            // SP Coefficients from Hann and Hanus(2002) FRL Research Contribution 41
            // IC Coefficients from Hann and Hanus(2002) FRL Research Contribution 41
            //
            float[,] HGPAR = {
                { 1.0F, -0.02457621F, -0.00407303F, 1.0F, 2.89556338F, 2.0F, 0.0F, 1.0F }, // DF
                { 1.0F, -0.14889850F, -0.00407303F, 1.0F, 7.69023575F, 2.0F, 0.0F, 1.0F }, // GW
                { 1.0F, -0.14889850F, -0.00322752F, 1.0F, 0.92071847F, 2.0F, 0.0F, 1.0F }, // PP
                { 1.0F, -0.14889850F, -0.00678955F, 1.0F, 0.92071847F, 2.0F, 0.0F, 1.0F }, // SP
                { 1.0F, -0.01453250F, -0.00637434F, 1.0F, 1.27228638F, 2.0F, 0.0F, 1.0F } // IC
            };
            float P1 = HGPAR[ISPGRP, 0];
            float P2 = HGPAR[ISPGRP, 1];
            float P3 = HGPAR[ISPGRP, 2];
            float P4 = HGPAR[ISPGRP, 3];
            float P5 = HGPAR[ISPGRP, 4];
            float P6 = HGPAR[ISPGRP, 5];
            float P7 = HGPAR[ISPGRP, 6];
            float P8 = HGPAR[ISPGRP, 7];
            float FCR = (float)(-P5 * Math.Pow(1.0 - CR, P6) * Math.Exp(P7 * Math.Pow(TCCH, 0.5)));
            float B0 = P1 * (float)Math.Exp(P2 * TCCH);
            float B1 = (float)Math.Exp(P3 * Math.Pow(TCCH, P4));
            float MODIFER = P8 * (B0 + (B1 - B0) * (float)Math.Exp(FCR));
            float CRADJ = 1.0F - (float)Math.Exp(-(25.0 * 25.0 * CR * CR));
            HG = PHTGRO * MODIFER * CRADJ;
            Debug.Assert(HG >= 0.0F);
        }

        private static void HG_THIN(int CYCLG, Variant VERSION, int ISPGRP, float BABT, float[] BART, float[] YT, out float THINADJ)
        {
            float PT1;
            float PT2;
            float PT3;
            if (VERSION <= Variant.Smc)
            {
                if (ISPGRP == 0)
                {
                    PT1 = -0.3197415492F;
                    PT2 = 0.7528887377F;
                    PT3 = -0.2268800162F;
                }
                else
                {
                    PT1 = 0.0F;
                    PT2 = 1.0F;
                    PT3 = 0.0F;
                }
            }
            else
            {
                if (ISPGRP == 0)
                {
                    PT1 = -0.613313694F;
                    PT2 = 1.0F;
                    PT3 = -0.443824038F;
                }
                else
                {
                    if (ISPGRP == 1)
                    {
                        PT1 = -0.3197415492F;
                        PT2 = 0.7528887377F;
                        PT3 = -0.2268800162F;
                    }
                    else
                    {
                        PT1 = 0.0F;
                        PT2 = 1.0F;
                        PT3 = 0.0F;
                    }
                }
            }
            float XTIME = 5.0F * (float)CYCLG;
            float THINX1 = 0.0F;
            for (int I = 1; I < 5; ++I)
            {
                THINX1 += BART[I] * (float)Math.Exp((PT3 / PT2) * (YT[0] - YT[0]));
            }
            float THINX2 = THINX1 + BART[0];
            float THINX3 = THINX1 + BABT;
            float PREM;
            if (THINX3 <= 0.0F)
            {
                PREM = 0.0F;
            }
            else
            {
                PREM = THINX2 / THINX3;
            }
            if (PREM > 0.75F)
            {
                PREM = 0.75F;
            }
            THINADJ = 1.0F + PT1 * (float)(Math.Pow(PREM, PT2) * Math.Exp(PT3 * (XTIME - YT[0])));
            Debug.Assert(THINADJ >= 0.0F);
        }

        /// <summary>
        /// Calculate Douglas-fir and ponderosa growth effective age and potential height growth for southwest Oregon.
        /// </summary>
        /// <param name="ISP">Douglas-fir coefficients are used if ISP == 1, ponderosa otherwise.</param>
        /// <param name="SI">Site index (feet) from breast height.</param>
        /// <param name="HT">Height of tree.</param>
        /// <param name="GEAGE">Growth effective age of tree.</param>
        /// <param name="PHTGRO">Potential height growth increment in feet.</param>
        /// <remarks>
        /// Derived from the code in appendix 2 of Hann and Scrivani 1987 (FRL Research Bulletin 59). Growth effective age is introduced in 
        /// Hann and Ritchie 1988 (Height Growth Rate of Douglas-Fir: A Comparison of Model Forms. Forest Science 34(1): 165–175.).
        /// </remarks>
        public static void HS_HG(int ISP, float SI, float HT, out float GEAGE, out float PHTGRO)
        {
            // BUGBUG these are a0, a1, and a2 in the paper
            float B0;
            float B1;
            float B2;
            if (ISP == 1)
            {
                // PSME
                B0 = -6.21693F;
                B1 = 0.281176F;
                B2 = 1.14354F;
            }
            else
            {
                // PIPO
                B0 = -6.54707F;
                B1 = 0.288169F;
                B2 = 1.21297F;
            }

            float BBC = B0 + B1 * (float)Math.Log(SI);
            float X50 = 1.0F - (float)Math.Exp(-1.0 * Math.Exp(BBC + B2 * 3.912023F));
            float A1A = 1.0F - (HT - 4.5F) * (X50 / SI);
            if (A1A <= 0.0F)
            {
                GEAGE = 500.0F;
                PHTGRO = 0.0F;
            }
            else
            {
                GEAGE = (float)Math.Pow((-1.0F * Math.Log(A1A)) / (Math.Exp(B0) * Math.Pow(SI, B1)), (1.0F / B2));
                float XAI = 1.0F - (float)Math.Exp(-1.0 * Math.Exp(BBC + B2 * Math.Log(GEAGE)));
                float XAI5 = 1.0F - (float)Math.Exp(-1.0 * Math.Exp(BBC + B2 * Math.Log(GEAGE + 5.0)));
                PHTGRO = (4.5F + (HT - 4.5F) * (XAI5 / XAI)) - HT;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeIndex">Index of tree to grow in tree data.</param>
        /// <param name="triplingMethod">Method of tripling. Unused.</param>
        /// <param name="ON">Always zero. Set to 1 to enable tripling.</param>
        /// <param name="variant">Organon variant.</param>
        /// <param name="CYCLG">Simulation cycle.</param>
        /// <param name="stand">Stand data.</param>
        /// <param name="TDATAR">Tree data.</param>
        /// <param name="SI_1">Primary site index from breast height.</param>
        /// <param name="SI_2">Secondary site index from breast height.</param>
        /// <param name="CCH">Canopy height? (DOUG?)</param>
        /// <param name="PN"></param>
        /// <param name="YF"></param>
        /// <param name="BABT">Thinning?</param>
        /// <param name="BART">Thinning?</param>
        /// <param name="YT">Thinning?</param>
        /// <param name="OLD">Obsolete?</param>
        /// <param name="PDEN">(DOUG?)</param>
        /// <param name="GROWTH">Tree data.</param>
        /// <remarks>
        /// M = METHOD OF TRIPLING
        /// 0 = NO TRIPLING
        /// 1 = TRIPLE EVERY TREE
        /// 2 = TRIPLE EVERY OTHER TREE
        /// 3 = RANDOM ERROR
        /// 
        /// ON = EVERY OTHER TREE TRIPLING DESIGNATOR
        /// 1 = TRIPLE THIS TREE
        /// 0 = DON'T TRIPLE THIS TREE
        /// </remarks>
        public static void HTGRO1(int treeIndex, int triplingMethod, int ON, Variant variant, int CYCLG, Stand stand, float[,] TDATAR, float SI_1, float SI_2,
                                  float[] CCH, float[] PN, float[] YF, float BABT, float[] BART, float[] YT, ref float OLD, float PDEN, float[,] GROWTH)
        {
            // BUGBUG remove M and ON
            // CALCULATE 5-YEAR HEIGHT GROWTH
            float CR = TDATAR[treeIndex, 2];

            // FOR MAJOR SPECIES
            int speciesGroup = stand.Integer[treeIndex, Constant.TreeIndex.Integer.SpeciesGroup];
            if (stand.IsBigSixSpecies(treeIndex))
            {
                float XI = 40.0F * (TDATAR[treeIndex, 1] / CCH[40]);
                int I = (int)XI + 2;
                float XXI = (float)I - 1.0F;
                float TCCH;
                if (TDATAR[treeIndex, 1] >= CCH[40])
                {
                    TCCH = 0.0F;
                }
                else if (I == 41)
                {
                    TCCH = CCH[39] * (40.0F - XI);
                }
                else
                {
                    TCCH = CCH[I] + (CCH[I - 1] - CCH[I]) * (XXI - XI);
                }

                // COMPUTE HEIGHT GROWTH OF UNTREATED TREES
                // GEAGE growth effective age in years
                // IDXAGE index age? (DOUG?)
                // HG height growth in feet
                FiaCode species = (FiaCode)stand.Integer[treeIndex, Constant.TreeIndex.Integer.Species];
                float GEAGE;
                float IDXAGE = 0.0F; // BUGBUG: IDXAGE not initialized on all Fortran code paths
                float HG = 0.0F; // BUGBUG: HG not initialized on all Fortran code paths
                switch (variant)
                {
                    case Variant.Swo:
                        float SITE;
                        int ISISP;
                        // POTENTIAL HEIGHT GROWTH FROM HANN AND SCRIVANI'S (1987) DOMINANT HEIGHT GROWTH EQUATION
                        if (species == FiaCode.PinusPonderosa)
                        {
                            SITE = SI_2;
                            ISISP = 2;
                        }
                        else
                        {
                            SITE = SI_1;
                            if (species == FiaCode.CalocedrusDecurrens)
                            {
                                SITE = (SI_1 + 4.5F) * 0.66F - 4.5F;
                            }
                            ISISP = 1;
                        }
                        HS_HG(ISISP, SITE, TDATAR[treeIndex, 1], out GEAGE, out float PHTGRO);
                        IDXAGE = 500.0F;
                        HG_SWO(speciesGroup, PHTGRO, CR, TCCH, out HG);
                        break;
                    case Variant.Nwo:
                        float GP = 5.0F;
                        if (speciesGroup == 3)
                        {
                            // POTENTIAL HEIGHT GROWTH FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH
                            SITE = SI_2 + 4.5F;
                            F_HG(SITE, TDATAR[treeIndex, 1], GP, out GEAGE, out PHTGRO);
                        }
                        else
                        {
                            // POTENTIAL HEIGHT GROWTH FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH FOR DOUGLAS-FIR AND GRAND FIR
                            SITE = SI_1 + 4.5F;
                            B_HG(SITE, TDATAR[treeIndex, 1], GP, out GEAGE, out PHTGRO);
                        }
                        IDXAGE = 120.0F;
                        HG_NWO(speciesGroup, PHTGRO, CR, TCCH, out HG);
                        break;
                    case Variant.Smc:
                        GP = 5.0F;
                        if (speciesGroup == 3)
                        {
                            // POTENTIAL HEIGHT GROWTH FROM FLEWELLING'S WESTERN HEMLOCK
                            // DOMINANT HEIGHT GROWTH
                            SITE = SI_2 + 4.5F;
                            F_HG(SITE, TDATAR[treeIndex, 1], GP, out GEAGE, out PHTGRO);
                        }
                        else
                        {
                            // POTENTIAL HEIGHT GROWTH FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH FOR DOUGLAS-FIR AND GRAND FIR
                            SITE = SI_1 + 4.5F;
                            B_HG(SITE, TDATAR[treeIndex, 1], GP, out GEAGE, out PHTGRO);
                        }
                        IDXAGE = 120.0F;
                        HG_SMC(speciesGroup, PHTGRO, CR, TCCH, out HG);
                        break;
                    case Variant.Rap:
                        GP = 1.0F;
                        if (speciesGroup == 1)
                        {
                            // POTENTIAL HEIGHT GROWTH FROM WEISKITTEL, HANN, HIBBS, LAM, AND BLUHM(2009) RED ALDER TOP HEIGHT GROWTH
                            SITE = SI_1 + 4.5F;
                            WHHLB_HG(SITE, PDEN, TDATAR[treeIndex, 1], GP, out GEAGE, out _);
                        }
                        else
                        {
                            if (speciesGroup == 3)
                            {
                                // POTENTIAL HEIGHT GROWTH FROM FLEWELLING'S WESTERN HEMLOCK DOMINANT HEIGHT GROWTH
                                SITE = -0.432F + 0.899F * (SI_2 + 4.5F);
                                F_HG(SITE, TDATAR[treeIndex, 1], GP, out GEAGE, out PHTGRO);
                            }
                            else
                            {
                                // POTENTIAL HEIGHT GROWTH FROM BRUCE'S (1981) DOMINANT HEIGHT GROWTH FOR DOUGLAS-FIR AND GRAND FIR
                                SITE = SI_2 + 4.5F;
                                B_HG(SITE, TDATAR[treeIndex, 1], GP, out GEAGE, out PHTGRO);
                            }
                            IDXAGE = 30.0F;
                            HG_RAP(speciesGroup, PHTGRO, CR, TCCH, out HG);
                        }
                        break;
                    default:
                        throw new NotSupportedException();
                }
                if (stand.IsBigSixSpecies(treeIndex) && (GEAGE > IDXAGE))
                {
                    OLD += 1.0F;
                    if (triplingMethod == 1 || (triplingMethod == 2 && ON == 1))
                    {
                        // BUGBUG should this be reachable when ON = 0? (tripling disabled?)
                        // BUGBUG encapsulation: Triple APIs should manage this
                        OLD += 2.0F;
                    }
                }

                HG_FERT(CYCLG, variant, speciesGroup, SI_1, PN, YF, out float FERTADJ);
                HG_THIN(CYCLG, variant, speciesGroup, BABT, BART, YT, out float THINADJ);
                GROWTH[treeIndex, 0] = HG * THINADJ * FERTADJ;
                LIMIT(variant, species, TDATAR[treeIndex, 0], TDATAR[treeIndex, 1], GROWTH[treeIndex, 1], ref GROWTH[treeIndex, 0]);
            }
        }

        public static void HTGRO2(int treeIndex, Variant variant, Stand stand, float[,] TDATAR, float RASI, float[,] CALIB, float[,] GROWTH)
        {
            // CALCULATE HEIGHT GROWTH FOR MINOR SPECIES
            float DBH = TDATAR[treeIndex, 0];
            int speciesGroup = stand.Integer[treeIndex, 1];
            if (stand.IsBigSixSpecies(treeIndex))
            {
                float PDBH = DBH - GROWTH[treeIndex, 1];
                float PRDHT1;
                float PRDHT2;
                switch (variant)
                {
                    case Variant.Swo:
                        HD_SWO(speciesGroup, DBH, out PRDHT2);
                        HD_SWO(speciesGroup, PDBH, out PRDHT1);
                        break;
                    case Variant.Nwo:
                        HD_NWO(speciesGroup, DBH, out PRDHT2);
                        HD_NWO(speciesGroup, PDBH, out PRDHT1);
                        break;
                    case Variant.Smc:
                        HD_SMC(speciesGroup, DBH, out PRDHT2);
                        HD_SMC(speciesGroup, PDBH, out PRDHT1);
                        break;
                    case Variant.Rap:
                        HD_RAP(speciesGroup, DBH, out PRDHT2);
                        HD_RAP(speciesGroup, PDBH, out PRDHT1);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                PRDHT1 = 4.5F + CALIB[speciesGroup, 0] * (PRDHT1 - 4.5F);
                PRDHT2 = 4.5F + CALIB[speciesGroup, 0] * (PRDHT2 - 4.5F);
                float PRDHT = (PRDHT2 / PRDHT1) * TDATAR[treeIndex, 1];

                // RED ALDER HEIGHT GROWTH
                FiaCode species = (FiaCode)stand.Integer[treeIndex, Constant.TreeIndex.Integer.Species];
                if ((species == FiaCode.AlnusRubra) && (variant <= Variant.Smc))
                {
                    RAGEA(TDATAR[treeIndex, 1], RASI, out float GEARA);
                    if (GEARA <= 0.0F)
                    {
                        GROWTH[treeIndex, 0] = 0.0F;
                    }
                    else
                    {
                        RAH40(GEARA, RASI, out float RAH1);
                        RAH40(GEARA + 5.0F, RASI, out float RAH2);
                        float RAHG = RAH2 - RAH1;
                        GROWTH[treeIndex, 0] = RAHG;
                    }
                }
                else
                {
                    GROWTH[treeIndex, 0] = PRDHT - TDATAR[treeIndex, 1];
                }
                GROWTH[treeIndex, 2] = GROWTH[treeIndex, 2] + GROWTH[treeIndex, 0];
            }
        }

        private static void LIMIT(Variant variant, FiaCode species, float DBH, float HT, float DG, ref float HG)
        {
            FiaCode speciesWithSwoTsheOptOut = species;
            if ((species == FiaCode.TsugaHeterophylla) && (variant == Variant.Swo))
            {
                // BUGBUG: not clear why SWO uses default coefficients for hemlock
                speciesWithSwoTsheOptOut = FiaCode.LithocarpusDensiflorus;
            }

            float A0;
            float A1;
            float A2;
            switch (speciesWithSwoTsheOptOut)
            {
                case FiaCode.PseudotsugaMenziesii:
                case FiaCode.TsugaHeterophylla:
                    A0 = 19.04942539F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
                case FiaCode.AbiesConcolor:
                case FiaCode.AbiesGrandis:
                    A0 = 16.26279948F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
                case FiaCode.PinusPonderosa:
                    A0 = 17.11482201F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
                case FiaCode.PinusLambertiana:
                    A0 = 14.29011403F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
                case FiaCode.AlnusRubra:
                    A0 = 60.619859F;
                    A1 = -1.59138564F;
                    A2 = 0.496705997F;
                    break;
                default:
                    A0 = 15.80319194F;
                    A1 = -0.04484724F;
                    A2 = 1.0F;
                    break;
            }

            float HT1 = HT - 4.5F;
            float HT2 = HT1 + HG;
            float HT3 = HT2 + HG;
            float DBH1 = DBH;
            float DBH2 = DBH1 + DG;
            float DBH3 = DBH2 + DG;
            float PHT1 = A0 * DBH1 / (1.0F - A1 * (float)Math.Pow(DBH1, A2));
            float PHT2 = A0 * DBH2 / (1.0F - A1 * (float)Math.Pow(DBH2, A2));
            float PHT3 = A0 * DBH3 / (1.0F - A1 * (float)Math.Pow(DBH3, A2));
            float PHGR1 = (PHT2 - PHT1 + HG) / 2.0F;
            float PHGR2 = PHT2 - HT1;
            if (HT2 > PHT2)
            {
                if (PHGR1 < PHGR2)
                {
                    HG = PHGR1;
                }
                else
                {
                    HG = PHGR2;
                }
            }
            else
            {
                if (HT3 > PHT3)
                {
                    HG = PHGR1;
                }
            }
            if (HG < 0.0F)
            {
                HG = 0.0F;
            }
        }

        public static void RAGEA(float H, float SI, out float GEA)
        {
            // RED ALDER GROWTH EFFECTIVE AGE EQUATION BASED ON H40 EQUATION FROM
            // WORTHINGTON, JOHNSON, STAEBLER AND LLOYD(1960) PNW RESEARCH PAPER 36
            GEA = 19.538F * H / (SI - 0.60924F * H);
        }

        public static void RAH40(float A, float SI, out float H)
        {
            // RED ALDER H40 EQUATION FROM FROM WORTHINGTON, JOHNSON, STAEBLER AND LLOYD(1960) PNW RESEARCH PAPER 36
            H = SI / (0.60924F + 19.538F / A);
        }

        public static void WHHLB_GEA(float H, float SI_UC, out float GEA)
        {
            // RED ALDER GROWTH EFFECTIVE AGE EQUATION BASED ON H40 EQUATION FROM
            // THE WEISKITTEL, HANN, HIBBS, LAM, AND BLUHM DOMINANT HEIGHT GROWTH EQUATION
            float B1 = -4.481266F;
            float B2 = -0.658884F;
            float X = (1.0F / B1) * (float)(Math.Log(H / SI_UC) + Math.Pow(20.0, B2));
            if (X < 0.03F)
            {
                X = 0.03F;
            }
            GEA = (float)Math.Pow(X, 1.0 / B2);
        }

        public static void WHHLB_H40(float H40M, float TAGEM, float TAGEP, out float PH40P)
        {
            // WEISKITTEL, HANN, HIBBS, LAM, AND BLUHM DOMINANT HEIGHT GROWTH EQUATION FOR RED ALDER
            float B1 = -4.481266F;
            float B2 = -0.658884F;
            PH40P = H40M * (float)Math.Exp(B1 * (Math.Pow(TAGEP, B2) - Math.Pow(TAGEM, B2)));
        }

        private static void WHHLB_HG(float SI_C, float PDEN, float HT, float GP, out float GEA, out float POTHGRO)
        {
            // WEISKITTEL, HANN, HIBBS, LAM, AND BLUHM DOMINANT HEIGHT GROWTH INCREMENT EQUATION FOR RED ALDER
            WHHLB_SI_UC(SI_C, PDEN, out float SI_UC);
            WHHLB_GEA(HT, SI_UC, out GEA);
            float A = GEA + GP;
            WHHLB_H40(HT, GEA, A, out float PHT);
            POTHGRO = PHT - HT;
        }

        public static void WHHLB_SI_UC(float SI_C, float PDEN, out float SI_UC)
        {
            // UNCORRECTS THE DENSITY INPACT UPON THE WEISKITTEL, HANN, HIBBS, LAM, AND BLUHN SITE INDEX FOR RED ALDER
            // SITE INDEX UNCORRECTED FOR DENSITY EFFECT
            SI_UC = SI_C * (1.0F - 0.326480904F * (float)Math.Exp(-0.000400268678 * Math.Pow(PDEN, 1.5)));
        }
    }
}

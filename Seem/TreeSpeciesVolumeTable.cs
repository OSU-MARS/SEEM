using System;
using System.Diagnostics;

namespace Osu.Cof.Ferm
{
    public class TreeSpeciesVolumeTable
    {
        // Scribner lumber recovery from BC Firmwood cubic scale
        // Indexed by small end diameter in centimeters.
        private static readonly float[] BoardFootRecoveryPerCubicMeter = new float[]
        {
                /* 0*/ 0.0F,
                /* 1*/ 0.0F,
                /* 2*/ 0.0F,
                /* 3*/ 0.0F,
                /* 4*/ 0.0F,
                /* 5*/ 0.0F,
                /* 6*/ 0.0F,
                /* 7*/ 0.0F,
                /* 8*/ 0.0F,
                /* 9*/ 24.8848F,
                /*10*/ 42.3848F,
                /*11*/ 59.8848F,
                /*12*/ 77.3848F,
                /*13*/ 94.8848F,
                /*14*/ 103.7320F,
                /*15*/ 112.5792F,
                /*16*/ 119.7373F,
                /*17*/ 126.8955F,
                /*18*/ 134.0536F,
                /*19*/ 140.0180F,
                /*20*/ 145.9824F,
                /*21*/ 150.4276F,
                /*22*/ 154.8728F,
                /*23*/ 159.3180F,
                /*24*/ 160.8720F,
                /*25*/ 162.4260F,
                /*26*/ 164.4190F,
                /*27*/ 166.4120F,
                /*28*/ 168.4050F,
                /*29*/ 170.3520F,
                /*30*/ 172.2990F,
                /*31*/ 173.0623F,
                /*32*/ 173.8257F,
                /*33*/ 174.5890F,
                /*34*/ 174.7475F,
                /*35*/ 174.9060F,
                /*36*/ 175.0645F,
                /*37*/ 175.0645F,
                /*38*/ 175.0645F,
                /*39*/ 177.6873F,
                /*40*/ 180.3102F,
                /*41*/ 182.9330F,
                /*42*/ 186.8365F,
                /*43*/ 190.7400F,
                /*44*/ 190.7400F,
                /*45*/ 190.7400F,
                /*46*/ 190.7400F,
                /*47*/ 190.7400F,
                /*48*/ 190.7400F,
                /*49*/ 194.4767F,
                /*50*/ 198.2133F,
                /*51*/ 201.9500F,
                /*52*/ 205.6390F,
                /*53*/ 209.3280F,
                /*54*/ 215.6880F,
                /*55*/ 222.0480F,
                /*56*/ 228.4080F,
                /*57*/ 228.4080F,
                /*58*/ 228.4080F,
                /*59*/ 232.4920F,
                /*60*/ 236.5760F,
                /*61*/ 240.6600F,
                /*62*/ 238.1467F,
                /*63*/ 235.6333F,
                /*64*/ 233.1200F,
                /*65*/ 228.9205F,
                /*66*/ 224.7210F,
                /*67*/ 222.1720F,
                /*68*/ 219.6230F,
                /*69*/ 217.0740F,
                /*70*/ 213.2010F,
                /*71*/ 209.3280F,
                /*72*/ 206.8687F,
                /*73*/ 204.4093F,
                /*74*/ 201.9500F,
                /*75*/ 198.0440F,
                /*76*/ 194.1380F,
                /*77*/ 194.1308F,
                /*78*/ 194.1308F,
                /*79*/ 194.1308F,
                /*80*/ 194.1308F,
                /*81*/ 194.1308F,
                /*82*/ 194.1308F,
                /*83*/ 194.1308F,
                /*84*/ 194.1308F,
                /*85*/ 194.1308F,
                /*86*/ 194.1308F,
                /*87*/ 194.1308F,
                /*88*/ 194.1308F,
                /*89*/ 194.1308F,
                /*90*/ 194.1308F,
                /*91*/ 194.1308F,
                /*92*/ 194.1308F,
                /*93*/ 194.1308F,
                /*94*/ 194.1308F,
                /*95*/ 194.1308F,
                /*96*/ 194.1308F,
                /*97*/ 194.1308F,
                /*98*/ 194.1308F,
                /*99*/ 194.1308F,
               /*100*/ 194.1308F
        };
        private static readonly float[,] ScribnerCLongLog = new float[,] {
            // small end diameter, inch
            // |      length, feet
            // |      0    1    2    3    4    5    6    7    8    9    10   11   12   13   14   15   16   17   18   19   20   21   22   23   24   25   26   27   28   29   30   31   32   33   34   35   36   37   38   39   40
            /* 0*/ {  0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0 },
            /* 1*/ {  0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0 },
            /* 2*/ {  0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0 },
            /* 3*/ {  0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   2,   2 },
            /* 4*/ {  0,   0,   0,   0,   0,   0,   0,   0,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   2,   2,   2,   2,   2,   2,   2,   2,   2,   2,   2,   2,   2,   2,   3,   3,   3,   3 },
            /* 5*/ {  0,   0,   0,   0,   0,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   2,   2,   2,   2,   2,   2,   2,   2,   2,   3,   3,   3,   3,   3,   3,   3,   3,   3,   4,   4,   4,   4,   4,   4,   4,   4 },
            /* 6*/ {  0,   0,   0,   0,   0,   1,   1,   1,   1,   1,   1,   1,   1,   2,   2,   2,   2,   2,   2,   2,   2,   3,   3,   3,   3,   3,   3,   3,   3,   4,   4,   4,   5,   5,   5,   5,   6,   6,   6,   6,   6 },
            /* 7*/ {  0,   0,   0,   0,   1,   1,   1,   1,   1,   1,   1,   2,   2,   2,   2,   2,   3,   3,   3,   3,   3,   3,   4,   4,   4,   4,   4,   4,   5,   5,   5,   5,   6,   6,   6,   6,   6,   7,   7,   7,   7 },
            /* 8*/ {  0,   0,   0,   0,   1,   1,   1,   1,   1,   1,   2,   2,   2,   2,   2,   2,   3,   3,   3,   4,   4,   4,   4,   4,   4,   5,   5,   5,   5,   5,   6,   6,   7,   7,   7,   8,   8,   8,   8,   9,   9 },
            /* 9*/ {  0,   0,   0,   1,   1,   1,   1,   1,   2,   2,   2,   2,   3,   3,   3,   3,   4,   4,   4,   5,   5,   5,   5,   6,   6,   6,   6,   7,   7,   7,   7,   7,   9,   10,  10,  10,  10,  11,  11,  11,  12 },
            /*10*/ {  0,   0,   1,   1,   1,   2,   2,   2,   3,   3,   3,   3,   4,   4,   4,   5,   6,   6,   6,   7,   7,   7,   8,   8,   9,   9,   9,   10,  10,  10,  11,  11,  12,  13,  13,  13,  14,  14,  14,  15,  15 },
            /*11*/ {  0,   0,   1,   1,   1,   2,   2,   3,   3,   3,   4,   4,   4,   5,   5,   6,   7,   7,   8,   8,   8,   9,   9,   10,  10,  10,  11,  11,  12,  12,  13,  13,  14,  15,  15,  16,  16,  17,  17,  18,  18 },
            /*12*/ {  0,   0,   1,   1,   2,   2,   3,   3,   4,   4,   5,   5,   6,   6,   7,   7,   8,   8,   9,   9,   10,  10,  11,  11,  12,  12,  13,  13,  14,  14,  15,  15,  16,  16,  17,  17,  18,  18,  19,  19,  20 },
            /*13*/ {  0,   1,   1,   2,   2,   3,   4,   4,   5,   5,   6,   7,   7,   8,   8,   9,   10,  10,  11,  11,  12,  13,  13,  14,  15,  15,  16,  16,  17,  18,  18,  19,  19,  20,  21,  21,  22,  22,  23,  24,  24 },
            /*14*/ {  0,   1,   1,   2,   3,   4,   4,   5,   6,   6,   7,   8,   9,   9,   10,  11,  11,  12,  13,  14,  14,  15,  16,  16,  17,  18,  19,  19,  20,  21,  21,  22,  23,  24,  24,  25,  26,  26,  27,  28,  29 },
            /*15*/ {  0,   1,   2,   3,   4,   4,   5,   6,   7,   8,   9,   10,  11,  12,  12,  13,  14,  15,  16,  17,  18,  19,  20,  20,  21,  22,  23,  24,  25,  26,  27,  28,  28,  29,  30,  31,  32,  33,  34,  35,  36 },
            /*16*/ {  0,   1,   2,   3,   4,   5,   6,   7,   8,   9,   10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23,  24,  25,  26,  27,  28,  29,  30,  31,  32,  33,  34,  35,  36,  37,  38,  39,  40 },
            /*17*/ {  0,   1,   2,   3,   5,   6,   7,   8,   9,   10,  12,  13,  14,  15,  16,  17,  18,  20,  21,  22,  23,  24,  25,  27,  28,  29,  30,  31,  32,  33,  35,  36,  37,  38,  39,  40,  42,  43,  44,  45,  46 },
            /*18*/ {  0,   1,   3,   4,   5,   7,   8,   9,   11,  12,  13,  15,  16,  17,  19,  20,  21,  23,  24,  25,  27,  28,  29,  31,  32,  33,  35,  36,  37,  39,  40,  41,  43,  44,  45,  47,  48,  49,  51,  52,  53 },
            /*19*/ {  0,   1,   3,   4,   6,   7,   9,   10,  12,  13,  15,  16,  18,  19,  21,  22,  24,  25,  27,  28,  30,  31,  33,  34,  36,  37,  39,  40,  42,  43,  45,  46,  48,  49,  51,  52,  54,  55,  57,  58,  60 },
            /*20*/ {  0,   2,   3,   5,   7,   9,   10,  12,  14,  16,  17,  19,  21,  23,  24,  26,  28,  30,  31,  33,  35,  37,  38,  40,  42,  44,  45,  47,  49,  51,  52,  54,  56,  58,  59,  61,  63,  65,  66,  68,  70 },
            /*21*/ {  0,   2,   4,   6,   8,   9,   11,  13,  15,  17,  19,  21,  23,  25,  27,  28,  30,  32,  34,  36,  38,  40,  42,  44,  46,  47,  49,  51,  53,  55,  57,  59,  61,  63,  65,  66,  68,  70,  72,  74,  76 },
            /*22*/ {  0,   2,   4,   6,   8,   10,  13,  15,  17,  19,  21,  23,  25,  27,  29,  31,  33,  35,  38,  40,  42,  44,  46,  48,  50,  52,  54,  56,  58,  61,  63,  65,  67,  69,  71,  73,  75,  77,  79,  81,  84 },
            /*23*/ {  0,   2,   5,   7,   9,   12,  14,  16,  19,  21,  24,  26,  28,  31,  33,  35,  38,  40,  42,  45,  47,  49,  52,  54,  56,  59,  61,  63,  66,  68,  71,  73,  75,  78,  80,  82,  85,  87,  89,  92,  94 },
            /*24*/ {  0,   3,   5,   8,   10,  13,  15,  18,  20,  23,  25,  28,  30,  33,  35,  38,  40,  43,  45,  48,  50,  53,  55,  58,  61,  63,  66,  68,  71,  73,  76,  78,  81,  83,  86,  88,  91,  93,  96,  98,  101 },
            /*25*/ {  0,   3,   6,   9,   11,  14,  17,  20,  23,  26,  29,  32,  34,  37,  40,  43,  46,  49,  52,  54,  57,  60,  63,  66,  69,  72,  75,  77,  80,  83,  86,  89,  92,  95,  98,  100, 103, 106, 109, 112, 115 },
            /*26*/ {  0,   3,   6,   9,   12,  16,  19,  22,  25,  28,  31,  34,  37,  41,  44,  47,  50,  53,  56,  59,  62,  66,  69,  72,  75,  78,  81,  84,  87,  91,  94,  97,  100, 103, 106, 109, 112, 116, 119, 122, 125 },
            /*27*/ {  0,   3,   7,   10,  14,  17,  21,  24,  27,  31,  34,  38,  41,  44,  48,  51,  55,  58,  62,  65,  68,  72,  75,  79,  82,  86,  89,  92,  96,  99,  103, 106, 110, 113, 116, 120, 123, 127, 130, 133, 137 },
            /*28*/ {  0,   4,   7,   11,  15,  18,  22,  25,  29,  33,  36,  40,  44,  47,  51,  55,  58,  62,  65,  69,  73,  76,  80,  84,  87,  91,  95,  98,  102, 105, 109, 113, 116, 120, 124, 127, 131, 135, 138, 142, 146 },
            /*29*/ {  0,   4,   8,   11,  15,  19,  23,  27,  30,  34,  38,  42,  46,  49,  53,  57,  61,  65,  68,  72,  76,  80,  84,  87,  91,  95,  99,  103, 107, 110, 114, 118, 122, 126, 129, 133, 137, 141, 145, 148, 152 },
            /*30*/ {  0,   4,   8,   12,  16,  21,  25,  29,  33,  37,  41,  45,  49,  53,  57,  62,  66,  70,  74,  78,  82,  86,  90,  94,  99,  103, 107, 111, 115, 119, 123, 127, 131, 135, 140, 144, 148, 152, 156, 160, 164 },
            /*31*/ {  0,   4,   9,   13,  18,  22,  27,  31,  36,  40,  44,  49,  53,  58,  62,  67,  71,  75,  80,  84,  89,  93,  98,  102, 107, 111, 115, 120, 124, 129, 133, 138, 142, 146, 151, 155, 160, 164, 169, 173, 178 },
            /*32*/ {  0,   5,   9,   14,  18,  23,  28,  32,  37,  41,  46,  51,  55,  60,  64,  69,  74,  78,  83,  87,  92,  97,  101, 106, 110, 115, 120, 124, 129, 133, 138, 143, 147, 152, 156, 161, 166, 170, 175, 179, 184 },
            /*33*/ {  0,   5,   10,  15,  20,  24,  29,  34,  39,  44,  49,  54,  59,  64,  69,  73,  78,  83,  88,  93,  98,  103, 108, 113, 118, 122, 127, 132, 137, 142, 147, 152, 157, 162, 167, 171, 176, 181, 186, 191, 196, },
            /*34*/ {  0,   5,   10,  15,  20,  25,  30,  35,  40,  45,  50,  55,  60,  65,  70,  75,  80,  85,  90,  95,  100, 105, 110, 115, 120, 125, 130, 135, 140, 145, 150, 155, 160, 165, 170, 175, 180, 185, 190, 195, 200, },
            /*35*/ {  0,   5,   11,  16,  22,  27,  33,  38,  44,  49,  55,  60,  66,  71,  77,  82,  88,  93,  98,  104, 109, 115, 120, 126, 131, 137, 142, 148, 153, 159, 164, 170, 175, 180, 186, 191, 197, 202, 208, 213, 219 },
            /*36*/ {  0,   6,   12,  17,  23,  29,  35,  40,  46,  52,  58,  63,  69,  75,  81,  86,  92,  98,  104, 110, 115, 121, 127, 133, 138, 144, 150, 156, 161, 167, 173, 179, 185, 190, 196, 202, 208, 213, 219, 225, 231 },
            /*37*/ {  0,   6,   13,  19,  26,  32,  39,  45,  51,  58,  64,  71,  77,  84,  90,  96,  103, 109, 116, 122, 129, 135, 142, 148, 154, 161, 167, 174, 180, 187, 193, 199, 206, 212, 219, 225, 232, 238, 244, 251, 257 },
            /*38*/ {  0,   7,   13,  20,  27,  33,  40,  47,  53,  60,  67,  73,  80,  87,  93,  100, 107, 113, 120, 127, 133, 140, 147, 153, 160, 167, 173, 180, 187, 194, 200, 207, 214, 220, 227, 234, 240, 247, 254, 260, 267 },
            /*39*/ {  0,   7,   14,  21,  28,  35,  42,  49,  56,  63,  70,  77,  84,  91,  98, 105, 112, 119, 126, 133, 140, 147, 154, 161, 168, 175, 182, 189, 196, 203, 210, 217, 224, 231, 238, 245, 252, 259, 266, 273, 280 },
            /*40*/ {  0,   8,   15,  23,  30,  38,  45,  53,  60,  68,  75,  83,  90,  98, 105, 113, 120, 128, 135, 143, 150, 158, 166, 173, 181, 188, 196, 203, 211, 218, 226, 233, 241, 248, 256, 263, 271, 278, 286, 293, 301 },
            /*41*/ {  0,   8,   16,  24,  32,  40,  48,  56,  64,  72,  79,  87,  95, 103, 111, 119, 127, 135, 143, 151, 159, 167, 175, 183, 191, 199, 207, 215, 223, 230, 238, 246, 254, 262, 270, 278, 286, 294, 302, 310, 318 },
            /*42*/ {  0,   8,   17,  25,  34,  42,  50,  59,  67,  76,  84,  92, 101, 109, 117, 126, 134, 143, 151, 159, 168, 176, 185, 193, 201, 210, 218, 227, 235, 243, 252, 260, 269, 277, 285, 294, 302, 310, 319, 327, 336 },
            /*43*/ {  0,   9,   17,  26,  35,  44,  52,  61,  70,  78,  87,  96, 105, 113, 122, 131, 140, 148, 157, 166, 174, 183, 192, 201, 209, 218, 227, 235, 244, 253, 262, 270, 279, 288, 296, 305, 314, 323, 331, 340, 349 },
            /*44*/ {  0,   9,   19,  28,  37,  46,  56,  65,  74,  83,  93, 102, 111, 120, 130, 139, 148, 157, 167, 176, 185, 194, 204, 213, 222, 231, 241, 250, 259, 268, 278, 287, 296, 305, 315, 324, 333, 342, 352, 361, 370 },
            /*45*/ {  0,   9,   19,  28,  38,  47,  57,  66,  76,  85,  95, 104, 114, 123, 133, 142, 152, 161, 171, 180, 190, 199, 209, 218, 228, 237, 247, 256, 266, 275, 285, 294, 304, 313, 323, 332, 342, 351, 361, 370, 380 },
            /*46*/ {  0,   10,  20,  30,  40,  50,  59,  69,  79,  89,  99, 109, 119, 129, 139, 149, 159, 168, 178, 188, 198, 208, 218, 228, 238, 248, 258, 268, 277, 287, 297, 307, 317, 327, 337, 347, 357, 367, 376, 386, 396 },
            /*47*/ {  0,   10,  21,  31,  41,  52,  62,  72,  83,  93, 104, 114, 124, 135, 145, 155, 166, 176, 186, 197, 207, 217, 228, 238, 248, 259, 269, 279, 290, 300, 311, 321, 331, 342, 352, 362, 373, 383, 393, 404, 414, },
            /*48*/ {  0,   11,  22,  32,  43,  54,  65,  76,  86,  97, 108, 119, 130, 140, 151, 162, 173, 184, 194, 205, 216, 227, 238, 248, 259, 270, 281, 292, 302, 313, 324, 335, 346, 356, 367, 378, 389, 399, 410, 421, 432 },
            /*49*/ {  0,   11,  22,  34,  45,  56,  67,  79,  90, 101, 112, 124, 135, 146, 157, 168, 180, 191, 202, 213, 225, 236, 247, 258, 270, 281, 292, 303, 314, 326, 337, 348, 359, 371, 382, 393, 404, 415, 427, 438, 449 },
            /*50*/ {  0,   12,  23,  35,  47,  58,  70,  82,  94, 105, 117, 129, 140, 152, 164, 175, 187, 199, 211, 222, 234, 246, 257, 269, 281, 292, 304, 316, 328, 339, 351, 363, 374, 386, 398, 409, 421, 433, 445, 456, 468 }
            };

        public float[,] Cubic2Saw { get; private init; } // BC Firmwood cubic volume by DBH and height class, m³/tree
        public float[,] Cubic3Saw { get; private init; } // BC Firmwood cubic volume by DBH and height class, m³/tree
        public float[,] Cubic4Saw { get; private init; } // BC Firmwood cubic volume by DBH and height class, m³/tree

        public float DiameterClassSizeInCentimeters { get; private init; }
        public float HeightClassSizeInMeters { get; private init; }

        public float MaximumDiameterInCentimeters { get; private init; }
        public float MaximumHeightInMeters { get; private init; }

        public float[,] Scribner2Saw { get; private init; } // Scriber board foot volume by DBH and height class, MBF/tree
        public float[,] Scribner3Saw { get; private init; } // Scriber board foot volume by DBH and height class, MBF/tree
        public float[,] Scribner4Saw { get; private init; } // Scriber board foot volume by DBH and height class, MBF/tree

        public TreeSpeciesVolumeTable(float maximumDiameterInCentimeters, float maximumHeightInMeters, float preferredLogLengthInMeters, Func<float, float, float, float> getDiameterInsideBark, Func<float, float> getNeiloidHeight, bool scribnerFromLumberRecovery)
        {
            if (preferredLogLengthInMeters > Constant.MetersPerFoot * 40.0F)
            {
                throw new NotSupportedException();
            }

            this.DiameterClassSizeInCentimeters = Constant.Bucking.DiameterClassSizeInCentimeters;
            this.HeightClassSizeInMeters = Constant.Bucking.HeightClassSizeInMeters;
            this.MaximumDiameterInCentimeters = maximumDiameterInCentimeters;
            this.MaximumHeightInMeters = maximumHeightInMeters;

            int diameterClasses = (int)(this.MaximumDiameterInCentimeters / this.DiameterClassSizeInCentimeters) + 1;
            int heightClasses = (int)(this.MaximumHeightInMeters / this.HeightClassSizeInMeters) + 1;
            this.Cubic2Saw = new float[diameterClasses, heightClasses];
            this.Cubic3Saw = new float[diameterClasses, heightClasses];
            this.Cubic4Saw = new float[diameterClasses, heightClasses];
            this.Scribner2Saw = new float[diameterClasses, heightClasses];
            this.Scribner3Saw = new float[diameterClasses, heightClasses];
            this.Scribner4Saw = new float[diameterClasses, heightClasses];

            // fill cubic and Scribner volume tables
            // start at index 1 since trees in zero diameter class have zero merchantable volume
            float defaultScribnerTrim = preferredLogLengthInMeters > Constant.Bucking.ScribnerShortLogLength ? Constant.Bucking.ScribnerTrimLongLog : Constant.Bucking.ScribnerTrimShortLog;
            float preferredLogLengthWithTrim = preferredLogLengthInMeters + defaultScribnerTrim;
            for (int dbhIndex = 1; dbhIndex < diameterClasses; ++dbhIndex)
            {
                float dbh = this.GetDiameter(dbhIndex);
                if (dbh < Constant.Bucking.MinimumScalingDiameter4Saw)
                {
                    // tree cannot produce a merchantable log
                    // Except in special case of a minimum log length of DBH or less, which isn't supported.
                    Debug.Assert(Constant.Bucking.MinimumLogLength4Saw >= 1.3F);
                    continue;
                }

                // start at index 1 since trees in zero height class have zero merchantable volume
                for (int heightIndex = 1; heightIndex < heightClasses; ++heightIndex)
                {
                    float height = this.GetHeight(heightIndex);
                    if (height < Constant.Bucking.MinimumLogLength4Saw + Constant.Bucking.StumpHeight)
                    {
                        // tree cannot produce a merchantable log
                        // This also avoids breakdown in Kozak 2004 form taper equations, which require trees be at least 1.3 m tall.
                        continue;
                    }

                    // with current simplified bucking, diameter inside bark only needs to be evaluated at log ends
                    // TODO: what do scalers actually do, particularly on larger logs?
                    float neiloidHeight = getNeiloidHeight(dbh);
                    float previousLogLengthWithTrim;
                    for (float logBottomHeight = Constant.Bucking.StumpHeight; logBottomHeight < height - Constant.Bucking.MinimumLogLength4Saw; logBottomHeight += previousLogLengthWithTrim + Constant.Bucking.KerfProcessingHead)
                    {
                        float logMinimumTopHeight = logBottomHeight + Constant.Bucking.MinimumLogLength4Saw;
                        if (logMinimumTopHeight > height)
                        {
                            break; // no merchantable log: done with tree
                        }

                        float logMaximumTopHeight = Math.Min(height, logBottomHeight + preferredLogLengthWithTrim);
                        float logMinimumTopDib = getDiameterInsideBark(dbh, height, logMaximumTopHeight);
                        float logTopHeight = logMaximumTopHeight;
                        float logTopDib = logMinimumTopDib;
                        if (logMinimumTopDib < Constant.Bucking.MinimumScalingDiameter4Saw)
                        {
                            float logMaximumTopDib = getDiameterInsideBark(dbh, height, logMinimumTopHeight);
                            if (logMaximumTopDib < Constant.Bucking.MinimumScalingDiameter4Saw)
                            {
                                break; // log undersize: done with tree
                            }

                            // potential for incremental performance and accuracy improvement from binary or golden section search
                            // seeded from minimum and maximum dibs
                            logTopHeight = logMinimumTopHeight;
                            logTopDib = logMaximumTopDib;
                            for (float logCandidateTopHeight = logMinimumTopHeight; logCandidateTopHeight < logMaximumTopHeight; logCandidateTopHeight += Constant.Bucking.EvaluationHeightStep)
                            {
                                float logCandidateTopDib = getDiameterInsideBark(dbh, height, logCandidateTopHeight);
                                if (logCandidateTopDib < Constant.Bucking.MinimumScalingDiameter4Saw)
                                {
                                    break;
                                }
                                logTopHeight = logCandidateTopHeight;
                                logTopDib = logCandidateTopDib;
                            }
                        }

                        float logLength = logTopHeight - logBottomHeight;
                        Debug.Assert(logLength >= Constant.Bucking.MinimumLogLength4Saw - 0.0001F);
                        // BC Firmwood cubic and Scribner.C long log volume
                        // Fonseca, M. 2005. The Measurement of Roundwood: Methodologies and Conversion Ratios. United Nations Economic 
                        //   Commission for Europe Trade and Timber Branch. Cromwell Press, Trowbridge. https://www.cabi.org/bookshop/book/9780851990798/
                        // section 2.2.2: BC Firmwood (0.5 * 0.0001 * pi = 0.00015708)
                        // section 2.3.2: Scribner long log
                        // Logs are assumed perfectly round, equivalent to assuming Poudel 2018 taper provides log rules' mean diameter.
                        // get BC firmwood volume
                        float bcFirmwoodBottomDiameter;
                        if (logBottomHeight > neiloidHeight)
                        {
                            // bottom of log is above base neiloid
                            bcFirmwoodBottomDiameter = getDiameterInsideBark(dbh, height, logBottomHeight);
                        }
                        else
                        {
                            // caliper above neiloid and project per Fonseca section 2.2.2.1.6
                            float bcFirmwoodLogBottomHeight = Math.Min(neiloidHeight, logTopHeight - 0.5F); // avoid math errors on very low height-diameter ratio trees
                            float bcFirmwoodCaliperDibAboveNeiloid = getDiameterInsideBark(dbh, height, bcFirmwoodLogBottomHeight);
                            float bcFirmwoodProjectionTaper = (bcFirmwoodCaliperDibAboveNeiloid - logTopDib) / (logTopHeight - bcFirmwoodLogBottomHeight); // taper in diameter
                            bcFirmwoodBottomDiameter = bcFirmwoodCaliperDibAboveNeiloid + bcFirmwoodProjectionTaper * (bcFirmwoodLogBottomHeight - logBottomHeight);
                            Debug.Assert(bcFirmwoodBottomDiameter > bcFirmwoodCaliperDibAboveNeiloid);
                            Debug.Assert(bcFirmwoodBottomDiameter < (1.0F + 0.01F * dbh / height * dbh / height) * getDiameterInsideBark(dbh, height, logBottomHeight)); // allow substantial overshoot on low height-diameter ratio trees
                        }
                        float bcFirmwoodBottomRadius = MathF.Round(0.5F * bcFirmwoodBottomDiameter); // rounded cm = diameter in rads
                        float bcFirmwoodTopRadius = MathF.Round(0.5F * logTopDib); // rounded cm = diameter in rads
                        float logCubicVolume = -1.0F;
                        float logScribnerVolume = -1.0F;
                        if (bcFirmwoodBottomRadius > 1.5F * bcFirmwoodTopRadius)
                        {
                            float logSegments = MathF.Round(logLength / Constant.Bucking.LogTaperSegmentationLength);
                            if (logSegments >= 2.0F)
                            {
                                float bcFirmwoodTaperHeight = logBottomHeight + MathF.Floor(0.5F * logSegments + 0.01F) * Constant.Bucking.LogTaperSegmentationLength;
                                if (bcFirmwoodTaperHeight < logTopHeight)
                                {
                                    float bcFirmwoodTaperDiameter = getDiameterInsideBark(dbh, height, bcFirmwoodTaperHeight);
                                    float bcFirmwoodTaperRadius = MathF.Round(0.5F * bcFirmwoodTaperDiameter);
                                    // Debug.Assert(bcFirmwoodBottomRadius <= 1.5F * bcFirmwoodTaperRadius); // violated by low height-diameter ratio trees
                                    // Debug.Assert(bcFirmwoodTaperRadius <= 1.5F * bcFirmwoodTopRadius); // prone to 10 cm taper radius with 6 cm top

                                    float bcFirmwoodLengthBelowTaper = MathF.Round(bcFirmwoodTaperHeight - logBottomHeight, 1, MidpointRounding.AwayFromZero);
                                    float bcFirmwoodLengthAboveTaper = MathF.Round(logTopHeight - bcFirmwoodTaperHeight, 1, MidpointRounding.AwayFromZero);
                                    float logLowerCubicVolume = MathF.Round(0.5F * 0.0001F * Constant.Pi * (bcFirmwoodTaperRadius * bcFirmwoodTaperRadius + bcFirmwoodBottomRadius * bcFirmwoodBottomRadius) * bcFirmwoodLengthBelowTaper, 3);
                                    float logUpperCubicVolume = MathF.Round(0.5F * 0.0001F * Constant.Pi * (bcFirmwoodTaperRadius * bcFirmwoodTaperRadius + bcFirmwoodTopRadius * bcFirmwoodTopRadius) * bcFirmwoodLengthAboveTaper, 3);
                                    logCubicVolume = logLowerCubicVolume + logUpperCubicVolume;

                                    if (scribnerFromLumberRecovery)
                                    {
                                        int scribnerLowerTopDiameter = (int)Math.Round(bcFirmwoodTaperDiameter);
                                        int scribnerUpperTopDiameter = (int)Math.Round(logTopDib);
                                        logScribnerVolume = TreeSpeciesVolumeTable.BoardFootRecoveryPerCubicMeter[scribnerLowerTopDiameter] * logLowerCubicVolume +
                                                            TreeSpeciesVolumeTable.BoardFootRecoveryPerCubicMeter[scribnerUpperTopDiameter] * logUpperCubicVolume;
                                    }
                                }
                            }
                        }
                        if (logCubicVolume < 0.0F)
                        {
                            float bcFirmwoodLength = MathF.Round(logLength, 1, MidpointRounding.AwayFromZero);
                            logCubicVolume = MathF.Round(0.5F * 0.0001F * Constant.Pi * (bcFirmwoodTopRadius * bcFirmwoodTopRadius + bcFirmwoodBottomRadius * bcFirmwoodBottomRadius) * bcFirmwoodLength, 3); // 0.0001 * cm² * m = m³

                            if (scribnerFromLumberRecovery)
                            {
                                int scribnerTopDiameter = (int)Math.Round(logTopDib);
                                logScribnerVolume = TreeSpeciesVolumeTable.BoardFootRecoveryPerCubicMeter[scribnerTopDiameter] * logCubicVolume;
                            }
                        }

                        // get Scribner long log volume if it wasn't recovered from cubic volume
                        if (scribnerFromLumberRecovery == false)
                        {
                            int scribnerDiameterInInches = (int)MathF.Floor(Constant.InchesPerCentimeter * logTopDib);
                            int scribnerLengthInFeet = (int)MathF.Floor(Constant.FeetPerMeter * logLength);
                            logScribnerVolume = 10.0F * TreeSpeciesVolumeTable.ScribnerCLongLog[scribnerDiameterInInches, scribnerLengthInFeet];
                        }

                        Debug.Assert(logCubicVolume > 0.0F);
                        Debug.Assert(logScribnerVolume > 0.0F);
                        if ((logTopDib >= Constant.Bucking.MinimumScalingDiameter2Saw) &&
                            (logLength >= Constant.Bucking.MinimumLogLength2Saw) &&
                            (logScribnerVolume >= Constant.Bucking.MinimumLogScribner2Saw))
                        {
                            // 2S
                            this.Cubic2Saw[dbhIndex, heightIndex] += logCubicVolume;
                            this.Scribner2Saw[dbhIndex, heightIndex] += logScribnerVolume;
                        }
                        else if ((logTopDib >= Constant.Bucking.MinimumScalingDiameter3Saw) &&
                                 (logLength >= Constant.Bucking.MinimumLogLength3Saw) &&
                                 (logScribnerVolume >= Constant.Bucking.MinimumLogScribner3Saw))
                        {
                            // 3S
                            this.Cubic3Saw[dbhIndex, heightIndex] += logCubicVolume;
                            this.Scribner3Saw[dbhIndex, heightIndex] += logScribnerVolume;
                        }
                        else if (logScribnerVolume >= Constant.Bucking.MinimumLogScribner4Saw)
                        {
                            // 4S
                            Debug.Assert(logTopDib >= Constant.Bucking.MinimumLogLength4Saw);
                            Debug.Assert(logTopDib >= Constant.Bucking.MinimumScalingDiameter4Saw);
                            this.Cubic4Saw[dbhIndex, heightIndex] += logCubicVolume;
                            this.Scribner4Saw[dbhIndex, heightIndex] += logScribnerVolume;
                        }
                        // else log not merchantable

                        float scribnerTrim = preferredLogLengthInMeters > Constant.Bucking.ScribnerShortLogLength ? Constant.Bucking.ScribnerTrimLongLog : Constant.Bucking.ScribnerTrimShortLog;
                        previousLogLengthWithTrim = logLength + scribnerTrim;
                    }
                }
            }
        }

        public int DiameterClasses
        {
            get { return this.Cubic2Saw.GetLength(0); }
        }

        public int HeightClasses
        {
            get { return this.Cubic2Saw.GetLength(1); }
        }

        public float GetDiameter(int dbhIndex)
        {
            return this.DiameterClassSizeInCentimeters * dbhIndex;
        }

        public float GetHeight(int heightIndex)
        {
            return this.HeightClassSizeInMeters * heightIndex;
        }

        public int ToDiameterIndex(float dbhInCentimeters)
        {
            // TODO: shift rounding to reduce bias?
            return (int)((dbhInCentimeters + 0.5F * this.DiameterClassSizeInCentimeters) / this.DiameterClassSizeInCentimeters);
        }

        public int ToHeightIndex(float heightInMeters)
        {
            // TODO: shift rounding to reduce bias?
            return (int)((heightInMeters + 0.5F * this.HeightClassSizeInMeters) / this.HeightClassSizeInMeters);
        }
    }
}

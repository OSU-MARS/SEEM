using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Mars.Seem.Tree
{
    public class TreeScaling
    {
        public static TreeScaling Default { get; private set; }
        public static ImmutableArray<FiaCode> MerchantableTreeSpeciesSupported { get; private set; }

        private readonly SortedList<FiaCode, TreeSpeciesMerchantableVolumeTable?> forwarder;
        private readonly float forwarderPreferredLogLengthInM;
        private readonly SortedList<FiaCode, TreeSpeciesMerchantableVolumeTable?> longLog;
        private readonly float longLogPreferredLengthInM;

        static TreeScaling()
        {
            TreeScaling.Default = new(Constant.Bucking.DefaultForwarderLogLengthInM, Constant.Bucking.DefaultLongLogLengthInM);
            TreeScaling.MerchantableTreeSpeciesSupported = ImmutableArray.Create(FiaCode.AlnusRubra, FiaCode.PseudotsugaMenziesii, FiaCode.ThujaPlicata, FiaCode.TsugaHeterophylla);
        }

        public TreeScaling(float forwarderPreferredLogLengthInM, float longLogPreferredLengthInM)
        {
            if (Single.IsNaN(forwarderPreferredLogLengthInM) || (forwarderPreferredLogLengthInM <= 0.0F) || (forwarderPreferredLogLengthInM > 20.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(forwarderPreferredLogLengthInM));
            }
            if (Single.IsNaN(longLogPreferredLengthInM) || (longLogPreferredLengthInM <= 0.0F) || (longLogPreferredLengthInM > 50.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(longLogPreferredLengthInM));
            }
            if (forwarderPreferredLogLengthInM > longLogPreferredLengthInM)
            {
                throw new ArgumentOutOfRangeException(nameof(forwarderPreferredLogLengthInM));
            }

            this.forwarder = new();
            this.forwarderPreferredLogLengthInM = forwarderPreferredLogLengthInM;
            this.longLog = new();
            this.longLogPreferredLengthInM = longLogPreferredLengthInM;
        }

        public float GetMaximumMerchantableDbh(FiaCode treeSpecies)
        {
            float maximumMerchantableDbhInCm = Single.NaN;
            if (this.TryGetForwarderVolumeTable(treeSpecies, out TreeSpeciesMerchantableVolumeTable? forwarderVolumeTable))
            {
                maximumMerchantableDbhInCm = forwarderVolumeTable.MaximumMerchantableDiameterInCentimeters;
            }
            if (this.TryGetLongLogVolumeTable(treeSpecies, out TreeSpeciesMerchantableVolumeTable? longLogVolumeTable))
            {
                if (Single.IsNaN(maximumMerchantableDbhInCm))
                {
                    maximumMerchantableDbhInCm = longLogVolumeTable.MaximumMerchantableDiameterInCentimeters;
                }
                else if (maximumMerchantableDbhInCm != longLogVolumeTable.MaximumMerchantableDiameterInCentimeters)
                {
                    throw new NotSupportedException("Maximum merchantable DBH for " + treeSpecies + " is " + maximumMerchantableDbhInCm + " cm in cut to length harvests and " + longLogVolumeTable.MaximumMerchantableDiameterInCentimeters + " cm in long long harvests. Currently, the maximum merchantable volume must be the same for both log sizes.");
                }
            }

            return maximumMerchantableDbhInCm;
        }

        //public static bool IsMerchantable(FiaCode treeSpecies)
        //{
        //    return TreeScaling.MerchantableTreeSpeciesSupported.Contains(treeSpecies);
        //}

        private static bool TryCreateSpeciesVolumeTable(FiaCode treeSpecies, float preferredLogLengthInM, [NotNullWhen(true)] out TreeSpeciesMerchantableVolumeTable? volumeTable)
        {
            volumeTable = treeSpecies switch
            {
                FiaCode.AlnusRubra => new(treeSpecies, preferredLogLengthInM, RedAlder.GetDiameterInsideBark, RedAlder.GetNeiloidHeight),
                FiaCode.PseudotsugaMenziesii => new(treeSpecies, preferredLogLengthInM, PoudelRegressions.GetDouglasFirDiameterInsideBark, DouglasFir.GetNeiloidHeight),
                FiaCode.ThujaPlicata => new(treeSpecies, preferredLogLengthInM, PoudelRegressions.GetWesternHemlockDiameterInsideBark, WesternHemlock.GetNeiloidHeight),
                FiaCode.TsugaHeterophylla => new(treeSpecies, preferredLogLengthInM, WesternRedcedar.GetDiameterInsideBark, WesternRedcedar.GetNeiloidHeight),
                // species not currently treated as merchantable due to lack of data
                FiaCode.AbiesGrandis or
                FiaCode.AcerMacrophyllum or 
                FiaCode.CalocedrusDecurrens or 
                FiaCode.QuercusGarryana => null,
                // nonmerchantable species supported by Organon
                FiaCode.ArbutusMenziesii or
                FiaCode.ChrysolepisChrysophyllaVarChrysophylla or
                FiaCode.CornusNuttallii or 
                FiaCode.NotholithocarpusDensiflorus or
                FiaCode.Salix or
                FiaCode.TaxusBrevifolia => null,
                _ => throw new NotSupportedException("Unhandled tree species " + treeSpecies + ".")
            };

            return volumeTable != null;
        }

        // thread safe implementation since default volume table is often used concurrently
        public bool TryGetForwarderVolumeTable(FiaCode treeSpecies, [NotNullWhen(true)] out TreeSpeciesMerchantableVolumeTable? forwardedVolumeTable)
        {
            if (this.forwarder.TryGetValue(treeSpecies, out forwardedVolumeTable) == false)
            {
                lock (this.forwarder)
                {
                    if (this.forwarder.TryGetValue(treeSpecies, out forwardedVolumeTable) == false)
                    {
                        TreeScaling.TryCreateSpeciesVolumeTable(treeSpecies, this.forwarderPreferredLogLengthInM, out forwardedVolumeTable);
                        this.forwarder.Add(treeSpecies, forwardedVolumeTable);
                    }
                }
            }

            return forwardedVolumeTable != null;
        }

        // thread safe implementation since default volume table is often used concurrently
        public bool TryGetLongLogVolumeTable(FiaCode treeSpecies, [NotNullWhen(true)] out TreeSpeciesMerchantableVolumeTable? longLogVolumeTable)
        {
            if (this.longLog.TryGetValue(treeSpecies, out longLogVolumeTable) == false)
            {
                lock (this.longLog)
                {
                    if (this.longLog.TryGetValue(treeSpecies, out longLogVolumeTable) == false)
                    {
                        TreeScaling.TryCreateSpeciesVolumeTable(treeSpecies, this.longLogPreferredLengthInM, out longLogVolumeTable);
                        this.longLog.Add(treeSpecies, longLogVolumeTable);
                    }
                }
            }

            return longLogVolumeTable != null;
        }
    }
}
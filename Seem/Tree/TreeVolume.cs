using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Mars.Seem.Tree
{
    public class TreeVolume
    {
        public static TreeVolume Default { get; private set; }
        public static IReadOnlyList<FiaCode> MerchantableTreeSpeciesSupported { get; private set; }

        private readonly SortedList<FiaCode, TreeSpeciesMerchantableVolumeTable?> forwarder;
        private readonly float forwarderPreferredLogLengthInM;
        private readonly SortedList<FiaCode, TreeSpeciesMerchantableVolumeTable?> longLog;
        private readonly float longLogPreferredLengthInM;

        static TreeVolume()
        {
            TreeVolume.Default = new(Constant.Bucking.DefaultForwarderLogLengthInM, Constant.Bucking.DefaultLongLogLengthInM);
            TreeVolume.MerchantableTreeSpeciesSupported = new FiaCode[] { FiaCode.AlnusRubra, FiaCode.PseudotsugaMenziesii, FiaCode.ThujaPlicata, FiaCode.TsugaHeterophylla };
        }

        public TreeVolume(float forwarderPreferredLogLengthInM, float longLogPreferredLengthInM)
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
        public bool TryGetForwarderVolumeTable(FiaCode treeSpecies, [NotNullWhen(true)] out TreeSpeciesMerchantableVolumeTable? forwarderVolumeTable)
        {
            if (this.forwarder.TryGetValue(treeSpecies, out forwarderVolumeTable) == false)
            {
                lock (this.forwarder)
                {
                    if (this.forwarder.TryGetValue(treeSpecies, out forwarderVolumeTable) == false)
                    {
                        TreeVolume.TryCreateSpeciesVolumeTable(treeSpecies, this.forwarderPreferredLogLengthInM, out forwarderVolumeTable);
                        this.forwarder.Add(treeSpecies, forwarderVolumeTable);
                    }
                }
            }

            return forwarderVolumeTable != null;
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
                        TreeVolume.TryCreateSpeciesVolumeTable(treeSpecies, this.longLogPreferredLengthInM, out longLogVolumeTable);
                        this.longLog.Add(treeSpecies, longLogVolumeTable);
                    }
                }
            }

            return longLogVolumeTable != null;
        }
    }
}
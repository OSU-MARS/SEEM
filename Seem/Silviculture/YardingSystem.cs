using Mars.Seem.Tree;
using System;
using System.Diagnostics;

namespace Mars.Seem.Silviculture
{
    public class YardingSystem
    {
        public bool IsYoader { get; init; }
        public float OverweightFirstLogsPerHa { get; private set; } // logs/ha
        public float ProcessorPMhPerHa { get; private set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float ProcessorProductivity { get; private set; } // m³/PMh₀
        public float YarderPMhPerHectare { get; private set; } // accumulated in delay free seconds/ha and then converted to PMh₀/ha
        public float YarderProductivity { get; private set; } // m³/PMh₀
        public float YarderSMhPerHectare { get; private set; } // SMh/ha

        public YardingSystem()
        {
            this.IsYoader = false;

            this.Clear();
        }

        public void AddOverweightFirstLog(float overweightLogsPerHa)
        {
            this.OverweightFirstLogsPerHa += overweightLogsPerHa;
        }

        public void AddTreeProcessing(float treeProcessingPMsPerHa)
        {
            this.ProcessorPMhPerHa += treeProcessingPMsPerHa;
        }
        
        public void CalculatePMhAndProductivity(Stand stand, float merchantableCubicVolumePerHa, HarvestSystems harvestSystems, float yardedWeightPerHa)
        {
            float meanGrappleYardingTurnTimeInS = harvestSystems.GrappleYardingConstant + harvestSystems.GrappleYardingLinear * (stand.MeanYardingDistanceFactor * stand.CorridorLengthInM + stand.AccessDistanceInM);
            float meanGrappleYardingPayload;
            float meanGrappleYarderUtilization;
            if (this.IsYoader)
            {
                meanGrappleYardingPayload = harvestSystems.GrappleYoaderMeanPayload;
                meanGrappleYarderUtilization = harvestSystems.GrappleYoaderUtilization;
            }
            else
            {
                meanGrappleYardingPayload = harvestSystems.GrappleSwingYarderMeanPayload;
                meanGrappleYarderUtilization = harvestSystems.GrappleSwingYarderUtilization;
            }

            float grappleSwingYarderTurnsPerHectare = yardedWeightPerHa / meanGrappleYardingPayload;
            this.YarderPMhPerHectare = grappleSwingYarderTurnsPerHectare * meanGrappleYardingTurnTimeInS / Constant.SecondsPerHour;
            this.YarderSMhPerHectare = this.YarderPMhPerHectare / meanGrappleYarderUtilization;
            this.YarderProductivity = merchantableCubicVolumePerHa / this.YarderPMhPerHectare;

            this.ProcessorPMhPerHa /= Constant.SecondsPerHour;
            this.ProcessorProductivity = merchantableCubicVolumePerHa / this.ProcessorPMhPerHa;
            Debug.Assert((this.YarderProductivity >= 0.0F) && (this.YarderProductivity < 1000.0F) &&
                         (this.ProcessorProductivity >= 0.0F) && (this.ProcessorProductivity < 1000.0F));
        }

        public void Clear()
        {
            this.OverweightFirstLogsPerHa = 0.0F;
            this.ProcessorPMhPerHa = 0.0F;
            this.ProcessorProductivity = Single.NaN;
            this.YarderPMhPerHectare = 0.0F;
            this.YarderProductivity = Single.NaN;
            this.YarderSMhPerHectare = 0.0F;
        }
    }
}

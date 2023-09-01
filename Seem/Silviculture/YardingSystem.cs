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
        
        public void CalculatePMhAndProductivity(Stand stand, bool isThin, float merchantableCubicVolumePerHa, HarvestSystems harvestSystems, float yardedWeightPerHa)
        {
            Debug.Assert((merchantableCubicVolumePerHa == 0.0F) || ((yardedWeightPerHa > 0.0F) || (merchantableCubicVolumePerHa < 100.0F)), "Stand " + stand.Name + ": merchantable volume is " + merchantableCubicVolumePerHa + " m³/ha but yarded weight is " + yardedWeightPerHa + " kg/ha.");

            float averageYardingDistance = stand.MeanYardingDistanceFactor * stand.CorridorLengthInM + stand.AccessDistanceInM;
            float meanGrappleYardingTurnTimeInS;
            if (isThin)
            {
                meanGrappleYardingTurnTimeInS = harvestSystems.GrappleYardingConstantThin + harvestSystems.GrappleYardingLinearThin * averageYardingDistance;
            }
            else
            {
                meanGrappleYardingTurnTimeInS = harvestSystems.GrappleYardingConstantRegen + harvestSystems.GrappleYardingLinearRegen * averageYardingDistance;
            }
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

            float grappleTurnsPerHa = yardedWeightPerHa / meanGrappleYardingPayload;
            this.YarderPMhPerHectare = grappleTurnsPerHa * meanGrappleYardingTurnTimeInS / Constant.SecondsPerHour;
            this.YarderSMhPerHectare = this.YarderPMhPerHectare / meanGrappleYarderUtilization;
            if (this.YarderPMhPerHectare > 0.0F)
            {
                this.YarderProductivity = merchantableCubicVolumePerHa / this.YarderPMhPerHectare;
            }
            else
            {
                Debug.Assert(this.YarderPMhPerHectare == 0.0F);
                this.YarderProductivity = 0.0F;
            }

            this.ProcessorPMhPerHa /= Constant.SecondsPerHour;
            if (this.ProcessorPMhPerHa > 0.0F)
            {
                this.ProcessorProductivity = merchantableCubicVolumePerHa / this.ProcessorPMhPerHa;
            }
            else
            {
                Debug.Assert(this.ProcessorPMhPerHa == 0.0F);
                this.ProcessorProductivity = 0.0F;
            }
            Debug.Assert((this.YarderProductivity >= 0.0F) && (this.YarderProductivity < 15000.0F), "Yarder productivity out of range.");
            Debug.Assert((this.ProcessorProductivity >= 0.0F) && (this.ProcessorProductivity < 15000.0F), "Processor productivity out of range.");
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

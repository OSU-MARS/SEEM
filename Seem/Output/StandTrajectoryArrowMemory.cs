using Apache.Arrow;
using Apache.Arrow.Types;
using Mars.Seem.Extensions;
using Mars.Seem.Optimization;
using Mars.Seem.Silviculture;
using Mars.Seem.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Mars.Seem.Output
{
    public class StandTrajectoryArrowMemory : ArrowMemory
    {
        private byte[]? stand;
        private byte[]? thin1;
        private byte[]? thin2;
        private byte[]? thin3;
        private byte[]? rotation;
        private byte[]? financialScenario;
        private byte[]? year;
        private byte[]? standAge;
        // tree growth
        private byte[]? tph;
        private byte[]? qmd;
        private byte[]? hTop;
        private byte[]? basalArea;
        private byte[]? reinekeSdi;
        private byte[]? standingCmh;
        private byte[]? standingMbfh;
        private byte[]? thinCmh;
        private byte[]? thinMbfh;
        private byte[]? baRemoved;
        private byte[]? baIntensity;
        private byte[]? tphDecrease;
            // financial
        private byte[]? npv;
        private byte[]? lev;
        // carbon - not supported for now
        //new("liveTreeBiomass;
        //new("SPH;
        //new("snagQMD;
        // harvest cost
        private byte[]? thinMinCostSystem;
        private byte[]? thinFallerGrappleSwingYarderCost;
        private byte[]? thinFallerGrappleYoaderCost;
        private byte[]? thinFellerBuncherGrappleSwingYarderCost;
        private byte[]? thinFellerBuncherGrappleYoaderCost;
        private byte[]? thinTrackedHarvesterForwarderCost;
        private byte[]? thinTrackedHarvesterGrappleSwingYarderCost;
        private byte[]? thinTrackedHarvesterGrappleYoaderCost;
        private byte[]? thinWheeledHarvesterForwarderCost;
        private byte[]? thinWheeledHarvesterGrappleSwingYarderCost;
        private byte[]? thinWheeledHarvesterGrappleYoaderCost;
        private byte[]? thinTaskCost;
        private byte[]? regenMinCostSystem;
        private byte[]? regenFallerGrappleSwingYarderCost;
        private byte[]? regenFallerGrappleYoaderCost;
        private byte[]? regenFellerBuncherGrappleSwingYarderCost;
        private byte[]? regenFellerBuncherGrappleYoaderCost;
        private byte[]? regenTrackedHarvesterGrappleSwingYarderCost;
        private byte[]? regenTrackedHarvesterGrappleYoaderCost;
        private byte[]? regenWheeledHarvesterGrappleSwingYarderCost;
        private byte[]? regenWheeledHarvesterGrappleYoaderCost;
        private byte[]? regenTaskCost;
        private byte[]? reforestationNpv;
        // timber sorts
        private byte[]? thinLogs2S;
        private byte[]? thinLogs3S;
        private byte[]? thinLogs4S;
        private byte[]? thinCmh2S;
        private byte[]? thinCmh3S;
        private byte[]? thinCmh4S;
        private byte[]? thinMbfh2S;
        private byte[]? thinMbfh3S;
        private byte[]? thinMbfh4S;
        private byte[]? thinPond2S;
        private byte[]? thinPond3S;
        private byte[]? thinPond4S;
        private byte[]? standingLogs2S;
        private byte[]? standingLogs3S;
        private byte[]? standingLogs4S;
        private byte[]? standingCmh2S;
        private byte[]? standingCmh3S;
        private byte[]? standingCmh4S;
        private byte[]? standingMbfh2S;
        private byte[]? standingMbfh3S;
        private byte[]? standingMbfh4S;
        private byte[]? regenPond2S;
        private byte[]? regenPond3S;
        private byte[]? regenPond4S;
        // equipment productivity
        private byte[]? thinFallerPMh;
        private byte[]? thinFallerProductivity;
        private byte[]? thinFellerBuncherPMh;
        private byte[]? thinFellerBuncherProductivity;
        private byte[]? thinTrackedHarvesterPMh;
        private byte[]? thinTrackedHarvesterProductivity;
        private byte[]? thinWheeledHarvesterPMh;
        private byte[]? thinWheeledHarvesterProductivity;
        private byte[]? thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder;
        private byte[]? thinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder;
        private byte[]? thinChainsawCmhWithFellerBuncherAndGrappleSwingYarder;
        private byte[]? thinChainsawPMhWithFellerBuncherAndGrappleSwingYarder;
        private byte[]? thinChainsawCrewWithFellerBuncherAndGrappleYoader;
        private byte[]? thinChainsawUtilizationWithFellerBuncherAndGrappleYoader;
        private byte[]? thinChainsawCmhWithFellerBuncherAndGrappleYoader;
        private byte[]? thinChainsawPMhWithFellerBuncherAndGrappleYoader;
        private byte[]? thinChainsawCrewWithTrackedHarvester;
        private byte[]? thinChainsawUtilizationWithTrackedHarvester;
        private byte[]? thinChainsawCmhWithTrackedHarvester;
        private byte[]? thinChainsawPMhWithTrackedHarvester;
        private byte[]? thinChainsawCrewWithWheeledHarvester;
        private byte[]? thinChainsawUtilizationWithWheeledHarvester;
        private byte[]? thinChainsawCmhWithWheeledHarvester;
        private byte[]? thinChainsawPMhWithWheeledHarvester;
        private byte[]? thinForwardingMethod;
        private byte[]? thinForwarderPMh;
        private byte[]? thinForwarderProductivity;
        private byte[]? thinForwardedWeight;
        private byte[]? thinGrappleSwingYarderPMhPerHectare;
        private byte[]? thinGrappleSwingYarderProductivity;
        private byte[]? thinGrappleSwingYarderOverweightFirstLogsPerHectare;
        private byte[]? thinGrappleYoaderPMhPerHectare;
        private byte[]? thinGrappleYoaderProductivity;
        private byte[]? thinGrappleYoaderOverweightFirstLogsPerHectare;
        private byte[]? thinProcessorPMhWithGrappleSwingYarder;
        private byte[]? thinProcessorProductivityWithGrappleSwingYarder;
        private byte[]? thinProcessorPMhWithGrappleYoader;
        private byte[]? thinProcessorProductivityWithGrappleYoader;
        private byte[]? thinLoadedWeight;
        private byte[]? regenFallerPMh;
        private byte[]? regenFallerProductivity;
        private byte[]? regenFellerBuncherPMh;
        private byte[]? regenFellerBuncherProductivity;
        private byte[]? regenTrackedHarvesterPMh;
        private byte[]? regenTrackedHarvesterProductivity;
        private byte[]? regenWheeledHarvesterPMh;
        private byte[]? regenWheeledHarvesterProductivity;
        private byte[]? regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder;
        private byte[]? regenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder;
        private byte[]? regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder;
        private byte[]? regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder;
        private byte[]? regenChainsawCrewWithFellerBuncherAndGrappleYoader;
        private byte[]? regenChainsawUtilizationWithFellerBuncherAndGrappleYoader;
        private byte[]? regenChainsawCmhWithFellerBuncherAndGrappleYoader;
        private byte[]? regenChainsawPMhWithFellerBuncherAndGrappleYoader;
        private byte[]? regenChainsawCrewWithTrackedHarvester;
        private byte[]? regenChainsawUtilizationWithTrackedHarvester;
        private byte[]? regenChainsawCmhWithTrackedHarvester;
        private byte[]? regenChainsawPMhWithTrackedHarvester;
        private byte[]? regenChainsawCrewWithWheeledHarvester;
        private byte[]? regenChainsawUtilizationWithWheeledHarvester;
        private byte[]? regenChainsawCmhWithWheeledHarvester;
        private byte[]? regenChainsawPMhWithWheeledHarvester;
        private byte[]? regenGrappleSwingYarderPMhPerHectare;
        private byte[]? regenGrappleSwingYarderProductivity;
        private byte[]? regenGrappleSwingYarderOverweightFirstLogsPerHectare;
        private byte[]? regenGrappleYoaderPMhPerHectare;
        private byte[]? regenGrappleYoaderProductivity;
        private byte[]? regenGrappleYoaderOverweightFirstLogsPerHectare;
        private byte[]? regenProcessorPMhWithGrappleSwingYarder;
        private byte[]? regenProcessorProductivityWithGrappleSwingYarder;
        private byte[]? regenProcessorPMhWithGrappleYoader;
        private byte[]? regenProcessorProductivityWithGrappleYoader;
        private byte[]? regenLoadedWeight;

        public StandTrajectoryArrowMemory(int capacityInRecords)
            : base(StandTrajectoryArrowMemory.CreateSchema(), capacityInRecords, 4 * 1000 * 1000) // 531 bytes / record * 400k records = 1.98 GB, https://github.com/apache/arrow/issues/37069
        {
            this.stand = null;
            this.thin1 = null;
            this.thin2 = null;
            this.thin3 = null;
            this.rotation = null;
            this.financialScenario = null;
            this.year = null;
            this.standAge = null;

            // tree growth
            this.tph = null;
            this.qmd = null;
            this.hTop = null;
            this.basalArea = null;
            this.reinekeSdi = null;
            this.standingCmh = null;
            this.standingMbfh = null;
            this.thinCmh = null;
            this.thinMbfh = null;
            this.baRemoved = null;
            this.baIntensity = null;
            this.tphDecrease = null;

            // financial
            this.npv = null;
            this.lev = null;
            // carbon - not supported for now
            //new("liveTreeBiomass = null;
            //new("SPH = null;
            //new("snagQMD = null;

            // harvest cost
            this.thinMinCostSystem = null;
            this.thinFallerGrappleSwingYarderCost = null;
            this.thinFallerGrappleYoaderCost = null;
            this.thinFellerBuncherGrappleSwingYarderCost = null;
            this.thinFellerBuncherGrappleYoaderCost = null;
            this.thinTrackedHarvesterForwarderCost = null;
            this.thinTrackedHarvesterGrappleSwingYarderCost = null;
            this.thinTrackedHarvesterGrappleYoaderCost = null;
            this.thinWheeledHarvesterForwarderCost = null;
            this.thinWheeledHarvesterGrappleSwingYarderCost = null;
            this.thinWheeledHarvesterGrappleYoaderCost = null;
            this.thinTaskCost = null;
            this.regenMinCostSystem = null;
            this.regenFallerGrappleSwingYarderCost = null;
            this.regenFallerGrappleYoaderCost = null;
            this.regenFellerBuncherGrappleSwingYarderCost = null;
            this.regenFellerBuncherGrappleYoaderCost = null;
            this.regenTrackedHarvesterGrappleSwingYarderCost = null;
            this.regenTrackedHarvesterGrappleYoaderCost = null;
            this.regenWheeledHarvesterGrappleSwingYarderCost = null;
            this.regenWheeledHarvesterGrappleYoaderCost = null;
            this.regenTaskCost = null;
            this.reforestationNpv = null;

            // timber sorts
            this.thinLogs2S = null;
            this.thinLogs3S = null;
            this.thinLogs4S = null;
            this.thinCmh2S = null;
            this.thinCmh3S = null;
            this.thinCmh4S = null;
            this.thinMbfh2S = null;
            this.thinMbfh3S = null;
            this.thinMbfh4S = null;
            this.thinPond2S = null;
            this.thinPond3S = null;
            this.thinPond4S = null;
            this.standingLogs2S = null;
            this.standingLogs3S = null;
            this.standingLogs4S = null;
            this.standingCmh2S = null;
            this.standingCmh3S = null;
            this.standingCmh4S = null;
            this.standingMbfh2S = null;
            this.standingMbfh3S = null;
            this.standingMbfh4S = null;
            this.regenPond2S = null;
            this.regenPond3S = null;
            this.regenPond4S = null;

            // equipment productivity
            this.thinFallerPMh = null;
            this.thinFallerProductivity = null;
            this.thinFellerBuncherPMh = null;
            this.thinFellerBuncherProductivity = null;
            this.thinTrackedHarvesterPMh = null;
            this.thinTrackedHarvesterProductivity = null;
            this.thinWheeledHarvesterPMh = null;
            this.thinWheeledHarvesterProductivity = null;
            this.thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder = null;
            this.thinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = null;
            this.thinChainsawCmhWithFellerBuncherAndGrappleSwingYarder = null;
            this.thinChainsawPMhWithFellerBuncherAndGrappleSwingYarder = null;
            this.thinChainsawCrewWithFellerBuncherAndGrappleYoader = null;
            this.thinChainsawUtilizationWithFellerBuncherAndGrappleYoader = null;
            this.thinChainsawCmhWithFellerBuncherAndGrappleYoader = null;
            this.thinChainsawPMhWithFellerBuncherAndGrappleYoader = null;
            this.thinChainsawCrewWithTrackedHarvester = null;
            this.thinChainsawUtilizationWithTrackedHarvester = null;
            this.thinChainsawCmhWithTrackedHarvester = null;
            this.thinChainsawPMhWithTrackedHarvester = null;
            this.thinChainsawCrewWithWheeledHarvester = null;
            this.thinChainsawUtilizationWithWheeledHarvester = null;
            this.thinChainsawCmhWithWheeledHarvester = null;
            this.thinChainsawPMhWithWheeledHarvester = null;
            this.thinForwardingMethod = null;
            this.thinForwarderPMh = null;
            this.thinForwarderProductivity = null;
            this.thinForwardedWeight = null;
            this.thinGrappleSwingYarderPMhPerHectare = null;
            this.thinGrappleSwingYarderProductivity = null;
            this.thinGrappleSwingYarderOverweightFirstLogsPerHectare = null;
            this.thinGrappleYoaderPMhPerHectare = null;
            this.thinGrappleYoaderProductivity = null;
            this.thinGrappleYoaderOverweightFirstLogsPerHectare = null;
            this.thinProcessorPMhWithGrappleSwingYarder = null;
            this.thinProcessorProductivityWithGrappleSwingYarder = null;
            this.thinProcessorPMhWithGrappleYoader = null;
            this.thinProcessorProductivityWithGrappleYoader = null;
            this.thinLoadedWeight = null;
            this.regenFallerPMh = null;
            this.regenFallerProductivity = null;
            this.regenFellerBuncherPMh = null;
            this.regenFellerBuncherProductivity = null;
            this.regenTrackedHarvesterPMh = null;
            this.regenTrackedHarvesterProductivity = null;
            this.regenWheeledHarvesterPMh = null;
            this.regenWheeledHarvesterProductivity = null;
            this.regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder = null;
            this.regenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = null;
            this.regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder = null;
            this.regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder = null;
            this.regenChainsawCrewWithFellerBuncherAndGrappleYoader = null;
            this.regenChainsawUtilizationWithFellerBuncherAndGrappleYoader = null;
            this.regenChainsawCmhWithFellerBuncherAndGrappleYoader = null;
            this.regenChainsawPMhWithFellerBuncherAndGrappleYoader = null;
            this.regenChainsawCrewWithTrackedHarvester = null;
            this.regenChainsawUtilizationWithTrackedHarvester = null;
            this.regenChainsawCmhWithTrackedHarvester = null;
            this.regenChainsawPMhWithTrackedHarvester = null;
            this.regenChainsawCrewWithWheeledHarvester = null;
            this.regenChainsawUtilizationWithWheeledHarvester = null;
            this.regenChainsawCmhWithWheeledHarvester = null;
            this.regenChainsawPMhWithWheeledHarvester = null;
            this.regenGrappleSwingYarderPMhPerHectare = null;
            this.regenGrappleSwingYarderProductivity = null;
            this.regenGrappleSwingYarderOverweightFirstLogsPerHectare = null;
            this.regenGrappleYoaderPMhPerHectare = null;
            this.regenGrappleYoaderProductivity = null;
            this.regenGrappleYoaderOverweightFirstLogsPerHectare = null;
            this.regenProcessorPMhWithGrappleSwingYarder = null;
            this.regenProcessorProductivityWithGrappleSwingYarder = null;
            this.regenProcessorPMhWithGrappleYoader = null;
            this.regenProcessorProductivityWithGrappleYoader = null;
            this.regenLoadedWeight = null;
        }

        private void Add(StandTrajectory trajectory, int startPeriod, WriteStandTrajectoryContext writeContext, int startIndexInRecordBatch, int periodsToCopy)
        {
            if (UInt32.TryParse(trajectory.Name, out UInt32 standID) == false)
            {
                throw new NotSupportedException("Stand trajectory name '" + trajectory.Name + "' could not be converted to an unsigned 32 bit integer. For the moment, trajectory names are required to be stand IDs.");
            }

            Span<UInt32> batchStand = MemoryMarshal.Cast<byte, UInt32>(this.stand);
            Span<Int16> batchThin1 = MemoryMarshal.Cast<byte, Int16>(this.thin1);
            Span<Int16> batchThin2 = MemoryMarshal.Cast<byte, Int16>(this.thin2);
            Span<Int16> batchThin3 = MemoryMarshal.Cast<byte, Int16>(this.thin3);
            Span<Int16> batchRotation = MemoryMarshal.Cast<byte, Int16>(this.rotation);
            Span<UInt32> batchFinancialScenario = MemoryMarshal.Cast<byte, UInt32>(this.financialScenario);
            Span<Int16> batchYear = MemoryMarshal.Cast<byte, Int16>(this.year);
            Span<Int16> batchStandAge = MemoryMarshal.Cast<byte, Int16>(this.standAge);
            // tree growth
            Span<float> batchTph = MemoryMarshal.Cast<byte, float>(this.tph);
            Span<float> batchQmd = MemoryMarshal.Cast<byte, float>(this.qmd);
            Span<float> batchHtop = MemoryMarshal.Cast<byte, float>(this.hTop);
            Span<float> batchBasalArea = MemoryMarshal.Cast<byte, float>(this.basalArea);
            Span<float> batchSdi = MemoryMarshal.Cast<byte, float>(this.reinekeSdi);
            Span<float> batchStandingCmh = MemoryMarshal.Cast<byte, float>(this.standingCmh);
            Span<float> batchStandingMbfh = MemoryMarshal.Cast<byte, float>(this.standingMbfh);
            Span<float> batchThinCmh = MemoryMarshal.Cast<byte, float>(this.thinCmh);
            Span<float> batchThinMbfh = MemoryMarshal.Cast<byte, float>(this.thinMbfh);
            Span<float> batchBAremoved = MemoryMarshal.Cast<byte, float>(this.baRemoved);
            Span<float> batchBAintensity = MemoryMarshal.Cast<byte, float>(this.baIntensity);
            Span<float> batchTPHdecrease = MemoryMarshal.Cast<byte, float>(this.tphDecrease);
            // financial
            Span<float> batchNpv = MemoryMarshal.Cast<byte, float>(this.npv);
            Span<float> batchLev = MemoryMarshal.Cast<byte, float>(this.lev);
            // carbon - not supported for now
            // Span<float> batchLiveTreeBiomass = MemoryMarshal.Cast<byte, float>(this.LiveTreeBiomass);
            // Span<float> batchSph = MemoryMarshal.Cast<byte, float>(this.Sph);
            // Span<float> batchQ = MemoryMarshal.Cast<byte, float>(this.Q);
            // harvest cost
            Span<HarvestSystemEquipment> batchThinMinCostSystem = MemoryMarshal.Cast<byte, HarvestSystemEquipment>(this.thinMinCostSystem);
            Span<float> batchThinFallerGrappleSwingYarderCost = MemoryMarshal.Cast<byte, float>(this.thinFallerGrappleSwingYarderCost);
            Span<float> batchThinFallerGrappleYoaderCost = MemoryMarshal.Cast<byte, float>(this.thinFallerGrappleYoaderCost);
            Span<float> batchThinFellerBuncherGrappleSwingYarderCost = MemoryMarshal.Cast<byte, float>(this.thinFellerBuncherGrappleSwingYarderCost);
            Span<float> batchThinFellerBuncherGrappleYoaderCost = MemoryMarshal.Cast<byte, float>(this.thinFellerBuncherGrappleYoaderCost);
            Span<float> batchThinTrackedHarvesterForwarderCost = MemoryMarshal.Cast<byte, float>(this.thinTrackedHarvesterForwarderCost);
            Span<float> batchThinTrackedHarvesterGrappleSwingYarderCost = MemoryMarshal.Cast<byte, float>(this.thinTrackedHarvesterGrappleSwingYarderCost);
            Span<float> batchThinTrackedHarvesterGrappleYoaderCost = MemoryMarshal.Cast<byte, float>(this.thinTrackedHarvesterGrappleYoaderCost);
            Span<float> batchThinWheeledHarvesterForwarderCost = MemoryMarshal.Cast<byte, float>(this.thinWheeledHarvesterForwarderCost);
            Span<float> batchThinWheeledHarvesterGrappleSwingYarderCost = MemoryMarshal.Cast<byte, float>(this.thinWheeledHarvesterGrappleSwingYarderCost);
            Span<float> batchThinWheeledHarvesterGrappleYoaderCost = MemoryMarshal.Cast<byte, float>(this.thinWheeledHarvesterGrappleYoaderCost);
            Span<float> batchThinTaskCost = MemoryMarshal.Cast<byte, float>(this.thinTaskCost);
            Span<HarvestSystemEquipment> batchRegenMinCostSystem = MemoryMarshal.Cast<byte, HarvestSystemEquipment>(this.regenMinCostSystem);
            Span<float> batchRegenFallerGrappleSwingYarderCost = MemoryMarshal.Cast<byte, float>(this.regenFallerGrappleSwingYarderCost);
            Span<float> batchRegenFallerGrappleYoaderCost = MemoryMarshal.Cast<byte, float>(this.regenFallerGrappleYoaderCost);
            Span<float> batchRegenFellerBuncherGrappleSwingYarderCost = MemoryMarshal.Cast<byte, float>(this.regenFellerBuncherGrappleSwingYarderCost);
            Span<float> batchRegenFellerBuncherGrappleYoaderCost = MemoryMarshal.Cast<byte, float>(this.regenFellerBuncherGrappleYoaderCost);
            Span<float> batchRegenTrackedHarvesterGrappleSwingYarderCost = MemoryMarshal.Cast<byte, float>(this.regenTrackedHarvesterGrappleSwingYarderCost);
            Span<float> batchRegenTrackedHarvesterGrappleYoaderCost = MemoryMarshal.Cast<byte, float>(this.regenTrackedHarvesterGrappleYoaderCost);
            Span<float> batchRegenWheeledHarvesterGrappleSwingYarderCost = MemoryMarshal.Cast<byte, float>(this.regenWheeledHarvesterGrappleSwingYarderCost);
            Span<float> batchRegenWheeledHarvesterGrappleYoaderCost = MemoryMarshal.Cast<byte, float>(this.regenWheeledHarvesterGrappleYoaderCost);
            Span<float> batchRegenTaskCost = MemoryMarshal.Cast<byte, float>(this.regenTaskCost);
            Span<float> batchReforestationNpv = MemoryMarshal.Cast<byte, float>(this.reforestationNpv);
            // timber sorts
            Span<float> batchThinLogs2S = MemoryMarshal.Cast<byte, float>(this.thinLogs2S);
            Span<float> batchThinLogs3S = MemoryMarshal.Cast<byte, float>(this.thinLogs3S);
            Span<float> batchThinLogs4S = MemoryMarshal.Cast<byte, float>(this.thinLogs4S);
            Span<float> batchThinCmh2S = MemoryMarshal.Cast<byte, float>(this.thinCmh2S);
            Span<float> batchThinCmh3S = MemoryMarshal.Cast<byte, float>(this.thinCmh3S);
            Span<float> batchThinCmh4S = MemoryMarshal.Cast<byte, float>(this.thinCmh4S);
            Span<float> batchThinMbfh2S = MemoryMarshal.Cast<byte, float>(this.thinMbfh2S);
            Span<float> batchThinMbfh3S = MemoryMarshal.Cast<byte, float>(this.thinMbfh3S);
            Span<float> batchThinMbfh4S = MemoryMarshal.Cast<byte, float>(this.thinMbfh4S);
            Span<float> batchThinPond2S = MemoryMarshal.Cast<byte, float>(this.thinPond2S);
            Span<float> batchThinPond3S = MemoryMarshal.Cast<byte, float>(this.thinPond3S);
            Span<float> batchThinPond4S = MemoryMarshal.Cast<byte, float>(this.thinPond4S);
            Span<float> batchStandingLogs2S = MemoryMarshal.Cast<byte, float>(this.standingLogs2S);
            Span<float> batchStandingLogs3S = MemoryMarshal.Cast<byte, float>(this.standingLogs3S);
            Span<float> batchStandingLogs4S = MemoryMarshal.Cast<byte, float>(this.standingLogs4S);
            Span<float> batchStandingCmh2S = MemoryMarshal.Cast<byte, float>(this.standingCmh2S);
            Span<float> batchStandingCmh3S = MemoryMarshal.Cast<byte, float>(this.standingCmh3S);
            Span<float> batchStandingCmh4S = MemoryMarshal.Cast<byte, float>(this.standingCmh4S);
            Span<float> batchStandingMbfh2S = MemoryMarshal.Cast<byte, float>(this.standingMbfh2S);
            Span<float> batchStandingMbfh3S = MemoryMarshal.Cast<byte, float>(this.standingMbfh3S);
            Span<float> batchStandingMbfh4S = MemoryMarshal.Cast<byte, float>(this.standingMbfh4S);
            Span<float> batchRegenPond2S = MemoryMarshal.Cast<byte, float>(this.regenPond2S);
            Span<float> batchRegenPond3S = MemoryMarshal.Cast<byte, float>(this.regenPond3S);
            Span<float> batchRegenPond4S = MemoryMarshal.Cast<byte, float>(this.regenPond4S);
            // equipment productivity
            Span<float> batchThinFallerPMh = MemoryMarshal.Cast<byte, float>(this.thinFallerPMh);
            Span<float> batchThinFallerProductivity = MemoryMarshal.Cast<byte, float>(this.thinFallerProductivity);
            Span<float> batchThinFellerBuncherPMh = MemoryMarshal.Cast<byte, float>(this.thinFellerBuncherPMh);
            Span<float> batchThinFellerBuncherProductivity = MemoryMarshal.Cast<byte, float>(this.thinFellerBuncherProductivity);
            Span<float> batchThinTrackedHarvesterPMh = MemoryMarshal.Cast<byte, float>(this.thinTrackedHarvesterPMh);
            Span<float> batchThinTrackedHarvesterProductivity = MemoryMarshal.Cast<byte, float>(this.thinTrackedHarvesterProductivity);
            Span<float> batchThinWheeledHarvesterPMh = MemoryMarshal.Cast<byte, float>(this.thinWheeledHarvesterPMh);
            Span<float> batchThinWheeledHarvesterProductivity = MemoryMarshal.Cast<byte, float>(this.thinWheeledHarvesterProductivity);
            Span<ChainsawCrewType> batchThinChainsawCrewWithFellerBuncherAndGrappleSwingYarder = MemoryMarshal.Cast<byte, ChainsawCrewType>(this.thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder);
            Span<float> batchThinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = MemoryMarshal.Cast<byte, float>(this.thinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder);
            Span<float> batchThinChainsawCmhWithFellerBuncherAndGrappleSwingYarder = MemoryMarshal.Cast<byte, float>(this.thinChainsawCmhWithFellerBuncherAndGrappleSwingYarder);
            Span<float> batchThinChainsawPMhWithFellerBuncherAndGrappleSwingYarder = MemoryMarshal.Cast<byte, float>(this.thinChainsawPMhWithFellerBuncherAndGrappleSwingYarder);
            Span<ChainsawCrewType> batchThinChainsawCrewWithFellerBuncherAndGrappleYoader = MemoryMarshal.Cast<byte, ChainsawCrewType>(this.thinChainsawCrewWithFellerBuncherAndGrappleYoader);
            Span<float> batchThinChainsawUtilizationWithFellerBuncherAndGrappleYoader = MemoryMarshal.Cast<byte, float>(this.thinChainsawUtilizationWithFellerBuncherAndGrappleYoader);
            Span<float> batchThinChainsawCmhWithFellerBuncherAndGrappleYoader = MemoryMarshal.Cast<byte, float>(this.thinChainsawCmhWithFellerBuncherAndGrappleYoader);
            Span<float> batchThinChainsawPMhWithFellerBuncherAndGrappleYoader = MemoryMarshal.Cast<byte, float>(this.thinChainsawPMhWithFellerBuncherAndGrappleYoader);
            Span<ChainsawCrewType> batchThinChainsawCrewWithTrackedHarvester = MemoryMarshal.Cast<byte, ChainsawCrewType>(this.thinChainsawCrewWithTrackedHarvester);
            Span<float> batchThinChainsawUtilizationWithTrackedHarvester = MemoryMarshal.Cast<byte, float>(this.thinChainsawUtilizationWithTrackedHarvester);
            Span<float> batchThinChainsawCmhWithTrackedHarvester = MemoryMarshal.Cast<byte, float>(this.thinChainsawCmhWithTrackedHarvester);
            Span<float> batchThinChainsawPMhWithTrackedHarvester = MemoryMarshal.Cast<byte, float>(this.thinChainsawPMhWithTrackedHarvester);
            Span<ChainsawCrewType> batchThinChainsawCrewWithWheeledHarvester = MemoryMarshal.Cast<byte, ChainsawCrewType>(this.thinChainsawCrewWithWheeledHarvester);
            Span<float> batchThinChainsawUtilizationWithWheeledHarvester = MemoryMarshal.Cast<byte, float>(this.thinChainsawUtilizationWithWheeledHarvester);
            Span<float> batchThinChainsawCmhWithWheeledHarvester = MemoryMarshal.Cast<byte, float>(this.thinChainsawCmhWithWheeledHarvester);
            Span<float> batchThinChainsawPMhWithWheeledHarvester = MemoryMarshal.Cast<byte, float>(this.thinChainsawPMhWithWheeledHarvester);
            Span<ForwarderLoadingMethod> batchThinForwardingMethod = MemoryMarshal.Cast<byte, ForwarderLoadingMethod>(this.thinForwardingMethod);
            Span<float> batchThinForwarderPMh = MemoryMarshal.Cast<byte, float>(this.thinForwarderPMh);
            Span<float> batchThinForwarderProductivity = MemoryMarshal.Cast<byte, float>(this.thinForwarderProductivity);
            Span<float> batchThinForwardedWeight = MemoryMarshal.Cast<byte, float>(this.thinForwardedWeight);
            Span<float> batchThinGrappleSwingYarderPMhPerHectare = MemoryMarshal.Cast<byte, float>(this.thinGrappleSwingYarderPMhPerHectare);
            Span<float> batchThinGrappleSwingYarderProductivity = MemoryMarshal.Cast<byte, float>(this.thinGrappleSwingYarderProductivity);
            Span<float> batchThinGrappleSwingYarderOverweightFirstLogsPerHectare = MemoryMarshal.Cast<byte, float>(this.thinGrappleSwingYarderOverweightFirstLogsPerHectare);
            Span<float> batchThinGrappleYoaderPMhPerHectare = MemoryMarshal.Cast<byte, float>(this.thinGrappleYoaderPMhPerHectare);
            Span<float> batchThinGrappleYoaderProductivity = MemoryMarshal.Cast<byte, float>(this.thinGrappleYoaderProductivity);
            Span<float> batchThinGrappleYoaderOverweightFirstLogsPerHectare = MemoryMarshal.Cast<byte, float>(this.thinGrappleYoaderOverweightFirstLogsPerHectare);
            Span<float> batchThinProcessorPMhWithGrappleSwingYarder = MemoryMarshal.Cast<byte, float>(this.thinProcessorPMhWithGrappleSwingYarder);
            Span<float> batchThinProcessorProductivityWithGrappleSwingYarder = MemoryMarshal.Cast<byte, float>(this.thinProcessorProductivityWithGrappleSwingYarder);
            Span<float> batchThinProcessorPMhWithGrappleYoader = MemoryMarshal.Cast<byte, float>(this.thinProcessorPMhWithGrappleYoader);
            Span<float> batchThinProcessorProductivityWithGrappleYoader = MemoryMarshal.Cast<byte, float>(this.thinProcessorProductivityWithGrappleYoader);
            Span<float> batchThinLoadedWeight = MemoryMarshal.Cast<byte, float>(this.thinLoadedWeight);
            
            Span<float> batchRegenFallerPMh = MemoryMarshal.Cast<byte, float>(this.regenFallerPMh);
            Span<float> batchRegenFallerProductivity = MemoryMarshal.Cast<byte, float>(this.regenFallerProductivity);
            Span<float> batchRegenFellerBuncherPMh = MemoryMarshal.Cast<byte, float>(this.regenFellerBuncherPMh);
            Span<float> batchRegenFellerBuncherProductivity = MemoryMarshal.Cast<byte, float>(this.regenFellerBuncherProductivity);
            Span<float> batchRegenTrackedHarvesterPMh = MemoryMarshal.Cast<byte, float>(this.regenTrackedHarvesterPMh);
            Span<float> batchRegenTrackedHarvesterProductivity = MemoryMarshal.Cast<byte, float>(this.regenTrackedHarvesterProductivity);
            Span<float> batchRegenWheeledHarvesterPMh = MemoryMarshal.Cast<byte, float>(this.regenWheeledHarvesterPMh);
            Span<float> batchRegenWheeledHarvesterProductivity = MemoryMarshal.Cast<byte, float>(this.regenWheeledHarvesterProductivity);
            Span<ChainsawCrewType> batchRegenChainsawCrewWithFellerBuncherAndGrappleSwingYarder = MemoryMarshal.Cast<byte, ChainsawCrewType>(this.regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder);
            Span<float> batchRegenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = MemoryMarshal.Cast<byte, float>(this.regenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder);
            Span<float> batchRegenChainsawCmhWithFellerBuncherAndGrappleSwingYarder = MemoryMarshal.Cast<byte, float>(this.regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder);
            Span<float> batchRegenChainsawPMhWithFellerBuncherAndGrappleSwingYarder = MemoryMarshal.Cast<byte, float>(this.regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder);
            Span<ChainsawCrewType> batchRegenChainsawCrewWithFellerBuncherAndGrappleYoader = MemoryMarshal.Cast<byte, ChainsawCrewType>(this.regenChainsawCrewWithFellerBuncherAndGrappleYoader);
            Span<float> batchRegenChainsawUtilizationWithFellerBuncherAndGrappleYoader = MemoryMarshal.Cast<byte, float>(this.regenChainsawUtilizationWithFellerBuncherAndGrappleYoader);
            Span<float> batchRegenChainsawCmhWithFellerBuncherAndGrappleYoader = MemoryMarshal.Cast<byte, float>(this.regenChainsawCmhWithFellerBuncherAndGrappleYoader);
            Span<float> batchRegenChainsawPMhWithFellerBuncherAndGrappleYoader = MemoryMarshal.Cast<byte, float>(this.regenChainsawPMhWithFellerBuncherAndGrappleYoader);
            Span<ChainsawCrewType> batchRegenChainsawCrewWithTrackedHarvester = MemoryMarshal.Cast<byte, ChainsawCrewType>(this.regenChainsawCrewWithTrackedHarvester);
            Span<float> batchRegenChainsawUtilizationWithTrackedHarvester = MemoryMarshal.Cast<byte, float>(this.regenChainsawUtilizationWithTrackedHarvester);
            Span<float> batchRegenChainsawCmhWithTrackedHarvester = MemoryMarshal.Cast<byte, float>(this.regenChainsawCmhWithTrackedHarvester);
            Span<float> batchRegenChainsawPMhWithTrackedHarvester = MemoryMarshal.Cast<byte, float>(this.regenChainsawPMhWithTrackedHarvester);
            Span<ChainsawCrewType> batchRegenChainsawCrewWithWheeledHarvester = MemoryMarshal.Cast<byte, ChainsawCrewType>(this.regenChainsawCrewWithWheeledHarvester);
            Span<float> batchRegenChainsawUtilizationWithWheeledHarvester = MemoryMarshal.Cast<byte, float>(this.regenChainsawUtilizationWithWheeledHarvester);
            Span<float> batchRegenChainsawCmhWithWheeledHarvester = MemoryMarshal.Cast<byte, float>(this.regenChainsawCmhWithWheeledHarvester);
            Span<float> batchRegenChainsawPMhWithWheeledHarvester = MemoryMarshal.Cast<byte, float>(this.regenChainsawPMhWithWheeledHarvester);
            Span<float> batchRegenGrappleSwingYarderPMhPerHectare = MemoryMarshal.Cast<byte, float>(this.regenGrappleSwingYarderPMhPerHectare);
            Span<float> batchRegenGrappleSwingYarderProductivity = MemoryMarshal.Cast<byte, float>(this.regenGrappleSwingYarderProductivity);
            Span<float> batchRegenGrappleSwingYarderOverweightFirstLogsPerHectare = MemoryMarshal.Cast<byte, float>(this.regenGrappleSwingYarderOverweightFirstLogsPerHectare);
            Span<float> batchRegenGrappleYoaderPMhPerHectare = MemoryMarshal.Cast<byte, float>(this.regenGrappleYoaderPMhPerHectare);
            Span<float> batchRegenGrappleYoaderProductivity = MemoryMarshal.Cast<byte, float>(this.regenGrappleYoaderProductivity);
            Span<float> batchRegenGrappleYoaderOverweightFirstLogsPerHectare = MemoryMarshal.Cast<byte, float>(this.regenGrappleYoaderOverweightFirstLogsPerHectare);
            Span<float> batchRegenProcessorPMhWithGrappleSwingYarder = MemoryMarshal.Cast<byte, float>(this.regenProcessorPMhWithGrappleSwingYarder);
            Span<float> batchRegenProcessorProductivityWithGrappleSwingYarder = MemoryMarshal.Cast<byte, float>(this.regenProcessorProductivityWithGrappleSwingYarder);
            Span<float> batchRegenProcessorPMhWithGrappleYoader = MemoryMarshal.Cast<byte, float>(this.regenProcessorPMhWithGrappleYoader);
            Span<float> batchRegenProcessorProductivityWithGrappleYoader = MemoryMarshal.Cast<byte, float>(this.regenProcessorProductivityWithGrappleYoader);
            Span<float> batchRegenLoadedWeight = MemoryMarshal.Cast<byte, float>(this.regenLoadedWeight);

            trajectory.GetMerchantableVolumes(out StandMerchantableVolume longLogVolume, out StandMerchantableVolume forwardedVolume);

            //SnagDownLogTable? snagsAndDownLogs = null;
            //if (writeContext.NoCarbon == false)
            //{
            //    snagsAndDownLogs = new(trajectory, writeContext.MaximumDiameter, writeContext.DiameterClassSize);
            //}

            (int firstThinAgeInt32, int secondThinAgeInt32, int thirdThinAgeInt32, int rotationAgeInt32) = writeContext.GetHarvestAges();
            Int16 firstThinAge = (Int16)firstThinAgeInt32;
            Int16 secondThinAge = (Int16)secondThinAgeInt32;
            Int16 thirdThinAge = (Int16)thirdThinAgeInt32;
            Int16 rotationAge = (Int16)rotationAgeInt32;

            int lastPeriodToCopy = startPeriod + periodsToCopy - 1;
            int financialIndex = writeContext.FinancialIndex;
            FinancialScenarios financialScenarios = writeContext.FinancialScenarios;
            StandDensity? previousStandDensity = null;
            float totalThinNetPresentValue = 0.0F;
            int? year = writeContext.StartYear;
            for (int periodIndex = startPeriod, recordIndex = startIndexInRecordBatch; periodIndex <= lastPeriodToCopy; ++periodIndex, ++recordIndex)
            {
                Stand stand = trajectory.StandByPeriod[periodIndex] ?? throw new NotSupportedException("Stand information missing for period " + periodIndex + ".");

                float basalAreaThinnedPerHa = trajectory.GetBasalAreaThinnedPerHa(periodIndex); // m²/ha
                if (writeContext.HarvestsOnly)
                {
                    if ((basalAreaThinnedPerHa == 0.0F) && (periodIndex != writeContext.EndOfRotationPeriod))
                    {
                        continue; // no trees cut in this before end of rotation period so no data to write
                    }
                }

                // financial value
                financialScenarios.TryGetNetPresentThinValue(trajectory, financialIndex, periodIndex, out HarvestFinancialValue? thinFinancialValue);
                LongLogHarvest longLogRegenHarvest = financialScenarios.GetNetPresentRegenerationHarvestValue(trajectory, financialIndex, periodIndex);

                batchStand[recordIndex] = standID;
                batchThin1[recordIndex] = firstThinAge;
                batchThin2[recordIndex] = secondThinAge;
                batchThin3[recordIndex] = thirdThinAge;
                batchRotation[recordIndex] = rotationAge;
                batchFinancialScenario[recordIndex] = (UInt32)financialIndex;
                batchYear[recordIndex] = year != null ? (Int16)year.Value : Constant.NoDataInt16;
                batchStandAge[recordIndex] = (Int16)trajectory.GetEndOfPeriodAge(periodIndex);
                
                if (writeContext.NoTreeGrowth == false)
                {
                    // get densities and volumes
                    float basalAreaIntensity = 0.0F; // fraction
                    if (periodIndex > 0)
                    {
                        previousStandDensity = trajectory.GetStandDensity(periodIndex - 1);
                        basalAreaIntensity = basalAreaThinnedPerHa / previousStandDensity.BasalAreaPerHa;
                    }

                    // TODO: support long log thins
                    float thinVolumeScribner = forwardedVolume.GetScribnerTotal(periodIndex); // MBF/ha
                    Debug.Assert((thinVolumeScribner == 0.0F && basalAreaThinnedPerHa == 0.0F) || (thinVolumeScribner > 0.0F && basalAreaThinnedPerHa > 0.0F));

                    StandDensity currentStandDensity = trajectory.GetStandDensity(periodIndex);
                    float treesPerHectareDecrease = 0.0F;
                    if (periodIndex > 0)
                    {
                        treesPerHectareDecrease = 1.0F - currentStandDensity.TreesPerHa / previousStandDensity!.TreesPerHa;
                    }

                    float quadraticMeanDiameterInCm = stand.GetQuadraticMeanDiameterInCentimeters();
                    float topHeightInM = stand.GetTopHeightInMeters();
                    // 1/(10 in * 2.54 cm/in) = 0.03937008
                    float reinekeStandDensityIndex = currentStandDensity.TreesPerHa * MathF.Pow(0.03937008F * quadraticMeanDiameterInCm, Constant.ReinekeExponent);

                    // write tree growth
                    batchTph[recordIndex] = currentStandDensity.TreesPerHa;
                    batchQmd[recordIndex] = quadraticMeanDiameterInCm;
                    batchHtop[recordIndex] = topHeightInM;
                    batchBasalArea[recordIndex] = currentStandDensity.BasalAreaPerHa;
                    batchSdi[recordIndex] = reinekeStandDensityIndex;
                    batchStandingCmh[recordIndex] = longLogVolume.GetCubicTotal(periodIndex);
                    batchStandingMbfh[recordIndex] = longLogVolume.GetScribnerTotal(periodIndex);
                    batchThinCmh[recordIndex] = forwardedVolume.GetCubicTotal(periodIndex); // TODO: support long log thins
                    batchThinMbfh[recordIndex] = thinVolumeScribner;
                    batchBAremoved[recordIndex] = basalAreaThinnedPerHa;
                    batchBAintensity[recordIndex] = basalAreaIntensity;
                    batchTPHdecrease[recordIndex] = treesPerHectareDecrease;

                    previousStandDensity = currentStandDensity;
                }
                else
                {
                    batchTph[recordIndex] = Single.NaN;
                    batchQmd[recordIndex] = Single.NaN;
                    batchHtop[recordIndex] = Single.NaN;
                    batchBasalArea[recordIndex] = Single.NaN;
                    batchSdi[recordIndex] = Single.NaN;
                    batchStandingCmh[recordIndex] = Single.NaN;
                    batchStandingMbfh[recordIndex] = Single.NaN;
                    batchThinCmh[recordIndex] = Single.NaN;
                    batchThinMbfh[recordIndex] = Single.NaN;
                    batchBAremoved[recordIndex] = Single.NaN;
                    batchBAintensity[recordIndex] = Single.NaN;
                    batchTPHdecrease[recordIndex] = Single.NaN;
                }

                if (writeContext.NoFinancial == false)
                {
                    if (thinFinancialValue != null)
                    {
                        totalThinNetPresentValue += thinFinancialValue.NetPresentValuePerHa;
                    }

                    float periodNetPresentValue = financialScenarios.GetNetPresentValue(trajectory, financialIndex, writeContext.EndOfRotationPeriod, totalThinNetPresentValue, longLogRegenHarvest);
                    float landExpectationValue = financialScenarios.GetLandExpectationValue(trajectory, financialIndex, writeContext.EndOfRotationPeriod, totalThinNetPresentValue, longLogRegenHarvest);

                    batchNpv[recordIndex] = periodNetPresentValue;
                    batchLev[recordIndex] = landExpectationValue;
                }
                else
                {
                    batchNpv[recordIndex] = Single.NaN;
                    batchLev[recordIndex] = Single.NaN;
                }

                //if (writeContext.NoCarbon == false)
                //{
                //    float liveBiomass = 0.001F * stand.GetLiveBiomass(); // Mg/ha
                //    batchLiveBimass[recordIndex] = liveBiomass;
                //    Debug.Assert(snagsAndDownLogs != null);

                //    batchSph[recordIndex] = snagsAndDownLogs.SnagsPerHectareByPeriod[periodIndex];
                //    matchSnagQmd[recordIndex] = snagsAndDownLogs.SnagQmdInCentimetersByPeriod[periodIndex]
                //}

                if (writeContext.NoHarvestCosts == false)
                {
                    if (thinFinancialValue != null)
                    {
                        if (thinFinancialValue is CutToLengthHarvest cutToLengthThin)
                        {
                            batchThinMinCostSystem[recordIndex] = cutToLengthThin.MinimumCostHarvestSystem;
                            batchThinFallerGrappleSwingYarderCost[recordIndex] = Single.NaN;
                            batchThinFallerGrappleYoaderCost[recordIndex] = Single.NaN;
                            batchThinFellerBuncherGrappleSwingYarderCost[recordIndex] = Single.NaN;
                            batchThinFellerBuncherGrappleYoaderCost[recordIndex] = Single.NaN;
                            batchThinTrackedHarvesterForwarderCost[recordIndex] = cutToLengthThin.TrackedHarvester.SystemCostPerHaWithForwarder;
                            batchThinTrackedHarvesterGrappleSwingYarderCost[recordIndex] = Single.NaN;
                            batchThinTrackedHarvesterGrappleYoaderCost[recordIndex] = Single.NaN;
                            batchThinWheeledHarvesterForwarderCost[recordIndex] = cutToLengthThin.WheeledHarvester.SystemCostPerHaWithForwarder;
                            batchThinWheeledHarvesterGrappleSwingYarderCost[recordIndex] = Single.NaN;
                            batchThinWheeledHarvesterGrappleYoaderCost[recordIndex] = Single.NaN;
                            batchThinTaskCost[recordIndex] = cutToLengthThin.HarvestRelatedTaskCostPerHa;
                        }
                        else if (thinFinancialValue is LongLogHarvest longLogThin)
                        {
                            batchThinMinCostSystem[recordIndex] = longLogThin.MinimumCostHarvestSystem;
                            batchThinFallerGrappleSwingYarderCost[recordIndex] = longLogThin.Fallers.SystemCostPerHaWithYarder;
                            batchThinFallerGrappleYoaderCost[recordIndex] = longLogThin.Fallers.SystemCostPerHaWithYoader;
                            batchThinFellerBuncherGrappleSwingYarderCost[recordIndex] = longLogThin.FellerBuncher.Yarder.SystemCostPerHa;
                            batchThinFellerBuncherGrappleYoaderCost[recordIndex] = longLogThin.FellerBuncher.Yoader.SystemCostPerHa;
                            batchThinTrackedHarvesterForwarderCost[recordIndex] = Single.NaN;
                            batchThinTrackedHarvesterGrappleSwingYarderCost[recordIndex] = longLogThin.TrackedHarvester.SystemCostPerHaWithYarder;
                            batchThinTrackedHarvesterGrappleYoaderCost[recordIndex] = longLogThin.TrackedHarvester.SystemCostPerHaWithYoader;
                            batchThinWheeledHarvesterForwarderCost[recordIndex] = Single.NaN;
                            batchThinWheeledHarvesterGrappleSwingYarderCost[recordIndex] = longLogThin.WheeledHarvester.SystemCostPerHaWithYarder;
                            batchThinWheeledHarvesterGrappleYoaderCost[recordIndex] = longLogThin.WheeledHarvester.SystemCostPerHaWithYoader;
                            batchThinTaskCost[recordIndex] = longLogThin.HarvestRelatedTaskCostPerHa;
                        }
                        else
                        {
                            throw new NotSupportedException("Unhandled thinning of type " + thinFinancialValue.GetType().Name + ".");
                        }
                    }
                    else
                    {
                        batchThinMinCostSystem[recordIndex] = HarvestSystemEquipment.None;
                        batchThinFallerGrappleSwingYarderCost[recordIndex] = Single.NaN;
                        batchThinFallerGrappleYoaderCost[recordIndex] = Single.NaN;
                        batchThinFellerBuncherGrappleSwingYarderCost[recordIndex] = Single.NaN;
                        batchThinFellerBuncherGrappleYoaderCost[recordIndex] = Single.NaN;
                        batchThinTrackedHarvesterForwarderCost[recordIndex] = Single.NaN;
                        batchThinTrackedHarvesterGrappleSwingYarderCost[recordIndex] = Single.NaN;
                        batchThinTrackedHarvesterGrappleYoaderCost[recordIndex] = Single.NaN;
                        batchThinWheeledHarvesterForwarderCost[recordIndex] = Single.NaN;
                        batchThinWheeledHarvesterGrappleSwingYarderCost[recordIndex] = Single.NaN;
                        batchThinWheeledHarvesterGrappleYoaderCost[recordIndex] = Single.NaN;
                        batchThinTaskCost[recordIndex] = Single.NaN;
                    }

                    batchRegenMinCostSystem[recordIndex] = longLogRegenHarvest.MinimumCostHarvestSystem;
                    batchRegenFallerGrappleSwingYarderCost[recordIndex] = longLogRegenHarvest.Fallers.SystemCostPerHaWithYarder;
                    batchRegenFallerGrappleYoaderCost[recordIndex] = longLogRegenHarvest.Fallers.SystemCostPerHaWithYoader;
                    batchRegenFellerBuncherGrappleSwingYarderCost[recordIndex] = longLogRegenHarvest.FellerBuncher.Yarder.SystemCostPerHa;
                    batchRegenFellerBuncherGrappleYoaderCost[recordIndex] = longLogRegenHarvest.FellerBuncher.Yoader.SystemCostPerHa;
                    // cutToLengthRegenHarvest.TrackedHarvester.SystemCostPerHaWithForwarder
                    batchRegenTrackedHarvesterGrappleSwingYarderCost[recordIndex] = longLogRegenHarvest.TrackedHarvester.SystemCostPerHaWithYarder;
                    batchRegenTrackedHarvesterGrappleYoaderCost[recordIndex] = longLogRegenHarvest.TrackedHarvester.SystemCostPerHaWithYoader;
                    // cutToLengthRegenHarvest.WheeledHarvester.SystemCostPerHaWithForwarder not applicable
                    batchRegenWheeledHarvesterGrappleSwingYarderCost[recordIndex] = longLogRegenHarvest.WheeledHarvester.SystemCostPerHaWithYarder;
                    batchRegenWheeledHarvesterGrappleYoaderCost[recordIndex] = longLogRegenHarvest.WheeledHarvester.SystemCostPerHaWithYoader;
                    batchRegenTaskCost[recordIndex] = longLogRegenHarvest.HarvestRelatedTaskCostPerHa;
                    batchReforestationNpv[recordIndex] = longLogRegenHarvest.ReforestationNpv;
                }
                if (writeContext.NoTimberSorts == false)
                {
                    if (thinFinancialValue != null)
                    {
                        batchThinLogs2S [recordIndex] = forwardedVolume.Logs2Saw[periodIndex];
                        batchThinLogs3S[recordIndex] = forwardedVolume.Logs3Saw[periodIndex];
                        batchThinLogs4S[recordIndex] = forwardedVolume.Logs4Saw[periodIndex];
                        batchThinCmh2S[recordIndex] = forwardedVolume.Cubic2Saw[periodIndex];
                        batchThinCmh3S[recordIndex] = forwardedVolume.Cubic3Saw[periodIndex];
                        batchThinCmh4S[recordIndex] = forwardedVolume.Cubic4Saw[periodIndex];
                        batchThinMbfh2S[recordIndex] = forwardedVolume.Scribner2Saw[periodIndex];
                        batchThinMbfh3S[recordIndex] = forwardedVolume.Scribner3Saw[periodIndex];
                        batchThinMbfh4S[recordIndex] = forwardedVolume.Scribner4Saw[periodIndex];
                        batchThinPond2S[recordIndex] = thinFinancialValue.PondValue2SawPerHa;
                        batchThinPond3S[recordIndex] = thinFinancialValue.PondValue3SawPerHa;
                        batchThinPond4S[recordIndex] = thinFinancialValue.PondValue4SawPerHa;
                    }
                    else
                    {
                        batchThinLogs2S[recordIndex] = Single.NaN;
                        batchThinLogs3S[recordIndex] = Single.NaN;
                        batchThinLogs4S[recordIndex] = Single.NaN;
                        batchThinCmh2S[recordIndex] = Single.NaN;
                        batchThinCmh3S[recordIndex] = Single.NaN;
                        batchThinCmh4S[recordIndex] = Single.NaN;
                        batchThinMbfh2S[recordIndex] = Single.NaN;
                        batchThinMbfh3S[recordIndex] = Single.NaN;
                        batchThinMbfh4S[recordIndex] = Single.NaN;
                        batchThinPond2S[recordIndex] = Single.NaN;
                        batchThinPond3S[recordIndex] = Single.NaN;
                        batchThinPond4S[recordIndex] = Single.NaN;
                    }
                    batchStandingLogs2S[recordIndex] = longLogVolume.Logs2Saw[periodIndex];
                    batchStandingLogs3S[recordIndex] = longLogVolume.Logs3Saw[periodIndex];
                    batchStandingLogs4S[recordIndex] = longLogVolume.Logs4Saw[periodIndex];
                    batchStandingCmh2S[recordIndex] = longLogVolume.Cubic2Saw[periodIndex];
                    batchStandingCmh3S[recordIndex] = longLogVolume.Cubic3Saw[periodIndex];
                    batchStandingCmh4S[recordIndex] = longLogVolume.Cubic4Saw[periodIndex];
                    batchStandingMbfh2S[recordIndex] = longLogVolume.Scribner2Saw[periodIndex];
                    batchStandingMbfh3S[recordIndex] = longLogVolume.Scribner3Saw[periodIndex];
                    batchStandingMbfh4S[recordIndex] = longLogVolume.Scribner4Saw[periodIndex];
                    batchRegenPond2S[recordIndex] = longLogRegenHarvest.PondValue2SawPerHa;
                    batchRegenPond3S[recordIndex] = longLogRegenHarvest.PondValue3SawPerHa;
                    batchRegenPond4S[recordIndex] = longLogRegenHarvest.PondValue4SawPerHa;
                }

                if (writeContext.NoEquipmentProductivity == false)
                {
                    if (thinFinancialValue != null)
                    {
                        if (thinFinancialValue is CutToLengthHarvest cutToLengthThin)
                        {
                            batchThinFallerPMh[recordIndex] = Single.NaN;
                            batchThinFallerProductivity[recordIndex] = Single.NaN;
                            batchThinFellerBuncherPMh[recordIndex] = Single.NaN;
                            batchThinFellerBuncherProductivity[recordIndex] = Single.NaN;
                            batchThinTrackedHarvesterPMh[recordIndex] = cutToLengthThin.TrackedHarvester.HarvesterPMhPerHa;
                            batchThinTrackedHarvesterProductivity[recordIndex] = cutToLengthThin.TrackedHarvester.HarvesterProductivity;
                            batchThinWheeledHarvesterPMh[recordIndex] = cutToLengthThin.WheeledHarvester.HarvesterPMhPerHa;
                            batchThinWheeledHarvesterProductivity[recordIndex] = cutToLengthThin.WheeledHarvester.HarvesterProductivity;
                            batchThinChainsawCrewWithFellerBuncherAndGrappleSwingYarder[recordIndex] = ChainsawCrewType.None;
                            batchThinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder[recordIndex] = Single.NaN;
                            batchThinChainsawCmhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = Single.NaN;
                            batchThinChainsawPMhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = Single.NaN;
                            batchThinChainsawCrewWithFellerBuncherAndGrappleYoader[recordIndex] = ChainsawCrewType.None;
                            batchThinChainsawUtilizationWithFellerBuncherAndGrappleYoader[recordIndex] = Single.NaN;
                            batchThinChainsawCmhWithFellerBuncherAndGrappleYoader[recordIndex] = Single.NaN;
                            batchThinChainsawPMhWithFellerBuncherAndGrappleYoader[recordIndex] = Single.NaN;
                            batchThinChainsawCrewWithTrackedHarvester[recordIndex] = cutToLengthThin.TrackedHarvester.ChainsawCrew;
                            batchThinChainsawUtilizationWithTrackedHarvester[recordIndex] = cutToLengthThin.TrackedHarvester.ChainsawUtilization;
                            batchThinChainsawCmhWithTrackedHarvester[recordIndex] = cutToLengthThin.TrackedHarvester.ChainsawCubicVolumePerHa;
                            batchThinChainsawPMhWithTrackedHarvester[recordIndex] = cutToLengthThin.TrackedHarvester.ChainsawPMhPerHa;
                            batchThinChainsawCrewWithWheeledHarvester[recordIndex] = cutToLengthThin.WheeledHarvester.ChainsawCrew;
                            batchThinChainsawUtilizationWithWheeledHarvester[recordIndex] = cutToLengthThin.WheeledHarvester.ChainsawUtilization;
                            batchThinChainsawCmhWithWheeledHarvester[recordIndex] = cutToLengthThin.WheeledHarvester.ChainsawCubicVolumePerHa;
                            batchThinChainsawPMhWithWheeledHarvester[recordIndex] = cutToLengthThin.WheeledHarvester.ChainsawPMhPerHa;
                            batchThinForwardingMethod[recordIndex] = cutToLengthThin.Forwarder.LoadingMethod;
                            batchThinForwarderPMh[recordIndex] = cutToLengthThin.Forwarder.ForwarderPMhPerHa;
                            batchThinForwarderProductivity[recordIndex] = cutToLengthThin.Forwarder.ForwarderProductivity;
                            batchThinForwardedWeight[recordIndex] = cutToLengthThin.Forwarder.ForwardedWeightPerHa;
                            batchThinGrappleSwingYarderPMhPerHectare[recordIndex] = Single.NaN;
                            batchThinGrappleSwingYarderProductivity[recordIndex] = Single.NaN;
                            batchThinGrappleSwingYarderOverweightFirstLogsPerHectare[recordIndex] = Single.NaN;
                            batchThinGrappleYoaderPMhPerHectare[recordIndex] = Single.NaN;
                            batchThinGrappleYoaderProductivity[recordIndex] = Single.NaN;
                            batchThinGrappleYoaderOverweightFirstLogsPerHectare[recordIndex] = Single.NaN;
                            batchThinProcessorPMhWithGrappleSwingYarder[recordIndex] = Single.NaN;
                            batchThinProcessorProductivityWithGrappleSwingYarder[recordIndex] = Single.NaN;
                            batchThinProcessorPMhWithGrappleYoader[recordIndex] = Single.NaN;
                            batchThinProcessorProductivityWithGrappleYoader[recordIndex] = Single.NaN;
                            batchThinLoadedWeight[recordIndex] =cutToLengthThin.Forwarder.ForwardedWeightPerHa;
                        }
                        else if (thinFinancialValue is LongLogHarvest longLogThin)
                        {
                            batchThinFallerPMh[recordIndex] = longLogThin.Fallers.ChainsawPMhPerHa;
                            batchThinFallerProductivity[recordIndex] = longLogThin.Fallers.ChainsawProductivity;
                            batchThinFellerBuncherPMh[recordIndex] = longLogThin.FellerBuncher.FellerBuncherPMhPerHa;
                            batchThinFellerBuncherProductivity[recordIndex] = longLogThin.FellerBuncher.FellerBuncherProductivity;
                            batchThinTrackedHarvesterPMh[recordIndex] = longLogThin.TrackedHarvester.HarvesterPMhPerHa;
                            batchThinTrackedHarvesterProductivity[recordIndex] = longLogThin.TrackedHarvester.HarvesterProductivity;
                            batchThinWheeledHarvesterPMh[recordIndex] = longLogThin.WheeledHarvester.HarvesterPMhPerHa;
                            batchThinWheeledHarvesterProductivity[recordIndex] = longLogThin.WheeledHarvester.HarvesterProductivity;
                            batchThinChainsawCrewWithFellerBuncherAndGrappleSwingYarder[recordIndex] = longLogThin.FellerBuncher.Yarder.ChainsawCrew;
                            batchThinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder[recordIndex] = longLogThin.FellerBuncher.Yarder.ChainsawUtilization;
                            batchThinChainsawCmhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = longLogThin.FellerBuncher.Yarder.ChainsawCubicVolumePerHa;
                            batchThinChainsawPMhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = longLogThin.FellerBuncher.Yarder.ChainsawPMhPerHa;
                            batchThinChainsawCrewWithFellerBuncherAndGrappleYoader[recordIndex] = longLogThin.FellerBuncher.Yoader.ChainsawCrew;
                            batchThinChainsawUtilizationWithFellerBuncherAndGrappleYoader[recordIndex] = longLogThin.FellerBuncher.Yoader.ChainsawUtilization;
                            batchThinChainsawCmhWithFellerBuncherAndGrappleYoader[recordIndex] = longLogThin.FellerBuncher.Yoader.ChainsawCubicVolumePerHa;
                            batchThinChainsawPMhWithFellerBuncherAndGrappleYoader[recordIndex] = longLogThin.FellerBuncher.Yoader.ChainsawPMhPerHa;
                            batchThinChainsawCrewWithTrackedHarvester[recordIndex] = longLogThin.TrackedHarvester.ChainsawCrew;
                            batchThinChainsawUtilizationWithTrackedHarvester[recordIndex] = longLogThin.TrackedHarvester.ChainsawUtilization;
                            batchThinChainsawCmhWithTrackedHarvester[recordIndex] = longLogThin.TrackedHarvester.ChainsawCubicVolumePerHa;
                            batchThinChainsawPMhWithTrackedHarvester[recordIndex] = longLogThin.TrackedHarvester.ChainsawPMhPerHa;
                            batchThinChainsawCrewWithWheeledHarvester[recordIndex] = longLogThin.WheeledHarvester.ChainsawCrew;
                            batchThinChainsawUtilizationWithWheeledHarvester[recordIndex] = longLogThin.WheeledHarvester.ChainsawUtilization;
                            batchThinChainsawCmhWithWheeledHarvester[recordIndex] = longLogThin.WheeledHarvester.ChainsawCubicVolumePerHa;
                            batchThinChainsawPMhWithWheeledHarvester[recordIndex] = longLogThin.WheeledHarvester.ChainsawPMhPerHa;
                            batchThinForwardingMethod[recordIndex] = ForwarderLoadingMethod.None;
                            batchThinForwarderPMh[recordIndex] = Single.NaN;
                            batchThinForwarderProductivity[recordIndex] = Single.NaN;
                            batchThinForwardedWeight[recordIndex] = Single.NaN;
                            batchThinGrappleSwingYarderPMhPerHectare[recordIndex] = longLogThin.Yarder.YarderPMhPerHectare;
                            batchThinGrappleSwingYarderProductivity[recordIndex] = longLogThin.Yarder.YarderProductivity;
                            batchThinGrappleSwingYarderOverweightFirstLogsPerHectare[recordIndex] = longLogThin.Yarder.OverweightFirstLogsPerHa;
                            batchThinGrappleYoaderPMhPerHectare[recordIndex] = longLogThin.Yoader.YarderPMhPerHectare;
                            batchThinGrappleYoaderProductivity[recordIndex] = longLogThin.Yoader.YarderProductivity;
                            batchThinGrappleYoaderOverweightFirstLogsPerHectare[recordIndex] = longLogThin.Yoader.OverweightFirstLogsPerHa;
                            batchThinProcessorPMhWithGrappleSwingYarder[recordIndex] = longLogThin.Yarder.ProcessorPMhPerHa;
                            batchThinProcessorProductivityWithGrappleSwingYarder[recordIndex] = longLogThin.Yarder.ProcessorProductivity;
                            batchThinProcessorPMhWithGrappleYoader[recordIndex] = longLogThin.Yoader.ProcessorPMhPerHa;
                            batchThinProcessorProductivityWithGrappleYoader[recordIndex] = longLogThin.Yoader.ProcessorProductivity;
                            batchThinLoadedWeight[recordIndex] = longLogThin.FellerBuncher.LoadedWeightPerHa;
                        }
                        else
                        {
                            throw new NotSupportedException("Unhandled thinning of type " + thinFinancialValue.GetType().Name + ".");
                        }
                    }
                    else
                    {
                        batchThinFallerPMh[recordIndex] = Single.NaN;
                        batchThinFallerProductivity[recordIndex] = Single.NaN;
                        batchThinFellerBuncherPMh[recordIndex] = Single.NaN;
                        batchThinFellerBuncherProductivity[recordIndex] = Single.NaN;
                        batchThinTrackedHarvesterPMh[recordIndex] = Single.NaN;
                        batchThinTrackedHarvesterProductivity[recordIndex] = Single.NaN;
                        batchThinWheeledHarvesterPMh[recordIndex] = Single.NaN;
                        batchThinWheeledHarvesterProductivity[recordIndex] = Single.NaN;
                        batchThinChainsawCrewWithFellerBuncherAndGrappleSwingYarder[recordIndex] = ChainsawCrewType.None;
                        batchThinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder[recordIndex] = Single.NaN;
                        batchThinChainsawCmhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = Single.NaN;
                        batchThinChainsawPMhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = Single.NaN;
                        batchThinChainsawCrewWithFellerBuncherAndGrappleYoader[recordIndex] = ChainsawCrewType.None;
                        batchThinChainsawUtilizationWithFellerBuncherAndGrappleYoader[recordIndex] = Single.NaN;
                        batchThinChainsawCmhWithFellerBuncherAndGrappleYoader[recordIndex] = Single.NaN;
                        batchThinChainsawPMhWithFellerBuncherAndGrappleYoader[recordIndex] = Single.NaN;
                        batchThinChainsawCrewWithTrackedHarvester[recordIndex] = ChainsawCrewType.None;
                        batchThinChainsawUtilizationWithTrackedHarvester[recordIndex] = Single.NaN;
                        batchThinChainsawCmhWithTrackedHarvester[recordIndex] = Single.NaN;
                        batchThinChainsawPMhWithTrackedHarvester[recordIndex] = Single.NaN;
                        batchThinChainsawCrewWithWheeledHarvester[recordIndex] = ChainsawCrewType.None;
                        batchThinChainsawUtilizationWithWheeledHarvester[recordIndex] = Single.NaN;
                        batchThinChainsawCmhWithWheeledHarvester[recordIndex] = Single.NaN;
                        batchThinChainsawPMhWithWheeledHarvester[recordIndex] = Single.NaN;
                        batchThinForwardingMethod[recordIndex] = ForwarderLoadingMethod.None;
                        batchThinForwarderPMh[recordIndex] = Single.NaN;
                        batchThinForwarderProductivity[recordIndex] = Single.NaN;
                        batchThinForwardedWeight[recordIndex] = Single.NaN;
                        batchThinGrappleSwingYarderPMhPerHectare[recordIndex] = Single.NaN;
                        batchThinGrappleSwingYarderProductivity[recordIndex] = Single.NaN;
                        batchThinGrappleSwingYarderOverweightFirstLogsPerHectare[recordIndex] = Single.NaN;
                        batchThinGrappleYoaderPMhPerHectare[recordIndex] = Single.NaN;
                        batchThinGrappleYoaderProductivity[recordIndex] = Single.NaN;
                        batchThinGrappleYoaderOverweightFirstLogsPerHectare[recordIndex] = Single.NaN;
                        batchThinProcessorPMhWithGrappleSwingYarder[recordIndex] = Single.NaN;
                        batchThinProcessorProductivityWithGrappleSwingYarder[recordIndex] = Single.NaN;
                        batchThinProcessorPMhWithGrappleYoader[recordIndex] = Single.NaN;
                        batchThinProcessorProductivityWithGrappleYoader[recordIndex] = Single.NaN;
                        batchThinLoadedWeight[recordIndex] = Single.NaN;
                    }

                    batchRegenFallerPMh[recordIndex] = longLogRegenHarvest.Fallers.ChainsawPMhPerHa;
                    batchRegenFallerProductivity[recordIndex] = longLogRegenHarvest.Fallers.ChainsawProductivity;
                    batchRegenFellerBuncherPMh[recordIndex] = longLogRegenHarvest.FellerBuncher.FellerBuncherPMhPerHa;
                    batchRegenFellerBuncherProductivity[recordIndex] = longLogRegenHarvest.FellerBuncher.FellerBuncherProductivity;
                    batchRegenTrackedHarvesterPMh[recordIndex] = longLogRegenHarvest.TrackedHarvester.HarvesterPMhPerHa;
                    batchRegenTrackedHarvesterProductivity[recordIndex] = longLogRegenHarvest.TrackedHarvester.HarvesterProductivity;
                    batchRegenWheeledHarvesterPMh[recordIndex] = longLogRegenHarvest.WheeledHarvester.HarvesterPMhPerHa;
                    batchRegenWheeledHarvesterProductivity[recordIndex] = longLogRegenHarvest.WheeledHarvester.HarvesterProductivity;
                    batchRegenChainsawCrewWithFellerBuncherAndGrappleSwingYarder[recordIndex] = longLogRegenHarvest.FellerBuncher.Yarder.ChainsawCrew;
                    batchRegenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder [recordIndex] = longLogRegenHarvest.FellerBuncher.Yarder.ChainsawUtilization;
                    batchRegenChainsawCmhWithFellerBuncherAndGrappleSwingYarder[recordIndex] = longLogRegenHarvest.FellerBuncher.Yarder.ChainsawCubicVolumePerHa;
                    batchRegenChainsawPMhWithFellerBuncherAndGrappleSwingYarder [recordIndex] = longLogRegenHarvest.FellerBuncher.Yarder.ChainsawPMhPerHa;
                    batchRegenChainsawCrewWithFellerBuncherAndGrappleYoader[recordIndex] = longLogRegenHarvest.FellerBuncher.Yoader.ChainsawCrew;
                    batchRegenChainsawUtilizationWithFellerBuncherAndGrappleYoader[recordIndex] = longLogRegenHarvest.FellerBuncher.Yoader.ChainsawUtilization;
                    batchRegenChainsawCmhWithFellerBuncherAndGrappleYoader[recordIndex] = longLogRegenHarvest.FellerBuncher.Yoader.ChainsawCubicVolumePerHa;
                    batchRegenChainsawPMhWithFellerBuncherAndGrappleYoader[recordIndex] = longLogRegenHarvest.FellerBuncher.Yoader.ChainsawPMhPerHa;
                    batchRegenChainsawCrewWithTrackedHarvester[recordIndex] = longLogRegenHarvest.TrackedHarvester.ChainsawCrew;
                    batchRegenChainsawUtilizationWithTrackedHarvester[recordIndex] = longLogRegenHarvest.TrackedHarvester.ChainsawUtilization;
                    batchRegenChainsawCmhWithTrackedHarvester[recordIndex] = longLogRegenHarvest.TrackedHarvester.ChainsawCubicVolumePerHa;
                    batchRegenChainsawPMhWithTrackedHarvester[recordIndex] = longLogRegenHarvest.TrackedHarvester.ChainsawPMhPerHa;
                    batchRegenChainsawCrewWithWheeledHarvester[recordIndex] = longLogRegenHarvest.WheeledHarvester.ChainsawCrew;
                    batchRegenChainsawUtilizationWithWheeledHarvester[recordIndex] = longLogRegenHarvest.WheeledHarvester.ChainsawUtilization;
                    batchRegenChainsawCmhWithWheeledHarvester[recordIndex] = longLogRegenHarvest.WheeledHarvester.ChainsawCubicVolumePerHa;
                    batchRegenChainsawPMhWithWheeledHarvester[recordIndex] = longLogRegenHarvest.WheeledHarvester.ChainsawPMhPerHa;
                    batchRegenGrappleSwingYarderPMhPerHectare[recordIndex] = longLogRegenHarvest.Yarder.YarderPMhPerHectare;
                    batchRegenGrappleSwingYarderProductivity[recordIndex] = longLogRegenHarvest.Yarder.YarderProductivity;
                    batchRegenGrappleSwingYarderOverweightFirstLogsPerHectare[recordIndex] = longLogRegenHarvest.Yarder.OverweightFirstLogsPerHa;
                    batchRegenGrappleYoaderPMhPerHectare[recordIndex] = longLogRegenHarvest.Yoader.YarderPMhPerHectare;
                    batchRegenGrappleYoaderProductivity[recordIndex] = longLogRegenHarvest.Yoader.YarderProductivity;
                    batchRegenGrappleYoaderOverweightFirstLogsPerHectare[recordIndex] = longLogRegenHarvest.Yoader.OverweightFirstLogsPerHa;
                    batchRegenProcessorPMhWithGrappleSwingYarder[recordIndex] = longLogRegenHarvest.Yarder.ProcessorPMhPerHa;
                    batchRegenProcessorProductivityWithGrappleSwingYarder[recordIndex] = longLogRegenHarvest.Yarder.ProcessorProductivity;
                    batchRegenProcessorPMhWithGrappleYoader[recordIndex] = longLogRegenHarvest.Yoader.ProcessorPMhPerHa;
                    batchRegenProcessorProductivityWithGrappleYoader[recordIndex] = longLogRegenHarvest.Yoader.ProcessorProductivity;
                    batchRegenLoadedWeight[recordIndex] = longLogRegenHarvest.FellerBuncher.LoadedWeightPerHa;
                }

                if (year != null)
                {
                    year += trajectory.PeriodLengthInYears;
                }
            }

            this.Count += periodsToCopy;
        }

        public void Add(StandTrajectory trajectory, WriteStandTrajectoryContext writeContext)
        {
            Debug.Assert((writeContext.EndOfRotationPeriod >= 0) && (writeContext.FinancialIndex >= 0));

            int periodsToCopy = writeContext.GetPeriodsToWrite(trajectory);

            (int startIndexInRecordBatch, int periodsToCopyToRecordBatch) = this.GetBatchIndicesForAdd(periodsToCopy);
            if (startIndexInRecordBatch == 0)
            {
                this.AppendNewBatch();
            }
            this.Add(trajectory, 0, writeContext, startIndexInRecordBatch, periodsToCopyToRecordBatch);

            int periodsRemainingToCopy = periodsToCopy - periodsToCopyToRecordBatch;
            if (periodsRemainingToCopy > 0)
            {
                this.AppendNewBatch();
                this.Add(trajectory, periodsToCopyToRecordBatch, writeContext, 0, periodsRemainingToCopy);
            }
        }

        private void AppendNewBatch()
        {
            int capacityInRecords = this.GetNextBatchLength();

            this.stand = new byte[capacityInRecords * sizeof(UInt32)];
            this.thin1 = new byte[capacityInRecords * sizeof(Int16)];
            this.thin2 = new byte[capacityInRecords * sizeof(Int16)];
            this.thin3 = new byte[capacityInRecords * sizeof(Int16)];
            this.rotation = new byte[capacityInRecords * sizeof(Int16)];
            this.financialScenario = new byte[capacityInRecords * sizeof(UInt32)];
            this.year = new byte[capacityInRecords * sizeof(Int16)];
            this.standAge = new byte[capacityInRecords * sizeof(Int16)];

            // tree growth
            this.tph = new byte[capacityInRecords * sizeof(float)];
            this.qmd = new byte[capacityInRecords * sizeof(float)];
            this.hTop = new byte[capacityInRecords * sizeof(float)];
            this.basalArea = new byte[capacityInRecords * sizeof(float)];
            this.reinekeSdi = new byte[capacityInRecords * sizeof(float)];
            this.standingCmh = new byte[capacityInRecords * sizeof(float)];
            this.standingMbfh = new byte[capacityInRecords * sizeof(float)];
            this.thinCmh = new byte[capacityInRecords * sizeof(float)];
            this.thinMbfh = new byte[capacityInRecords * sizeof(float)];
            this.baRemoved = new byte[capacityInRecords * sizeof(float)];
            this.baIntensity = new byte[capacityInRecords * sizeof(float)];
            this.tphDecrease = new byte[capacityInRecords * sizeof(float)];

            // financial
            this.npv = new byte[capacityInRecords * sizeof(float)];
            this.lev = new byte[capacityInRecords * sizeof(float)];
            // carbon - not supported for now
            //new("liveTreeBiomass = new byte[capacityInRecords * sizeof(float)];
            //new("SPH = new byte[capacityInRecords * sizeof(float)];
            //new("snagQMD = new byte[capacityInRecords * sizeof(float)];

            // harvest cost
            this.thinMinCostSystem = new byte[capacityInRecords * sizeof(byte)];
            this.thinFallerGrappleSwingYarderCost = new byte[capacityInRecords * sizeof(float)];
            this.thinFallerGrappleYoaderCost = new byte[capacityInRecords * sizeof(float)];
            this.thinFellerBuncherGrappleSwingYarderCost = new byte[capacityInRecords * sizeof(float)];
            this.thinFellerBuncherGrappleYoaderCost = new byte[capacityInRecords * sizeof(float)];
            this.thinTrackedHarvesterForwarderCost = new byte[capacityInRecords * sizeof(float)];
            this.thinTrackedHarvesterGrappleSwingYarderCost = new byte[capacityInRecords * sizeof(float)];
            this.thinTrackedHarvesterGrappleYoaderCost = new byte[capacityInRecords * sizeof(float)];
            this.thinWheeledHarvesterForwarderCost = new byte[capacityInRecords * sizeof(float)];
            this.thinWheeledHarvesterGrappleSwingYarderCost = new byte[capacityInRecords * sizeof(float)];
            this.thinWheeledHarvesterGrappleYoaderCost = new byte[capacityInRecords * sizeof(float)];
            this.thinTaskCost = new byte[capacityInRecords * sizeof(float)];
            this.regenMinCostSystem = new byte[capacityInRecords * sizeof(byte)];
            this.regenFallerGrappleSwingYarderCost = new byte[capacityInRecords * sizeof(float)];
            this.regenFallerGrappleYoaderCost = new byte[capacityInRecords * sizeof(float)];
            this.regenFellerBuncherGrappleSwingYarderCost = new byte[capacityInRecords * sizeof(float)];
            this.regenFellerBuncherGrappleYoaderCost = new byte[capacityInRecords * sizeof(float)];
            this.regenTrackedHarvesterGrappleSwingYarderCost = new byte[capacityInRecords * sizeof(float)];
            this.regenTrackedHarvesterGrappleYoaderCost = new byte[capacityInRecords * sizeof(float)];
            this.regenWheeledHarvesterGrappleSwingYarderCost = new byte[capacityInRecords * sizeof(float)];
            this.regenWheeledHarvesterGrappleYoaderCost = new byte[capacityInRecords * sizeof(float)];
            this.regenTaskCost = new byte[capacityInRecords * sizeof(float)];
            this.reforestationNpv = new byte[capacityInRecords * sizeof(float)];

            // timber sorts
            this.thinLogs2S = new byte[capacityInRecords * sizeof(float)];
            this.thinLogs3S = new byte[capacityInRecords * sizeof(float)];
            this.thinLogs4S = new byte[capacityInRecords * sizeof(float)];
            this.thinCmh2S = new byte[capacityInRecords * sizeof(float)];
            this.thinCmh3S = new byte[capacityInRecords * sizeof(float)];
            this.thinCmh4S = new byte[capacityInRecords * sizeof(float)];
            this.thinMbfh2S = new byte[capacityInRecords * sizeof(float)];
            this.thinMbfh3S = new byte[capacityInRecords * sizeof(float)];
            this.thinMbfh4S = new byte[capacityInRecords * sizeof(float)];
            this.thinPond2S = new byte[capacityInRecords * sizeof(float)];
            this.thinPond3S = new byte[capacityInRecords * sizeof(float)];
            this.thinPond4S = new byte[capacityInRecords * sizeof(float)];
            this.standingLogs2S = new byte[capacityInRecords * sizeof(float)];
            this.standingLogs3S = new byte[capacityInRecords * sizeof(float)];
            this.standingLogs4S = new byte[capacityInRecords * sizeof(float)];
            this.standingCmh2S = new byte[capacityInRecords * sizeof(float)];
            this.standingCmh3S = new byte[capacityInRecords * sizeof(float)];
            this.standingCmh4S = new byte[capacityInRecords * sizeof(float)];
            this.standingMbfh2S = new byte[capacityInRecords * sizeof(float)];
            this.standingMbfh3S = new byte[capacityInRecords * sizeof(float)];
            this.standingMbfh4S = new byte[capacityInRecords * sizeof(float)];
            this.regenPond2S = new byte[capacityInRecords * sizeof(float)];
            this.regenPond3S = new byte[capacityInRecords * sizeof(float)];
            this.regenPond4S = new byte[capacityInRecords * sizeof(float)];

            // equipment productivity
            this.thinFallerPMh = new byte[capacityInRecords * sizeof(float)];
            this.thinFallerProductivity = new byte[capacityInRecords * sizeof(float)];
            this.thinFellerBuncherPMh = new byte[capacityInRecords * sizeof(float)];
            this.thinFellerBuncherProductivity = new byte[capacityInRecords * sizeof(float)];
            this.thinTrackedHarvesterPMh = new byte[capacityInRecords * sizeof(float)];
            this.thinTrackedHarvesterProductivity = new byte[capacityInRecords * sizeof(float)];
            this.thinWheeledHarvesterPMh = new byte[capacityInRecords * sizeof(float)];
            this.thinWheeledHarvesterProductivity = new byte[capacityInRecords * sizeof(float)];
            this.thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder = new byte[capacityInRecords * sizeof(byte)];
            this.thinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = new byte[capacityInRecords * sizeof(float)];
            this.thinChainsawCmhWithFellerBuncherAndGrappleSwingYarder = new byte[capacityInRecords * sizeof(float)];
            this.thinChainsawPMhWithFellerBuncherAndGrappleSwingYarder = new byte[capacityInRecords * sizeof(float)];
            this.thinChainsawCrewWithFellerBuncherAndGrappleYoader = new byte[capacityInRecords * sizeof(byte)];
            this.thinChainsawUtilizationWithFellerBuncherAndGrappleYoader = new byte[capacityInRecords * sizeof(float)];
            this.thinChainsawCmhWithFellerBuncherAndGrappleYoader = new byte[capacityInRecords * sizeof(float)];
            this.thinChainsawPMhWithFellerBuncherAndGrappleYoader = new byte[capacityInRecords * sizeof(float)];
            this.thinChainsawCrewWithTrackedHarvester = new byte[capacityInRecords * sizeof(byte)];
            this.thinChainsawUtilizationWithTrackedHarvester = new byte[capacityInRecords * sizeof(float)];
            this.thinChainsawCmhWithTrackedHarvester = new byte[capacityInRecords * sizeof(float)];
            this.thinChainsawPMhWithTrackedHarvester = new byte[capacityInRecords * sizeof(float)];
            this.thinChainsawCrewWithWheeledHarvester = new byte[capacityInRecords * sizeof(byte)];
            this.thinChainsawUtilizationWithWheeledHarvester = new byte[capacityInRecords * sizeof(float)];
            this.thinChainsawCmhWithWheeledHarvester = new byte[capacityInRecords * sizeof(float)];
            this.thinChainsawPMhWithWheeledHarvester = new byte[capacityInRecords * sizeof(float)];
            this.thinForwardingMethod = new byte[capacityInRecords * sizeof(byte)];
            this.thinForwarderPMh = new byte[capacityInRecords * sizeof(float)];
            this.thinForwarderProductivity = new byte[capacityInRecords * sizeof(float)];
            this.thinForwardedWeight = new byte[capacityInRecords * sizeof(float)];
            this.thinGrappleSwingYarderPMhPerHectare = new byte[capacityInRecords * sizeof(float)];
            this.thinGrappleSwingYarderProductivity = new byte[capacityInRecords * sizeof(float)];
            this.thinGrappleSwingYarderOverweightFirstLogsPerHectare = new byte[capacityInRecords * sizeof(float)];
            this.thinGrappleYoaderPMhPerHectare = new byte[capacityInRecords * sizeof(float)];
            this.thinGrappleYoaderProductivity = new byte[capacityInRecords * sizeof(float)];
            this.thinGrappleYoaderOverweightFirstLogsPerHectare = new byte[capacityInRecords * sizeof(float)];
            this.thinProcessorPMhWithGrappleSwingYarder = new byte[capacityInRecords * sizeof(float)];
            this.thinProcessorProductivityWithGrappleSwingYarder = new byte[capacityInRecords * sizeof(float)];
            this.thinProcessorPMhWithGrappleYoader = new byte[capacityInRecords * sizeof(float)];
            this.thinProcessorProductivityWithGrappleYoader = new byte[capacityInRecords * sizeof(float)];
            this.thinLoadedWeight = new byte[capacityInRecords * sizeof(float)];

            this.regenFallerPMh = new byte[capacityInRecords * sizeof(float)];
            this.regenFallerProductivity = new byte[capacityInRecords * sizeof(float)];
            this.regenFellerBuncherPMh = new byte[capacityInRecords * sizeof(float)];
            this.regenFellerBuncherProductivity = new byte[capacityInRecords * sizeof(float)];
            this.regenTrackedHarvesterPMh = new byte[capacityInRecords * sizeof(float)];
            this.regenTrackedHarvesterProductivity = new byte[capacityInRecords * sizeof(float)];
            this.regenWheeledHarvesterPMh = new byte[capacityInRecords * sizeof(float)];
            this.regenWheeledHarvesterProductivity = new byte[capacityInRecords * sizeof(float)];
            this.regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder = new byte[capacityInRecords * sizeof(byte)];
            this.regenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = new byte[capacityInRecords * sizeof(float)];
            this.regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder = new byte[capacityInRecords * sizeof(float)];
            this.regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder = new byte[capacityInRecords * sizeof(float)];
            this.regenChainsawCrewWithFellerBuncherAndGrappleYoader = new byte[capacityInRecords * sizeof(byte)];
            this.regenChainsawUtilizationWithFellerBuncherAndGrappleYoader = new byte[capacityInRecords * sizeof(float)];
            this.regenChainsawCmhWithFellerBuncherAndGrappleYoader = new byte[capacityInRecords * sizeof(float)];
            this.regenChainsawPMhWithFellerBuncherAndGrappleYoader = new byte[capacityInRecords * sizeof(float)];
            this.regenChainsawCrewWithTrackedHarvester = new byte[capacityInRecords * sizeof(byte)];
            this.regenChainsawUtilizationWithTrackedHarvester = new byte[capacityInRecords * sizeof(float)];
            this.regenChainsawCmhWithTrackedHarvester = new byte[capacityInRecords * sizeof(float)];
            this.regenChainsawPMhWithTrackedHarvester = new byte[capacityInRecords * sizeof(float)];
            this.regenChainsawCrewWithWheeledHarvester = new byte[capacityInRecords * sizeof(byte)];
            this.regenChainsawUtilizationWithWheeledHarvester = new byte[capacityInRecords * sizeof(float)];
            this.regenChainsawCmhWithWheeledHarvester = new byte[capacityInRecords * sizeof(float)];
            this.regenChainsawPMhWithWheeledHarvester = new byte[capacityInRecords * sizeof(float)];
            this.regenGrappleSwingYarderPMhPerHectare = new byte[capacityInRecords * sizeof(float)];
            this.regenGrappleSwingYarderProductivity = new byte[capacityInRecords * sizeof(float)];
            this.regenGrappleSwingYarderOverweightFirstLogsPerHectare = new byte[capacityInRecords * sizeof(float)];
            this.regenGrappleYoaderPMhPerHectare = new byte[capacityInRecords * sizeof(float)];
            this.regenGrappleYoaderProductivity = new byte[capacityInRecords * sizeof(float)];
            this.regenGrappleYoaderOverweightFirstLogsPerHectare = new byte[capacityInRecords * sizeof(float)];
            this.regenProcessorPMhWithGrappleSwingYarder = new byte[capacityInRecords * sizeof(float)];
            this.regenProcessorProductivityWithGrappleSwingYarder = new byte[capacityInRecords * sizeof(float)];
            this.regenProcessorPMhWithGrappleYoader = new byte[capacityInRecords * sizeof(float)];
            this.regenProcessorProductivityWithGrappleYoader = new byte[capacityInRecords * sizeof(float)];
            this.regenLoadedWeight = new byte[capacityInRecords * sizeof(float)];

            // repackage arrays into Arrow record batch
            // Order must match schema. Mismatches result in column data swaps or corruption, depending on sizes of data elements.
            IArrowArray[] arrowArrays = new IArrowArray[]
            {
                ArrowArrayExtensions.WrapInUInt32(this.stand),
                ArrowArrayExtensions.WrapInInt16(this.thin1),
                ArrowArrayExtensions.WrapInInt16(this.thin2),
                ArrowArrayExtensions.WrapInInt16(this.thin3),
                ArrowArrayExtensions.WrapInInt16(this.rotation),
                ArrowArrayExtensions.WrapInUInt32(this.financialScenario),
                ArrowArrayExtensions.WrapInInt16(this.year),
                ArrowArrayExtensions.WrapInInt16(this.standAge),

                // tree growth
                ArrowArrayExtensions.WrapInFloat(this.tph),
                ArrowArrayExtensions.WrapInFloat(this.qmd),
                ArrowArrayExtensions.WrapInFloat(this.hTop),
                ArrowArrayExtensions.WrapInFloat(this.basalArea),
                ArrowArrayExtensions.WrapInFloat(this.reinekeSdi),
                ArrowArrayExtensions.WrapInFloat(this.standingCmh),
                ArrowArrayExtensions.WrapInFloat(this.standingMbfh),
                ArrowArrayExtensions.WrapInFloat(this.thinCmh),
                ArrowArrayExtensions.WrapInFloat(this.thinMbfh),
                ArrowArrayExtensions.WrapInFloat(this.baRemoved),
                ArrowArrayExtensions.WrapInFloat(this.baIntensity),
                ArrowArrayExtensions.WrapInFloat(this.tphDecrease),

                // financial
                ArrowArrayExtensions.WrapInFloat(this.npv),
                ArrowArrayExtensions.WrapInFloat(this.lev),
                // carbon - not supported for now
                //ArrowArrayExtensions.WrapInFloat(this.liveTreeBiomass),
                //ArrowArrayExtensions.WrapInFloat(this.SPH),
                //ArrowArrayExtensions.WrapInFloat(this.snagQMD),

                // harvest cost
                ArrowArrayExtensions.WrapInUInt8(this.thinMinCostSystem),
                ArrowArrayExtensions.WrapInFloat(this.thinFallerGrappleSwingYarderCost),
                ArrowArrayExtensions.WrapInFloat(this.thinFallerGrappleYoaderCost),
                ArrowArrayExtensions.WrapInFloat(this.thinFellerBuncherGrappleSwingYarderCost),
                ArrowArrayExtensions.WrapInFloat(this.thinFellerBuncherGrappleYoaderCost),
                ArrowArrayExtensions.WrapInFloat(this.thinTrackedHarvesterForwarderCost),
                ArrowArrayExtensions.WrapInFloat(this.thinTrackedHarvesterGrappleSwingYarderCost),
                ArrowArrayExtensions.WrapInFloat(this.thinTrackedHarvesterGrappleYoaderCost),
                ArrowArrayExtensions.WrapInFloat(this.thinWheeledHarvesterForwarderCost),
                ArrowArrayExtensions.WrapInFloat(this.thinWheeledHarvesterGrappleSwingYarderCost),
                ArrowArrayExtensions.WrapInFloat(this.thinWheeledHarvesterGrappleYoaderCost),
                ArrowArrayExtensions.WrapInFloat(this.thinTaskCost),
                ArrowArrayExtensions.WrapInUInt8(this.regenMinCostSystem),
                ArrowArrayExtensions.WrapInFloat(this.regenFallerGrappleSwingYarderCost),
                ArrowArrayExtensions.WrapInFloat(this.regenFallerGrappleYoaderCost),
                ArrowArrayExtensions.WrapInFloat(this.regenFellerBuncherGrappleSwingYarderCost),
                ArrowArrayExtensions.WrapInFloat(this.regenFellerBuncherGrappleYoaderCost),
                ArrowArrayExtensions.WrapInFloat(this.regenTrackedHarvesterGrappleSwingYarderCost),
                ArrowArrayExtensions.WrapInFloat(this.regenTrackedHarvesterGrappleYoaderCost),
                ArrowArrayExtensions.WrapInFloat(this.regenWheeledHarvesterGrappleSwingYarderCost),
                ArrowArrayExtensions.WrapInFloat(this.regenWheeledHarvesterGrappleYoaderCost),
                ArrowArrayExtensions.WrapInFloat(this.regenTaskCost),
                ArrowArrayExtensions.WrapInFloat(this.reforestationNpv),

                // timber sorts
                ArrowArrayExtensions.WrapInFloat(this.thinLogs2S),
                ArrowArrayExtensions.WrapInFloat(this.thinLogs3S),
                ArrowArrayExtensions.WrapInFloat(this.thinLogs4S),
                ArrowArrayExtensions.WrapInFloat(this.thinCmh2S),
                ArrowArrayExtensions.WrapInFloat(this.thinCmh3S),
                ArrowArrayExtensions.WrapInFloat(this.thinCmh4S),
                ArrowArrayExtensions.WrapInFloat(this.thinMbfh2S),
                ArrowArrayExtensions.WrapInFloat(this.thinMbfh3S),
                ArrowArrayExtensions.WrapInFloat(this.thinMbfh4S),
                ArrowArrayExtensions.WrapInFloat(this.thinPond2S),
                ArrowArrayExtensions.WrapInFloat(this.thinPond3S),
                ArrowArrayExtensions.WrapInFloat(this.thinPond4S),
                ArrowArrayExtensions.WrapInFloat(this.standingLogs2S),
                ArrowArrayExtensions.WrapInFloat(this.standingLogs3S),
                ArrowArrayExtensions.WrapInFloat(this.standingLogs4S),
                ArrowArrayExtensions.WrapInFloat(this.standingCmh2S),
                ArrowArrayExtensions.WrapInFloat(this.standingCmh3S),
                ArrowArrayExtensions.WrapInFloat(this.standingCmh4S),
                ArrowArrayExtensions.WrapInFloat(this.standingMbfh2S),
                ArrowArrayExtensions.WrapInFloat(this.standingMbfh3S),
                ArrowArrayExtensions.WrapInFloat(this.standingMbfh4S),
                ArrowArrayExtensions.WrapInFloat(this.regenPond2S),
                ArrowArrayExtensions.WrapInFloat(this.regenPond3S),
                ArrowArrayExtensions.WrapInFloat(this.regenPond4S),

                // equipment productivity
                ArrowArrayExtensions.WrapInFloat(this.thinFallerPMh),
                ArrowArrayExtensions.WrapInFloat(this.thinFallerProductivity),
                ArrowArrayExtensions.WrapInFloat(this.thinFellerBuncherPMh),
                ArrowArrayExtensions.WrapInFloat(this.thinFellerBuncherProductivity),
                ArrowArrayExtensions.WrapInFloat(this.thinTrackedHarvesterPMh),
                ArrowArrayExtensions.WrapInFloat(this.thinTrackedHarvesterProductivity),
                ArrowArrayExtensions.WrapInFloat(this.thinWheeledHarvesterPMh),
                ArrowArrayExtensions.WrapInFloat(this.thinWheeledHarvesterProductivity),
                ArrowArrayExtensions.WrapInUInt8(this.thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder),
                ArrowArrayExtensions.WrapInFloat(this.thinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder),
                ArrowArrayExtensions.WrapInFloat(this.thinChainsawCmhWithFellerBuncherAndGrappleSwingYarder),
                ArrowArrayExtensions.WrapInFloat(this.thinChainsawPMhWithFellerBuncherAndGrappleSwingYarder),
                ArrowArrayExtensions.WrapInUInt8(this.thinChainsawCrewWithFellerBuncherAndGrappleYoader),
                ArrowArrayExtensions.WrapInFloat(this.thinChainsawUtilizationWithFellerBuncherAndGrappleYoader),
                ArrowArrayExtensions.WrapInFloat(this.thinChainsawCmhWithFellerBuncherAndGrappleYoader),
                ArrowArrayExtensions.WrapInFloat(this.thinChainsawPMhWithFellerBuncherAndGrappleYoader),
                ArrowArrayExtensions.WrapInUInt8(this.thinChainsawCrewWithTrackedHarvester),
                ArrowArrayExtensions.WrapInFloat(this.thinChainsawUtilizationWithTrackedHarvester),
                ArrowArrayExtensions.WrapInFloat(this.thinChainsawCmhWithTrackedHarvester),
                ArrowArrayExtensions.WrapInFloat(this.thinChainsawPMhWithTrackedHarvester),
                ArrowArrayExtensions.WrapInUInt8(this.thinChainsawCrewWithWheeledHarvester),
                ArrowArrayExtensions.WrapInFloat(this.thinChainsawUtilizationWithWheeledHarvester),
                ArrowArrayExtensions.WrapInFloat(this.thinChainsawCmhWithWheeledHarvester),
                ArrowArrayExtensions.WrapInFloat(this.thinChainsawPMhWithWheeledHarvester),
                ArrowArrayExtensions.WrapInUInt8(this.thinForwardingMethod),
                ArrowArrayExtensions.WrapInFloat(this.thinForwarderPMh),
                ArrowArrayExtensions.WrapInFloat(this.thinForwarderProductivity),
                ArrowArrayExtensions.WrapInFloat(this.thinForwardedWeight),
                ArrowArrayExtensions.WrapInFloat(this.thinGrappleSwingYarderPMhPerHectare),
                ArrowArrayExtensions.WrapInFloat(this.thinGrappleSwingYarderProductivity),
                ArrowArrayExtensions.WrapInFloat(this.thinGrappleSwingYarderOverweightFirstLogsPerHectare),
                ArrowArrayExtensions.WrapInFloat(this.thinGrappleYoaderPMhPerHectare),
                ArrowArrayExtensions.WrapInFloat(this.thinGrappleYoaderProductivity),
                ArrowArrayExtensions.WrapInFloat(this.thinGrappleYoaderOverweightFirstLogsPerHectare),
                ArrowArrayExtensions.WrapInFloat(this.thinProcessorPMhWithGrappleSwingYarder),
                ArrowArrayExtensions.WrapInFloat(this.thinProcessorProductivityWithGrappleSwingYarder),
                ArrowArrayExtensions.WrapInFloat(this.thinProcessorPMhWithGrappleYoader),
                ArrowArrayExtensions.WrapInFloat(this.thinProcessorProductivityWithGrappleYoader),
                ArrowArrayExtensions.WrapInFloat(this.thinLoadedWeight),

                ArrowArrayExtensions.WrapInFloat(this.regenFallerPMh),
                ArrowArrayExtensions.WrapInFloat(this.regenFallerProductivity),
                ArrowArrayExtensions.WrapInFloat(this.regenFellerBuncherPMh),
                ArrowArrayExtensions.WrapInFloat(this.regenFellerBuncherProductivity),
                ArrowArrayExtensions.WrapInFloat(this.regenTrackedHarvesterPMh),
                ArrowArrayExtensions.WrapInFloat(this.regenTrackedHarvesterProductivity),
                ArrowArrayExtensions.WrapInFloat(this.regenWheeledHarvesterPMh),
                ArrowArrayExtensions.WrapInFloat(this.regenWheeledHarvesterProductivity),
                ArrowArrayExtensions.WrapInUInt8(this.regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder),
                ArrowArrayExtensions.WrapInFloat(this.regenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder),
                ArrowArrayExtensions.WrapInFloat(this.regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder),
                ArrowArrayExtensions.WrapInFloat(this.regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder),
                ArrowArrayExtensions.WrapInUInt8(this.regenChainsawCrewWithFellerBuncherAndGrappleYoader),
                ArrowArrayExtensions.WrapInFloat(this.regenChainsawUtilizationWithFellerBuncherAndGrappleYoader),
                ArrowArrayExtensions.WrapInFloat(this.regenChainsawCmhWithFellerBuncherAndGrappleYoader),
                ArrowArrayExtensions.WrapInFloat(this.regenChainsawPMhWithFellerBuncherAndGrappleYoader),
                ArrowArrayExtensions.WrapInUInt8(this.regenChainsawCrewWithTrackedHarvester),
                ArrowArrayExtensions.WrapInFloat(this.regenChainsawUtilizationWithTrackedHarvester),
                ArrowArrayExtensions.WrapInFloat(this.regenChainsawCmhWithTrackedHarvester),
                ArrowArrayExtensions.WrapInFloat(this.regenChainsawPMhWithTrackedHarvester),
                ArrowArrayExtensions.WrapInUInt8(this.regenChainsawCrewWithWheeledHarvester),
                ArrowArrayExtensions.WrapInFloat(this.regenChainsawUtilizationWithWheeledHarvester),
                ArrowArrayExtensions.WrapInFloat(this.regenChainsawCmhWithWheeledHarvester),
                ArrowArrayExtensions.WrapInFloat(this.regenChainsawPMhWithWheeledHarvester),
                ArrowArrayExtensions.WrapInFloat(this.regenGrappleSwingYarderPMhPerHectare),
                ArrowArrayExtensions.WrapInFloat(this.regenGrappleSwingYarderProductivity),
                ArrowArrayExtensions.WrapInFloat(this.regenGrappleSwingYarderOverweightFirstLogsPerHectare),
                ArrowArrayExtensions.WrapInFloat(this.regenGrappleYoaderPMhPerHectare),
                ArrowArrayExtensions.WrapInFloat(this.regenGrappleYoaderProductivity),
                ArrowArrayExtensions.WrapInFloat(this.regenGrappleYoaderOverweightFirstLogsPerHectare),
                ArrowArrayExtensions.WrapInFloat(this.regenProcessorPMhWithGrappleSwingYarder),
                ArrowArrayExtensions.WrapInFloat(this.regenProcessorProductivityWithGrappleSwingYarder),
                ArrowArrayExtensions.WrapInFloat(this.regenProcessorPMhWithGrappleYoader),
                ArrowArrayExtensions.WrapInFloat(this.regenProcessorProductivityWithGrappleYoader),
                ArrowArrayExtensions.WrapInFloat(this.regenLoadedWeight)
            };

            this.RecordBatches.Add(new(this.Schema, arrowArrays, capacityInRecords));
        }

        private static Schema CreateSchema()
        {
            // create schema
            List<Field> fields = new()
            {
                new("stand", UInt32Type.Default, false),
                new("thin1", Int16Type.Default, false),
                new("thin2", Int16Type.Default, false),
                new("thin3", Int16Type.Default, false),
                new("rotation", Int16Type.Default, false),
                new("financialScenario", UInt32Type.Default, false),
                new("year", Int16Type.Default, false),
                new("standAge", Int16Type.Default, false),
                // tree growth
                new("TPH", FloatType.Default, false),
                new("QMD", FloatType.Default, false),
                new("Htop", FloatType.Default, false),
                new("BA", FloatType.Default, false),
                new("SDI", FloatType.Default, false),
                new("standingCmh", FloatType.Default, false),
                new("standingMbfh", FloatType.Default, false),
                new("thinCmh", FloatType.Default, false),
                new("thinMbfh", FloatType.Default, false),
                new("BAremoved", FloatType.Default, false),
                new("BAintensity", FloatType.Default, false),
                new("TPHdecrease", FloatType.Default, false),
                // financial
                new("NPV", FloatType.Default, false),
                new("LEV", FloatType.Default, false),
                // carbon - not supported for now
                //new("liveTreeBiomass", FloatType.Default, false),
                //new("SPH", FloatType.Default, false),
                //new("snagQMD", FloatType.Default, false),
                // harvest cost
                new("thinMinCostSystem", UInt8Type.Default, false),
                new("thinFallerGrappleSwingYarderCost", FloatType.Default, false),
                new("thinFallerGrappleYoaderCost", FloatType.Default, false),
                new("thinFellerBuncherGrappleSwingYarderCost", FloatType.Default, false),
                new("thinFellerBuncherGrappleYoaderCost", FloatType.Default, false),
                new("thinTrackedHarvesterForwarderCost", FloatType.Default, false),
                new("thinTrackedHarvesterGrappleSwingYarderCost", FloatType.Default, false),
                new("thinTrackedHarvesterGrappleYoaderCost", FloatType.Default, false),
                new("thinWheeledHarvesterForwarderCost", FloatType.Default, false),
                new("thinWheeledHarvesterGrappleSwingYarderCost", FloatType.Default, false),
                new("thinWheeledHarvesterGrappleYoaderCost", FloatType.Default, false),
                new("thinTaskCost", FloatType.Default, false),
                new("regenMinCostSystem", UInt8Type.Default, false),
                new("regenFallerGrappleSwingYarderCost", FloatType.Default, false),
                new("regenFallerGrappleYoaderCost", FloatType.Default, false),
                new("regenFellerBuncherGrappleSwingYarderCost", FloatType.Default, false),
                new("regenFellerBuncherGrappleYoaderCost", FloatType.Default, false),
                new("regenTrackedHarvesterGrappleSwingYarderCost", FloatType.Default, false),
                new("regenTrackedHarvesterGrappleYoaderCost", FloatType.Default, false),
                new("regenWheeledHarvesterGrappleSwingYarderCost", FloatType.Default, false),
                new("regenWheeledHarvesterGrappleYoaderCost", FloatType.Default, false),
                new("regenTaskCost", FloatType.Default, false),
                new("reforestationNpv", FloatType.Default, false),
                // timber sorts
                new("thinLogs2S", FloatType.Default, false),
                new("thinLogs3S", FloatType.Default, false),
                new("thinLogs4S", FloatType.Default, false),
                new("thinCmh2S", FloatType.Default, false),
                new("thinCmh3S", FloatType.Default, false),
                new("thinCmh4S", FloatType.Default, false),
                new("thinMbfh2S", FloatType.Default, false),
                new("thinMbfh3S", FloatType.Default, false),
                new("thinMbfh4S", FloatType.Default, false),
                new("thinPond2S", FloatType.Default, false),
                new("thinPond3S", FloatType.Default, false),
                new("thinPond4S", FloatType.Default, false),
                new("standingLogs2S", FloatType.Default, false),
                new("standingLogs3S", FloatType.Default, false),
                new("standingLogs4S", FloatType.Default, false),
                new("standingCmh2S", FloatType.Default, false),
                new("standingCmh3S", FloatType.Default, false),
                new("standingCmh4S", FloatType.Default, false),
                new("standingMbfh2S", FloatType.Default, false),
                new("standingMbfh3S", FloatType.Default, false),
                new("standingMbfh4S", FloatType.Default, false),
                new("regenPond2S", FloatType.Default, false),
                new("regenPond3S", FloatType.Default, false),
                new("regenPond4S", FloatType.Default, false),
                // equipment productivity
                new("thinFallerPMh", FloatType.Default, false),
                new("thinFallerProductivity", FloatType.Default, false),
                new("thinFellerBuncherPMh", FloatType.Default, false),
                new("thinFellerBuncherProductivity", FloatType.Default, false),
                new("thinTrackedHarvesterPMh", FloatType.Default, false),
                new("thinTrackedHarvesterProductivity", FloatType.Default, false),
                new("thinWheeledHarvesterPMh", FloatType.Default, false),
                new("thinWheeledHarvesterProductivity", FloatType.Default, false),
                new("thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder", UInt8Type.Default, false),
                new("thinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder", FloatType.Default, false),
                new("thinChainsawCmhWithFellerBuncherAndGrappleSwingYarder", FloatType.Default, false),
                new("thinChainsawPMhWithFellerBuncherAndGrappleSwingYarder", FloatType.Default, false),
                new("thinChainsawCrewWithFellerBuncherAndGrappleYoader", UInt8Type.Default, false),
                new("thinChainsawUtilizationWithFellerBuncherAndGrappleYoader", FloatType.Default, false),
                new("thinChainsawCmhWithFellerBuncherAndGrappleYoader", FloatType.Default, false),
                new("thinChainsawPMhWithFellerBuncherAndGrappleYoader", FloatType.Default, false),
                new("thinChainsawCrewWithTrackedHarvester", UInt8Type.Default, false),
                new("thinChainsawUtilizationWithTrackedHarvester", FloatType.Default, false),
                new("thinChainsawCmhWithTrackedHarvester", FloatType.Default, false),
                new("thinChainsawPMhWithTrackedHarvester", FloatType.Default, false),
                new("thinChainsawCrewWithWheeledHarvester", UInt8Type.Default, false),
                new("thinChainsawUtilizationWithWheeledHarvester", FloatType.Default, false),
                new("thinChainsawCmhWithWheeledHarvester", FloatType.Default, false),
                new("thinChainsawPMhWithWheeledHarvester", FloatType.Default, false),
                new("thinForwardingMethod", UInt8Type.Default, false),
                new("thinForwarderPMh", FloatType.Default, false),
                new("thinForwarderProductivity", FloatType.Default, false),
                new("thinForwardedWeight", FloatType.Default, false),
                new("thinGrappleSwingYarderPMhPerHectare", FloatType.Default, false),
                new("thinGrappleSwingYarderProductivity", FloatType.Default, false),
                new("thinGrappleSwingYarderOverweightFirstLogsPerHectare", FloatType.Default, false),
                new("thinGrappleYoaderPMhPerHectare", FloatType.Default, false),
                new("thinGrappleYoaderProductivity", FloatType.Default, false),
                new("thinGrappleYoaderOverweightFirstLogsPerHectare", FloatType.Default, false),
                new("thinProcessorPMhWithGrappleSwingYarder", FloatType.Default, false),
                new("thinProcessorProductivityWithGrappleSwingYarder", FloatType.Default, false),
                new("thinProcessorPMhWithGrappleYoader", FloatType.Default, false),
                new("thinProcessorProductivityWithGrappleYoader", FloatType.Default, false),
                new("thinLoadedWeight", FloatType.Default, false),
                new("regenFallerPMh", FloatType.Default, false),
                new("regenFallerProductivity", FloatType.Default, false),
                new("regenFellerBuncherPMh", FloatType.Default, false),
                new("regenFellerBuncherProductivity", FloatType.Default, false),
                new("regenTrackedHarvesterPMh", FloatType.Default, false),
                new("regenTrackedHarvesterProductivity", FloatType.Default, false),
                new("regenWheeledHarvesterPMh", FloatType.Default, false),
                new("regenWheeledHarvesterProductivity", FloatType.Default, false),
                new("regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder", UInt8Type.Default, false),
                new("regenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder", FloatType.Default, false),
                new("regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder", FloatType.Default, false),
                new("regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder", FloatType.Default, false),
                new("regenChainsawCrewWithFellerBuncherAndGrappleYoader", UInt8Type.Default, false),
                new("regenChainsawUtilizationWithFellerBuncherAndGrappleYoader", FloatType.Default, false),
                new("regenChainsawCmhWithFellerBuncherAndGrappleYoader", FloatType.Default, false),
                new("regenChainsawPMhWithFellerBuncherAndGrappleYoader", FloatType.Default, false),
                new("regenChainsawCrewWithTrackedHarvester", UInt8Type.Default, false),
                new("regenChainsawUtilizationWithTrackedHarvester", FloatType.Default, false),
                new("regenChainsawCmhWithTrackedHarvester", FloatType.Default, false),
                new("regenChainsawPMhWithTrackedHarvester", FloatType.Default, false),
                new("regenChainsawCrewWithWheeledHarvester", UInt8Type.Default, false),
                new("regenChainsawUtilizationWithWheeledHarvester", FloatType.Default, false),
                new("regenChainsawCmhWithWheeledHarvester", FloatType.Default, false),
                new("regenChainsawPMhWithWheeledHarvester", FloatType.Default, false),
                new("regenGrappleSwingYarderPMhPerHectare", FloatType.Default, false),
                new("regenGrappleSwingYarderProductivity", FloatType.Default, false),
                new("regenGrappleSwingYarderOverweightFirstLogsPerHectare", FloatType.Default, false),
                new("regenGrappleYoaderPMhPerHectare", FloatType.Default, false),
                new("regenGrappleYoaderProductivity", FloatType.Default, false),
                new("regenGrappleYoaderOverweightFirstLogsPerHectare", FloatType.Default, false),
                new("regenProcessorPMhWithGrappleSwingYarder", FloatType.Default, false),
                new("regenProcessorProductivityWithGrappleSwingYarder", FloatType.Default, false),
                new("regenProcessorPMhWithGrappleYoader", FloatType.Default, false),
                new("regenProcessorProductivityWithGrappleYoader", FloatType.Default, false),
                new("regenLoadedWeight", FloatType.Default, false)
            };

            Dictionary<string, string> metadata = new()
            {
                // always on
                { "stand", "stand ID" },
                { "thin1", "age of first thin in years or -1 for no thin" },
                { "thin2", "age of second thin in years or -1 for no thin" },
                { "thin3", "age of third thin in years or -1 for no thin" },
                { "rotation", "rotation age in years" },
                { "financialScenario", "index of financial scenario used in cost calculations" },
                { "year", "calendar year, CE" },
                { "standAge", "nominal age of dominant and codominant trees in stand, years" },
                // tree growth
                { "TPH", "trees per hectare" },
                { "QMD", "quadratic mean diameter, cm" },
                { "Htop", "H100, m" },
                { "BA", "basal area, m² ha⁻¹" },
                { "SDI", "Reineke SDI" },
                { "standingCmh", "standing merchantable cubic volume, BC Firmwood m³ ha⁻¹" },
                { "standingMbfh", "standing merchantable board foot volume in 12.2 m (40 foot) logs to a 12.7 cm (5 inch) top, Scribner.C MBF ha⁻¹" },
                { "thinCmh", "merchantable volume removed in thinning, BC Firmwood m³ ha⁻¹" },
                { "thinMbfh", "merchantable volume removed in thinning, Scribner.C MBF ha⁻¹" },
                { "BAremoved", "basal area removed in thinning, m² ha⁻¹" },
                { "BAintensity", "fraction of previous timestep’s basal area removed in thinning" },
                { "TPHdecrease", "total decrease in trees per hectare from thinning and mortality" },
                // financial
                { "NPV", "the net present value of a thin at this time step, if one occurs otherwise, the net present value of a harvest rotation at the stand age, US$ ha⁻¹" },
                { "LEV", "land expectation value of harvest rotation at stand age, US$ ha⁻¹" },
                // carbon - not supported for now
                //{ "liveTreeBiomass", "biomass of live trees, kg ha⁻¹" },
                //{ "SPH", "snags per hectare" },
                //{ "snagQMD", "quadratic mean diameter of snags, cm" },
                // harvest cost
                { "thinMinCostSystem", "enum indicating lowest cost system for thinning" },
                { "thinFallerGrappleSwingYarderCost", "total felling and stump to mill extraction cost for thin if using hand falling, a swing yarder with whole tree grapple yarding, processor, and loader, US$ ha⁻¹" },
                { "thinFallerGrappleYoaderCost", "total felling and stump to mill extraction cost for thin if using hand falling, a yoader with whole tree grapple yarding, processor, and loader, US$ ha⁻¹" },
                { "thinFellerBuncherGrappleSwingYarderCost", "total felling and stump to mill extraction cost for thin if using a (tethered) feller-buncher, swing yarder with whole tree grapple yarding, processor, and loader, US$ ha⁻¹" },
                { "thinFellerBuncherGrappleYoaderCost", "total felling and stump to mill extraction cost for thin if using a (tethered) feller-buncher, yoader with whole tree grapple yarding, processor, and loader, US$ ha⁻¹" },
                { "thinTrackedHarvesterForwarderCost", "total felling and stump to mill extraction cost for thin if using (tethered) tracked harvester and forwarder, US$ ha⁻¹" },
                { "thinTrackedHarvesterGrappleSwingYarderCost", "total felling and stump to mill extraction cost for thin if using a (tethered) tracked harvester, swing yarder with long log grapple yarding, and loader, US$ ha⁻¹" },
                { "thinTrackedHarvesterGrappleYoaderCost", "total felling and stump to mill extraction cost for thin if using a (tethered) tracked harvester, yoader with long log grapple yarding, and loader, US$ ha⁻¹" },
                { "thinWheeledHarvesterForwarderCost", "total felling and stump to mill extraction cost for thin if using (tethered) wheeled harvester and forwarder, US$ ha⁻¹" },
                { "thinWheeledHarvesterGrappleSwingYarderCost", "total felling and stump to mill extraction cost for thin if using a (tethered) eight-wheel harvester, swing yarder with long log grapple yarding, and loader, US$ ha⁻¹" },
                { "thinWheeledHarvesterGrappleYoaderCost", "total felling and stump to mill extraction cost for thin if using a (tethered) eight-wheel harvester, yoader with long log grapple yarding, and loader, US$ ha⁻¹" },
                { "thinTaskCost", "cost of slash disposal, road maintenance, and other harvest related tasks for thinning, US$ ha⁻¹" },
                { "regenMinCostSystem", "enum indicating lowest cost system for regeneration harvest" },
                { "regenFallerGrappleSwingYarderCost", "total felling and stump to mill extraction cost for regeneration harvest if using hand falling, a swing yarder with whole tree grapple yarding, processor, and loader, US$ ha⁻¹" },
                { "regenFallerGrappleYoaderCost", "total felling and stump to mill extraction cost for regeneration harvest if using hand falling, a yoader with whole tree grapple yarding, processor, and loader, US$ ha⁻¹" },
                { "regenFellerBuncherGrappleSwingYarderCost", "total felling and stump to mill extraction cost using a (tethered) feller-buncher, swing yarder with whole tree grapple yarding, processor, and loader, US$ ha⁻¹" },
                { "regenFellerBuncherGrappleYoaderCost", "total felling and stump to mill extraction cost using a (tethered) feller-buncher, yoader with whole tree grapple yarding, processor, and loader, US$ ha⁻¹" },
                { "regenTrackedHarvesterGrappleSwingYarderCost", "total felling and stump to mill extraction cost using a (tethered) tracked harvester, swing yarder with long log grapple yarding, and loader, US$ ha⁻¹" },
                { "regenTrackedHarvesterGrappleYoaderCost", "total felling and stump to mill extraction cost using a (tethered) tracked harvester, yoader with long log grapple yarding, and loader, US$ ha⁻¹" },
                { "regenWheeledHarvesterGrappleSwingYarderCost", "total felling and stump to mill extraction cost using a (tethered) wheeled harvester, swing yarder with long log grapple yarding, and loader, US$ ha⁻¹" },
                { "regenWheeledHarvesterGrappleYoaderCost", "total felling and stump to mill extraction cost using a (tethered) wheeled harvester, yoader with long log grapple yarding, and loader, US$ ha⁻¹" },
                { "regenTaskCost", "cost of slash disposal, road maintenance, reforestation, and other harvest related tasks for thinning, US$ ha⁻¹" },
                { "reforestationNpv", "net present value of reforestation if a regeneration harvest is performed at this timestep, US$ ha⁻¹" },
                // timber sorts
                { "thinLogs2S", "2 saw logs cut during thinning by sort (2, 3, or 4 saw), logs ha⁻¹" },
                { "thinLogs3S", "3 saw logs cut during thinning by sort (2, 3, or 4 saw), logs ha⁻¹" },
                { "thinLogs4S", "4 saw logs cut during thinning by sort (2, 3, or 4 saw), logs ha⁻¹" },
                { "thinCmh2S", "merchantable 2 saw volume thinned by sort, BC Firmwood m³ ha⁻¹" },
                { "thinCmh3S", "merchantable 3 saw volume thinned by sort, BC Firmwood m³ ha⁻¹" },
                { "thinCmh4S", "merchantable 4 saw volume thinned by sort, BC Firmwood m³ ha⁻¹" },
                { "thinMbfh2S", "merchantable 2 saw volume thinned by sort, Scribner.C MBF ha-1" },
                { "thinMbfh3S", "merchantable 3 saw volume thinned by sort, Scribner.C MBF ha-1" },
                { "thinMbfh4S", "merchantable 4 saw volume thinned by sort, Scribner.C MBF ha-1" },
                { "thinPond2S", "pond value of 2 saw logs cut during thinning by sort, US$ ha⁻¹" },
                { "thinPond3S", "pond value of 3 saw logs cut during thinning by sort, US$ ha⁻¹" },
                { "thinPond4S", "pond value of 4 saw logs cut during thinning by sort, US$ ha⁻¹" },
                { "standingLogs2S", "12.2 m 2 saw logs buckable from standing trees by sort, logs ha⁻¹" },
                { "standingLogs3S", "12.2 m 3 saw logs buckable from standing trees by sort, logs ha⁻¹" },
                { "standingLogs4S", "12.2 m 4 saw logs buckable from standing trees by sort, logs ha⁻¹" },
                { "standingCmh2S", "merchantable volume of 12.2 m 2 saw logs in standing trees by sort, BC Firmwood m³ ha⁻¹" },
                { "standingCmh3S", "merchantable volume of 12.2 m 3 saw logs in standing trees by sort, BC Firmwood m³ ha⁻¹" },
                { "standingCmh4S", "merchantable volume of 12.2 m 4 saw logs in standing trees by sort, BC Firmwood m³ ha⁻¹" },
                { "standingMbfh2S", "merchantable volume of 12.2 m 2 saw logs in standing trees by sort, Scribner.C MBF ha⁻¹" },
                { "standingMbfh3S", "merchantable volume of 12.2 m 3 saw logs in standing trees by sort, Scribner.C MBF ha⁻¹" },
                { "standingMbfh4S", "merchantable volume of 12.2 m 4 saw logs in standing trees by sort, Scribner.C MBF ha⁻¹" },
                { "regenPond2S", "pond value of standing 12.2 m 2 saw logs, US$ ha⁻¹" },
                { "regenPond3S", "pond value of standing 12.2 m 3 saw logs, US$ ha⁻¹" },
                { "regenPond4S", "pond value of standing 12.2 m 4 saw logs, US$ ha⁻¹" },
                // equipment productivity
                { "thinFallerPMh", "productive “machine” hours for hand fallers to perform felling and any needed bucking at the stump during thinning, PMh₀ ha⁻¹" },
                { "thinFallerProductivity", "productivity of fallers during thinning, m³ PMh₀⁻¹ ha⁻¹" },
                { "thinFellerBuncherPMh", "productive machine hours for a (tethered) feller-buncher to perform felling during thinning, PMh₀ ha⁻¹" },
                { "thinFellerBuncherProductivity", "productivity of feller-buncher during thinning, m³ PMh₀⁻¹ ha⁻¹" },
                { "thinTrackedHarvesterPMh", "productive machine hours for a (tethered) tracked harvester to perform felling and bucking during thinning, PMh₀ ha⁻¹" },
                { "thinTrackedHarvesterProductivity", "productivity of tracked harvester during thinning, m³ PMh₀⁻¹ ha⁻¹" },
                { "thinWheeledHarvesterPMh", "productive machine hours for a (tethered) eight-wheel harvester to perform felling and bucking during thinning, PMh₀ ha⁻¹" },
                { "thinWheeledHarvesterProductivity", "productivity of eight-wheel harvester during thinning, m³ PMh₀⁻¹ ha⁻¹" },
                { "thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder", "enum indicating lowest cost chainsaw crew type to use with a feller-buncher, swing yarder, processor, and loader system" },
                { "thinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder", "fraction of time chainsaw crew spends felling and/or bucking with short moves to the next tree versus making longer moves across the unit with a feller-buncher, swing yarder, processor, and loader system" },
                { "thinChainsawCmhWithFellerBuncherAndGrappleSwingYarder", "merchantable volume felled and/or bucked by chainsaw crew following a feller-buncher and bucking for a swing yarder, m³ ha⁻¹" },
                { "thinChainsawPMhWithFellerBuncherAndGrappleSwingYarder", "productive “machine” hours put in by chainsaw crew following a feller-buncher and bucking for a swing yarder, PMh₀⁻¹ ha⁻¹" },
                { "thinChainsawCrewWithFellerBuncherAndGrappleYoader", "enum indicating lowest cost chainsaw crew type to use with a feller-buncher, yoader, processor, and loader system" },
                { "thinChainsawUtilizationWithFellerBuncherAndGrappleYoader", "fraction of time chainsaw crew spends felling and/or bucking with short moves to the next tree versus making longer moves across the unit with a feller-buncher, yoader, processor, and loader system" },
                { "thinChainsawCmhWithFellerBuncherAndGrappleYoader", "merchantable volume felled and/or bucked by chainsaw crew following a feller-buncher and bucking for a yoader, m³ ha⁻¹" },
                { "thinChainsawPMhWithFellerBuncherAndGrappleYoader", "productive “machine” hours put in by chainsaw crew following a feller-buncher and bucking for a yoader, PMh₀⁻¹ ha⁻¹" },
                { "thinChainsawCrewWithTrackedHarvester", "enum indicating lowest cost chainsaw crew type to use with a tracked harvester" },
                { "thinChainsawUtilizationWithTrackedHarvester", "fraction of time chainsaw crew spends felling and/or bucking with short moves to the next tree versus making longer moves across the unit when supporting a tracked harvester" },
                { "thinChainsawCmhWithTrackedHarvester", "merchantable volume felled and/or bucked by chainsaw crew supporting a tracked harvester, m³ ha⁻¹" },
                { "thinChainsawPMhWithTrackedHarvester", "productive “machine” hours put in by chainsaw crew supporting an eight-wheel harvester, PMh₀⁻¹ ha⁻¹" },
                { "thinChainsawCrewWithWheeledHarvester", "enum indicating lowest cost chainsaw crew type to use with an eight-wheel harvester" },
                { "thinChainsawCrewUtilizationWithWheeledHarvester", "fraction of time chainsaw crew spends felling and/or bucking with short moves to the next tree versus making longer moves across the unit when supporting an eight-wheel harvester" },
                { "thinChainsawCmhWithWheeledHarvester", "merchantable volume felled and/or bucked by chainsaw crew supporting an eight-wheel harvester, m³ ha⁻¹" },
                { "thinChainsawPMhWithWheeledHarvester", "productive “machine” hours put in by chainsaw crew supporting an eight-wheel harvester, PMh₀⁻¹ ha⁻¹" },
                { "thinForwardingMethod", "enum indicating optimal loading of 2S, 3S, and 4S sorts on forwarder, currently assumes only a single species is harvested" },
                { "thinForwarderPMh", "productive machine hours for (tethered) wheeled forwarder to move logs from stump to road during thinning, PMh₀ ha⁻¹" },
                { "thinForwarderProductivity", "productivity of wheeled forwarder during thinning, m³ PMh₀⁻¹ ha⁻¹" },
                { "thinForwardedWeight", "total green weight of logs and retained bark forwarded to road, kg ha⁻¹" },
                { "thinGrappleSwingYarderPMhPerHectare", "productive machine hours for a swing yarder to grapple logs from stump to chute during thinning, PMh₀ ha⁻¹" },
                { "thinGrappleSwingYarderProductivity", "productivity of grapple swing yarder during thinning, m³ PMh₀⁻¹ ha⁻¹" },
                { "thinGrappleSwingYarderOverweightFirstLogsPerHectare", "number of first logs which must be bucked off to avoid a yarding whole tree weighting more than a swing yarder’s payload limit, logs ha⁻¹ (or, equivalently, trees per hectare)" },
                { "thinGrappleYoaderPMhPerHectare", "productive machine hours for a yoader to grapple logs from stump to chute during thinning, PMh₀ ha⁻¹" },
                { "thinGrappleYoaderProductivity", "productivity of yoader during thinning, m³ PMh₀⁻¹ ha⁻¹" },
                { "thinGrappleYoaderOverweightFirstLogsPerHectare", "number of first logs which must be bucked off to avoid a yarding whole tree weighting more than a yoader’s payload limit, logs ha⁻¹ (or, equivalently, trees per hectare)" },
                { "thinProcessorPMhWithGrappleSwingYarder", "productive machine hours for a processor following a swing yarder during thinning, PMh₀ ha⁻¹" },
                { "thinProcessorProductivityWithGrappleSwingYarder", "productivity of processor paired with a swing yarder during thinning, m³ PMh₀⁻¹ ha⁻¹" },
                { "thinProcessorPMhWithGrappleYoader", "productive machine hours for a processor following a yoader during thinning, PMh₀ ha⁻¹" },
                { "thinProcessorProductivityWithGrappleYoader", "productivity of processor paired with a yoader during thinning, m³ PMh₀⁻¹ ha⁻¹" },
                { "thinLoadedWeight", "total green weight of cut to length logs loaded onto a mule train (harvester-forwarder thins) or long logs loaded onto a truck (yarded thins), kg ha⁻¹" },
                { "regenFallerPMh", "productive “machine” hours for hand fallers to perform felling and any needed bucking at the stump during regeneration harvest, PMh₀ ha⁻¹" },
                { "regenFallerProductivity", "productivity of fallers during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹" },
                { "regenFellerBuncherPMh", "productive machine hours for (tethered) feller-buncher to perform felling during regeneration harvest, PMh₀ ha⁻¹" },
                { "regenFellerBuncherProductivity", "productivity of feller-buncher during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹" },
                { "regenTrackedHarvesterPMh", "productive machine hours for (tethered) tracked harvester to perform felling and bucking during regeneration harvest, PMh₀ ha⁻¹" },
                { "regenTrackedHarvesterProductivity", "productivity of tracked harvester during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹" },
                { "regenWheeledHarvesterPMh", "productive machine hours for (tethered) eight-wheel wheel harvester to perform felling and bucking during regeneration harvest, PMh₀ ha⁻¹" },
                { "regenWheeledHarvesterProductivity", "productivity of eight-wheel harvester during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹" },
                { "regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder", "most cost of effective type of chainsaw crew to support a feller-buncher in bucking logs to meet a grapple swing yarder’s payload capability" },
                { "regenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder", "fraction of time chainsaw crew spends in wood production, as opposed to hiking to the next tree, when using a feller-buncher and swing yarder system, SMh SMh⁻¹" },
                { "regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder", "total merchantable wood volume of trees processed, at least partially, by chainsaw during regeneration harvest when using a feller-buncher and swing yarder system, m³ ha⁻¹" },
                { "regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder", "productive machine hours put in by chainsaw crew when using a feller-buncher and swing yarder system, PMh₀ ha⁻¹" },
                { "regenChainsawCrewWithFellerBuncherAndGrappleYoader", "most cost of effective type of chainsaw crew to support a feller-buncher in bucking logs to meet a grapple yoader’s payload capability" },
                { "regenChainsawUtilizationWithFellerBuncherAndGrappleYoader", "fraction of time chainsaw crew spends in wood production, as opposed to hiking to the next tree, when using a feller-buncher and yoader system, SMh SMh⁻¹" },
                { "regenChainsawCmhWithFellerBuncherAndGrappleYoader", "total merchantable wood volume of trees processed, at least partially, by chainsaw during regeneration harvest when using a feller-buncher and yoader system, m³ ha⁻¹" },
                { "regenChainsawPMhWithFellerBuncherAndGrappleYoader", "productive machine hours put in by chainsaw crew when using a feller-buncher and yoader system, PMh₀ ha⁻¹" },
                { "regenChainsawCrewWithTrackedHarvester", "most cost of effective type of chainsaw crew to support a tracked harvester in felling trees to meet a grapple yoader’s payload capability" },
                { "regenChainsawUtilizationWithTrackedHarvester", "fraction of time chainsaw crew spends in wood production, as opposed to hiking to the next tree, when using a tracked harvester, SMh SMh⁻¹" },
                { "regenChainsawCmhWithTrackedHarvester", "total merchantable wood volume of trees processed, at least partially, by chainsaw during regeneration harvest when using a tracked harvester, m³ ha⁻¹" },
                { "regenChainsawPMhWithTrackedHarvester", "productive machine hours put in by chainsaw crew when using a tracked harvester, PMh₀ ha⁻¹" },
                { "regenChainsawCrewWithWheeledHarvester", "most cost of effective type of chainsaw crew to support an eight-wheel harvester in felling trees to meet a grapple yoader’s payload capability" },
                { "regenChainsawUtilizationWithWheeledHarvester", "fraction of time chainsaw crew spends in wood production, as opposed to hiking to the next tree, when using an eight-wheel harvester, SMh SMh⁻¹" },
                { "regenChainsawCmhWithWheeledHarvester", "total merchantable wood volume of trees processed, at least partially, by chainsaw during regeneration harvest when using an eight-wheel harvester, m³ ha⁻¹" },
                { "regenChainsawPMhWithWheeledHarvester", "productive machine hours put in by chainsaw crew when using an eight wheeled harvester, PMh₀ ha⁻¹" },
                { "regenGrappleSwingYarderPMhPerHectare", "productive machine hours for a swing yarder grappling whole trees or bucked logs regeneration harvest, PMh₀ ha⁻¹" },
                { "regenGrappleSwingYarderProductivity", "productivity of swing yarder during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹" },
                { "regenGrappleSwingYarderOverweightFirstLogsPerHectare", "number of first logs which must be bucked off to avoid a yarding whole tree weighting more than a swing yarder’s payload limit, logs ha⁻¹ (or, equivalently, trees per hectare)" },
                { "regenGrappleYoaderPMhPerHectare", "productive machine hours for a yoader grappling whole trees or bucked logs during regeneration harvest, PMh₀ ha⁻¹" },
                { "regenGrappleYoaderProductivity", "productivity of yoader during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹" },
                { "regenGrappleYoaderOverweightFirstLogsPerHectare", "number of first logs which must be bucked off to avoid a yarding whole tree weighting more than a yoader’s payload limit, logs ha⁻¹ (or, equivalently, trees per hectare)" },
                { "regenProcessorPMhWithGrappleSwingYarder", "productive machine hours for a processor following a swing yarder during regeneration harvest, PMh₀ ha⁻¹" },
                { "regenProcessorProductivityWithGrappleSwingYarder", "productivity of processor paired with a swing yarder during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹" },
                { "regenProcessorPMhWithGrappleYoader", "productive machine hours for a processor following a yoader during regeneration harvest, PMh₀ ha⁻¹" },
                { "regenProcessorProductivityWithGrappleYoader", "productivity of processor paired with a yoader during regeneration harvest, m³ PMh₀⁻¹ ha⁻¹" },
                { "regenLoadedWeight", "total weight of logs and retained bark loaded on log trucks during regeneration harvest, kg ha⁻¹" }
            };

            return new Schema(fields, metadata);
        }
    }
}

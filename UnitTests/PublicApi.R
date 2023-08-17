library(arrow)
library(dplyr)
library(readr)
library(tidyr)

as_chainsaw_crew = function(column)
{
  return(factor(column, labels = c("None", "Bucker", "Fallers", "Operator"), levels = seq(0, 3)))
}

as_forwarding_method = function(column)
{
  return(factor(column, labels = c("None", "AllSortsCombined", "AllSortsSeparate", "TwoFourSCombined"), levels = seq(0, 3)))
}

as_harvest_system = function(column)
{
  return(factor(column, labels = c("None", "FallersGrappleSwingYarderProcessorLoader", "FallersGrappleYoaderProcessorLoader", "FellerBuncherGrappleSwingYarderProcessorLoader", "FellerBuncherGrappleYoaderProcessorLoader", "TrackedHarvesterForwarder", "TrackedHarvesterGrappleSwingYarderLoader", "TrackedHarvesterGrappleYoaderLoader", "WheeledHarvesterForwarder", "WheeledHarvesterGrappleSwingYarderLoader", "WheeledHarvesterGrappleYoaderLoader"), levels = seq(0, 10)))
}

get_nrmse = function(columnCsv, columnFeather)
{
  # check for case where .csv is all NA and .feather is all NaN
  if ((length(columnCsv) == length(columnFeather)) & (sum(is.na(columnCsv)) == length(columnCsv)) & (sum(is.nan(columnFeather)) == length(columnFeather)))
  {
    return(0)
  }
  # otherwise do numerical diff when columns aren't both zero or NaN
  return(sqrt(mean(if_else(((columnCsv == 0) & (columnFeather == 0)) | (is.nan(columnCsv) & is.nan(columnFeather)), 0, (columnCsv - columnFeather)^2 / (0.5 * abs(columnCsv + columnFeather))))))
}

# load any set of identical trajectories written to both .csv and .feather and diff the two
# Files should be identical within 1) .csv truncation error and 2) feather enum and string dictionary encoding differences.
trajectoriesCsv = read_csv("../Elliott/trees/Organon/Elliott stand trajectories 2016-2116.csv", col_types = cols(.default = "d", financialScenario = "c", thinMinCostSystem  = "c", regenMinCostSystem = "c", thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder = "c", thinChainsawCrewWithFellerBuncherAndGrappleYoader = "c", thinChainsawCrewWithTrackedHarvester = "c", thinChainsawCrewWithWheeledHarvester = "c", thinForwardingMethod = "c", regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder = "c", regenChainsawCrewWithFellerBuncherAndGrappleYoader = "c", regenChainsawCrewWithTrackedHarvester = "c", regenChainsawCrewWithWheeledHarvester = "c"))
trajectoriesFeather = read_feather("../Elliott/trees/Organon/Elliott stand trajectories 2016-2116.feather", mmap = FALSE)

csvFeatherDiff = tibble(stand = sum(trajectoriesCsv$stand != trajectoriesFeather$stand),
                        financialScenariosCsv = unique(trajectoriesCsv$financialScenario),
                        financialScenariosFeather = unique(trajectoriesFeather$financialScenario),
                        year = sum(trajectoriesCsv$year != trajectoriesFeather$year),
                        standAge = sum(trajectoriesCsv$standAge != trajectoriesFeather$standAge),
                        tph = get_nrmse(trajectoriesCsv$TPH, trajectoriesFeather$TPH),
                        qmd = get_nrmse(trajectoriesCsv$QMD, trajectoriesFeather$QMD),
                        hTop = get_nrmse(trajectoriesCsv$Htop, trajectoriesFeather$Htop),
                        basalArea = get_nrmse(trajectoriesCsv$BA, trajectoriesFeather$BA),
                        sdi = get_nrmse(trajectoriesCsv$SDI, trajectoriesFeather$SDI),
                        standingCmh = get_nrmse(trajectoriesCsv$standingCmh, trajectoriesFeather$standingCmh),
                        standingMbfh = get_nrmse(trajectoriesCsv$standingMbfh, trajectoriesFeather$standingMbfh),
                        thinCmh = get_nrmse(trajectoriesCsv$thinCmh, trajectoriesFeather$thinCmh),
                        thinMbfh = get_nrmse(trajectoriesCsv$thinMbfh, trajectoriesFeather$thinMbfh),
                        baRemoved = get_nrmse(trajectoriesCsv$BAremoved, trajectoriesFeather$BAremoved),
                        baIntensity = get_nrmse(trajectoriesCsv$BAintensity, trajectoriesFeather$BAintensity),
                        tphDecrease = get_nrmse(trajectoriesCsv$TPHdecrease, trajectoriesFeather$TPHdecrease),
                        npv = get_nrmse(trajectoriesCsv$NPV, trajectoriesFeather$NPV),
                        lev = get_nrmse(trajectoriesCsv$LEV, trajectoriesFeather$LEV),
                        thinMinCostSystem = sum(replace_na(trajectoriesCsv$thinMinCostSystem, "None") != as_harvest_system(trajectoriesFeather$thinMinCostSystem)),
                        thinFallerGrappleSwingYarderCost = get_nrmse(trajectoriesCsv$thinFallerGrappleSwingYarderCost, trajectoriesFeather$thinFallerGrappleSwingYarderCost),
                        thinFallerGrappleYoaderCost = get_nrmse(trajectoriesCsv$thinFallerGrappleYoaderCost, trajectoriesFeather$thinFallerGrappleYoaderCost),
                        thinFellerBuncherGrappleSwingYarderCost = get_nrmse(trajectoriesCsv$thinFellerBuncherGrappleSwingYarderCost, trajectoriesFeather$thinFellerBuncherGrappleSwingYarderCost),
                        thinFellerBuncherGrappleYoaderCost = get_nrmse(trajectoriesCsv$thinFellerBuncherGrappleYoaderCost, trajectoriesFeather$thinFellerBuncherGrappleYoaderCost),
                        thinTrackedHarvesterForwarderCost = get_nrmse(trajectoriesCsv$thinTrackedHarvesterForwarderCost, trajectoriesFeather$thinTrackedHarvesterForwarderCost),
                        thinTrackedHarvesterGrappleSwingYarderCost = get_nrmse(trajectoriesCsv$thinTrackedHarvesterGrappleSwingYarderCost, trajectoriesFeather$thinTrackedHarvesterGrappleSwingYarderCost),
                        thinTrackedHarvesterGrappleYoaderCost = get_nrmse(trajectoriesCsv$thinTrackedHarvesterGrappleYoaderCost, trajectoriesFeather$thinTrackedHarvesterGrappleYoaderCost),
                        thinWheeledHarvesterForwarderCost = get_nrmse(trajectoriesCsv$thinWheeledHarvesterForwarderCost, trajectoriesFeather$thinWheeledHarvesterForwarderCost),
                        thinWheeledHarvesterGrappleSwingYarderCost = get_nrmse(trajectoriesCsv$thinWheeledHarvesterGrappleSwingYarderCost, trajectoriesFeather$thinWheeledHarvesterGrappleSwingYarderCost),
                        thinWheeledHarvesterGrappleYoaderCost = get_nrmse(trajectoriesCsv$thinWheeledHarvesterGrappleYoaderCost, trajectoriesFeather$thinWheeledHarvesterGrappleYoaderCost),
                        thinTaskCost = get_nrmse(trajectoriesCsv$thinTaskCost, trajectoriesFeather$thinTaskCost),
                        regenMinCostSystem = sum(trajectoriesCsv$regenMinCostSystem != as_harvest_system(trajectoriesFeather$regenMinCostSystem)),
                        regenFallerGrappleSwingYarderCost = get_nrmse(trajectoriesCsv$regenFallerGrappleSwingYarderCost, trajectoriesFeather$regenFallerGrappleSwingYarderCost),
                        regenFallerGrappleYoaderCost = get_nrmse(trajectoriesCsv$regenFallerGrappleYoaderCost, trajectoriesFeather$regenFallerGrappleYoaderCost),
                        regenFellerBuncherGrappleSwingYarderCost = get_nrmse(trajectoriesCsv$regenFellerBuncherGrappleSwingYarderCost, trajectoriesFeather$regenFellerBuncherGrappleSwingYarderCost),
                        regenFellerBuncherGrappleYoaderCost = get_nrmse(trajectoriesCsv$regenFellerBuncherGrappleYoaderCost, trajectoriesFeather$regenFellerBuncherGrappleYoaderCost),
                        regenTrackedHarvesterGrappleSwingYarderCost = get_nrmse(trajectoriesCsv$regenTrackedHarvesterGrappleSwingYarderCost, trajectoriesFeather$regenTrackedHarvesterGrappleSwingYarderCost),
                        regenTrackedHarvesterGrappleYoaderCost = get_nrmse(trajectoriesCsv$regenTrackedHarvesterGrappleYoaderCost, trajectoriesFeather$regenTrackedHarvesterGrappleYoaderCost),
                        regenWheeledHarvesterGrappleSwingYarderCost = get_nrmse(trajectoriesCsv$regenWheeledHarvesterGrappleSwingYarderCost, trajectoriesFeather$regenWheeledHarvesterGrappleSwingYarderCost),
                        regenWheeledHarvesterGrappleYoaderCost = get_nrmse(trajectoriesCsv$regenWheeledHarvesterGrappleYoaderCost, trajectoriesFeather$regenWheeledHarvesterGrappleYoaderCost),
                        regenTaskCost = get_nrmse(trajectoriesCsv$regenTaskCost, trajectoriesFeather$regenTaskCost),
                        reforestationNpv = get_nrmse(trajectoriesCsv$reforestationNpv, trajectoriesFeather$reforestationNpv),
                        thinLogs2S = get_nrmse(trajectoriesCsv$thinLogs2S, trajectoriesFeather$thinLogs2S),
                        thinLogs3S = get_nrmse(trajectoriesCsv$thinLogs3S, trajectoriesFeather$thinLogs3S),
                        thinLogs4S = get_nrmse(trajectoriesCsv$thinLogs4S, trajectoriesFeather$thinLogs4S),
                        thinCmh2S = get_nrmse(trajectoriesCsv$thinCmh2S, trajectoriesFeather$thinCmh2S),
                        thinCmh3S = get_nrmse(trajectoriesCsv$thinCmh3S, trajectoriesFeather$thinCmh3S),
                        thinCmh4S = get_nrmse(trajectoriesCsv$thinCmh4S, trajectoriesFeather$thinCmh4S),
                        thinMbfh2S = get_nrmse(trajectoriesCsv$thinMbfh2S, trajectoriesFeather$thinMbfh2S),
                        thinMbfh3S = get_nrmse(trajectoriesCsv$thinMbfh3S, trajectoriesFeather$thinMbfh3S),
                        thinMbfh4S = get_nrmse(trajectoriesCsv$thinMbfh4S, trajectoriesFeather$thinMbfh4S),
                        thinPond2S = get_nrmse(trajectoriesCsv$thinPond2S, trajectoriesFeather$thinPond2S),
                        thinPond3S = get_nrmse(trajectoriesCsv$thinPond3S, trajectoriesFeather$thinPond3S),
                        thinPond4S = get_nrmse(trajectoriesCsv$thinPond4S, trajectoriesFeather$thinPond4S),
                        standingLogs2S = get_nrmse(trajectoriesCsv$standingLogs2S, trajectoriesFeather$standingLogs2S),
                        standingLogs3S = get_nrmse(trajectoriesCsv$standingLogs3S, trajectoriesFeather$standingLogs3S),
                        standingLogs4S = get_nrmse(trajectoriesCsv$standingLogs4S, trajectoriesFeather$standingLogs4S),
                        standingCmh2S = get_nrmse(trajectoriesCsv$standingCmh2S, trajectoriesFeather$standingCmh2S),
                        standingCmh3S = get_nrmse(trajectoriesCsv$standingCmh3S, trajectoriesFeather$standingCmh3S),
                        standingCmh4S = get_nrmse(trajectoriesCsv$standingCmh4S, trajectoriesFeather$standingCmh4S),
                        standingMbfh2S = get_nrmse(trajectoriesCsv$standingMbfh2S, trajectoriesFeather$standingMbfh2S),
                        standingMbfh3S = get_nrmse(trajectoriesCsv$standingMbfh3S, trajectoriesFeather$standingMbfh3S),
                        standingMbfh4S = get_nrmse(trajectoriesCsv$standingMbfh4S, trajectoriesFeather$standingMbfh4S),
                        regenPond2S = get_nrmse(trajectoriesCsv$regenPond2S, trajectoriesFeather$regenPond2S),
                        regenPond3S = get_nrmse(trajectoriesCsv$regenPond3S, trajectoriesFeather$regenPond3S),
                        regenPond4S = get_nrmse(trajectoriesCsv$regenPond4S, trajectoriesFeather$regenPond4S),
                        thinFallerPMh = get_nrmse(trajectoriesCsv$thinFallerPMh, trajectoriesFeather$thinFallerPMh),
                        thinFallerProductivity = get_nrmse(trajectoriesCsv$thinFallerProductivity, trajectoriesFeather$thinFallerProductivity),
                        thinFellerBuncherPMh = get_nrmse(trajectoriesCsv$thinFellerBuncherPMh, trajectoriesFeather$thinFellerBuncherPMh),
                        thinFellerBuncherProductivity = get_nrmse(trajectoriesCsv$thinFellerBuncherProductivity, trajectoriesFeather$thinFellerBuncherProductivity),
                        thinTrackedHarvesterPMh = get_nrmse(trajectoriesCsv$thinTrackedHarvesterPMh, trajectoriesFeather$thinTrackedHarvesterPMh),
                        thinTrackedHarvesterProductivity = get_nrmse(trajectoriesCsv$thinTrackedHarvesterProductivity, trajectoriesFeather$thinTrackedHarvesterProductivity),
                        thinWheeledHarvesterPMh = get_nrmse(trajectoriesCsv$thinWheeledHarvesterPMh, trajectoriesFeather$thinWheeledHarvesterPMh),
                        thinWheeledHarvesterProductivity = get_nrmse(trajectoriesCsv$thinWheeledHarvesterProductivity, trajectoriesFeather$thinWheeledHarvesterProductivity),
                        thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder = sum(replace_na(trajectoriesCsv$thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder, "None") != as_chainsaw_crew(trajectoriesFeather$thinChainsawCrewWithFellerBuncherAndGrappleSwingYarder)),
                        thinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = get_nrmse(trajectoriesCsv$thinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder, trajectoriesFeather$thinChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder),
                        thinChainsawCmhWithFellerBuncherAndGrappleSwingYarder = get_nrmse(trajectoriesCsv$thinChainsawCmhWithFellerBuncherAndGrappleSwingYarder, trajectoriesFeather$thinChainsawCmhWithFellerBuncherAndGrappleSwingYarder),
                        thinChainsawPMhWithFellerBuncherAndGrappleSwingYarder = get_nrmse(trajectoriesCsv$thinChainsawPMhWithFellerBuncherAndGrappleSwingYarder, trajectoriesFeather$thinChainsawPMhWithFellerBuncherAndGrappleSwingYarder),
                        thinChainsawCrewWithFellerBuncherAndGrappleYoader = sum(replace_na(trajectoriesCsv$thinChainsawCrewWithFellerBuncherAndGrappleYoader, "None") != as_chainsaw_crew(trajectoriesFeather$thinChainsawCrewWithFellerBuncherAndGrappleYoader)),
                        thinChainsawUtilizationWithFellerBuncherAndGrappleYoader = get_nrmse(trajectoriesCsv$thinChainsawUtilizationWithFellerBuncherAndGrappleYoader, trajectoriesFeather$thinChainsawUtilizationWithFellerBuncherAndGrappleYoader),
                        thinChainsawCmhWithFellerBuncherAndGrappleYoader = get_nrmse(trajectoriesCsv$thinChainsawCmhWithFellerBuncherAndGrappleYoader, trajectoriesFeather$thinChainsawCmhWithFellerBuncherAndGrappleYoader),
                        thinChainsawPMhWithFellerBuncherAndGrappleYoader = get_nrmse(trajectoriesCsv$thinChainsawPMhWithFellerBuncherAndGrappleYoader, trajectoriesFeather$thinChainsawPMhWithFellerBuncherAndGrappleYoader),
                        thinChainsawCrewWithTrackedHarvester = sum(replace_na(trajectoriesCsv$thinChainsawCrewWithTrackedHarvester, "None") != as_chainsaw_crew(trajectoriesFeather$thinChainsawCrewWithTrackedHarvester)),
                        thinChainsawUtilizationWithTrackedHarvester = get_nrmse(trajectoriesCsv$thinChainsawUtilizationWithTrackedHarvester, trajectoriesFeather$thinChainsawUtilizationWithTrackedHarvester),
                        thinChainsawCmhWithTrackedHarvester = get_nrmse(trajectoriesCsv$thinChainsawCmhWithTrackedHarvester, trajectoriesFeather$thinChainsawCmhWithTrackedHarvester),
                        thinChainsawPMhWithTrackedHarvester = get_nrmse(trajectoriesCsv$thinChainsawPMhWithTrackedHarvester, trajectoriesFeather$thinChainsawPMhWithTrackedHarvester),
                        thinChainsawCrewWithWheeledHarvester = sum(replace_na(trajectoriesCsv$thinChainsawCrewWithWheeledHarvester, "None") != as_chainsaw_crew(trajectoriesFeather$thinChainsawCrewWithWheeledHarvester)),
                        thinChainsawUtilizationWithWheeledHarvester = get_nrmse(trajectoriesCsv$thinChainsawUtilizationWithWheeledHarvester, trajectoriesFeather$thinChainsawUtilizationWithWheeledHarvester),
                        thinChainsawCmhWithWheeledHarvester = get_nrmse(trajectoriesCsv$thinChainsawCmhWithWheeledHarvester, trajectoriesFeather$thinChainsawCmhWithWheeledHarvester),
                        thinChainsawPMhWithWheeledHarvester = get_nrmse(trajectoriesCsv$thinChainsawPMhWithWheeledHarvester, trajectoriesFeather$thinChainsawPMhWithWheeledHarvester),
                        thinForwardingMethod = sum(replace_na(trajectoriesCsv$thinForwardingMethod, "None") != as_forwarding_method(trajectoriesFeather$thinForwardingMethod)),
                        thinForwarderPMh = get_nrmse(trajectoriesCsv$thinForwarderPMh, trajectoriesFeather$thinForwarderPMh),
                        thinForwarderProductivity = get_nrmse(trajectoriesCsv$thinForwarderProductivity, trajectoriesFeather$thinForwarderProductivity),
                        thinForwardedWeight = get_nrmse(trajectoriesCsv$thinForwardedWeight, trajectoriesFeather$thinForwardedWeight),
                        thinGrappleSwingYarderPMhPerHectare = get_nrmse(trajectoriesCsv$thinGrappleSwingYarderPMhPerHectare, trajectoriesFeather$thinGrappleSwingYarderPMhPerHectare),
                        thinGrappleSwingYarderProductivity = get_nrmse(trajectoriesCsv$thinGrappleSwingYarderProductivity, trajectoriesFeather$thinGrappleSwingYarderProductivity),
                        thinGrappleSwingYarderOverweightFirstLogsPerHectare = get_nrmse(trajectoriesCsv$thinGrappleSwingYarderOverweightFirstLogsPerHectare, trajectoriesFeather$thinGrappleSwingYarderOverweightFirstLogsPerHectare),
                        thinGrappleYoaderPMhPerHectare = get_nrmse(trajectoriesCsv$thinGrappleYoaderPMhPerHectare, trajectoriesFeather$thinGrappleYoaderPMhPerHectare),
                        thinGrappleYoaderProductivity = get_nrmse(trajectoriesCsv$thinGrappleYoaderProductivity, trajectoriesFeather$thinGrappleYoaderProductivity),
                        thinGrappleYoaderOverweightFirstLogsPerHectare = get_nrmse(trajectoriesCsv$thinGrappleYoaderOverweightFirstLogsPerHectare, trajectoriesFeather$thinGrappleYoaderOverweightFirstLogsPerHectare),
                        thinProcessorPMhWithGrappleSwingYarder = get_nrmse(trajectoriesCsv$thinProcessorPMhWithGrappleSwingYarder, trajectoriesFeather$thinProcessorPMhWithGrappleSwingYarder),
                        thinProcessorProductivityWithGrappleSwingYarder = get_nrmse(trajectoriesCsv$thinProcessorProductivityWithGrappleSwingYarder, trajectoriesFeather$thinProcessorProductivityWithGrappleSwingYarder),
                        thinProcessorPMhWithGrappleYoader = get_nrmse(trajectoriesCsv$thinProcessorPMhWithGrappleYoader, trajectoriesFeather$thinProcessorPMhWithGrappleYoader),
                        thinProcessorProductivityWithGrappleYoader = get_nrmse(trajectoriesCsv$thinProcessorProductivityWithGrappleYoader, trajectoriesFeather$thinProcessorProductivityWithGrappleYoader),
                        thinLoadedWeight = get_nrmse(trajectoriesCsv$thinLoadedWeight, trajectoriesFeather$thinLoadedWeight),
                        regenFallerPMh = get_nrmse(trajectoriesCsv$regenFallerPMh, trajectoriesFeather$regenFallerPMh),
                        regenFallerProductivity = get_nrmse(trajectoriesCsv$regenFallerProductivity, trajectoriesFeather$regenFallerProductivity),
                        regenFellerBuncherPMh = get_nrmse(trajectoriesCsv$regenFellerBuncherPMh, trajectoriesFeather$regenFellerBuncherPMh),
                        regenFellerBuncherProductivity = get_nrmse(trajectoriesCsv$regenFellerBuncherProductivity, trajectoriesFeather$regenFellerBuncherProductivity),
                        regenTrackedHarvesterPMh = get_nrmse(trajectoriesCsv$regenTrackedHarvesterPMh, trajectoriesFeather$regenTrackedHarvesterPMh),
                        regenTrackedHarvesterProductivity = get_nrmse(trajectoriesCsv$regenTrackedHarvesterProductivity, trajectoriesFeather$regenTrackedHarvesterProductivity),
                        regenWheeledHarvesterPMh = get_nrmse(trajectoriesCsv$regenWheeledHarvesterPMh, trajectoriesFeather$regenWheeledHarvesterPMh),
                        regenWheeledHarvesterProductivity = get_nrmse(trajectoriesCsv$regenWheeledHarvesterProductivity, trajectoriesFeather$regenWheeledHarvesterProductivity),
                        regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder = sum(trajectoriesCsv$regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder != as_chainsaw_crew(trajectoriesFeather$regenChainsawCrewWithFellerBuncherAndGrappleSwingYarder)),
                        regenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder = get_nrmse(trajectoriesCsv$regenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder, trajectoriesFeather$regenChainsawUtilizationWithFellerBuncherAndGrappleSwingYarder),
                        regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder = get_nrmse(trajectoriesCsv$regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder, trajectoriesFeather$regenChainsawCmhWithFellerBuncherAndGrappleSwingYarder),
                        regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder = get_nrmse(trajectoriesCsv$regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder, trajectoriesFeather$regenChainsawPMhWithFellerBuncherAndGrappleSwingYarder),
                        regenChainsawCrewWithFellerBuncherAndGrappleYoader = sum(trajectoriesCsv$regenChainsawCrewWithFellerBuncherAndGrappleYoader != as_chainsaw_crew(trajectoriesFeather$regenChainsawCrewWithFellerBuncherAndGrappleYoader)),
                        regenChainsawUtilizationWithFellerBuncherAndGrappleYoader = get_nrmse(trajectoriesCsv$regenChainsawUtilizationWithFellerBuncherAndGrappleYoader, trajectoriesFeather$regenChainsawUtilizationWithFellerBuncherAndGrappleYoader),
                        regenChainsawCmhWithFellerBuncherAndGrappleYoader = get_nrmse(trajectoriesCsv$regenChainsawCmhWithFellerBuncherAndGrappleYoader, trajectoriesFeather$regenChainsawCmhWithFellerBuncherAndGrappleYoader),
                        regenChainsawPMhWithFellerBuncherAndGrappleYoader = get_nrmse(trajectoriesCsv$regenChainsawPMhWithFellerBuncherAndGrappleYoader, trajectoriesFeather$regenChainsawPMhWithFellerBuncherAndGrappleYoader),
                        regenChainsawCrewWithTrackedHarvester = sum(trajectoriesCsv$regenChainsawCrewWithTrackedHarvester != as_chainsaw_crew(trajectoriesFeather$regenChainsawCrewWithTrackedHarvester)),
                        regenChainsawUtilizationWithTrackedHarvester = get_nrmse(trajectoriesCsv$regenChainsawUtilizationWithTrackedHarvester, trajectoriesFeather$regenChainsawUtilizationWithTrackedHarvester),
                        regenChainsawCmhWithTrackedHarvester = get_nrmse(trajectoriesCsv$regenChainsawCmhWithTrackedHarvester, trajectoriesFeather$regenChainsawCmhWithTrackedHarvester),
                        regenChainsawPMhWithTrackedHarvester = get_nrmse(trajectoriesCsv$regenChainsawPMhWithTrackedHarvester, trajectoriesFeather$regenChainsawPMhWithTrackedHarvester),
                        regenChainsawCrewWithWheeledHarvester = sum(trajectoriesCsv$regenChainsawCrewWithWheeledHarvester != as_chainsaw_crew(trajectoriesFeather$regenChainsawCrewWithWheeledHarvester)),
                        regenChainsawUtilizationWithWheeledHarvester = get_nrmse(trajectoriesCsv$regenChainsawUtilizationWithWheeledHarvester, trajectoriesFeather$regenChainsawUtilizationWithWheeledHarvester),
                        regenChainsawCmhWithWheeledHarvester = get_nrmse(trajectoriesCsv$regenChainsawCmhWithWheeledHarvester, trajectoriesFeather$regenChainsawCmhWithWheeledHarvester),
                        regenChainsawPMhWithWheeledHarvester = get_nrmse(trajectoriesCsv$regenChainsawPMhWithWheeledHarvester, trajectoriesFeather$regenChainsawPMhWithWheeledHarvester),
                        regenGrappleSwingYarderPMhPerHectare = get_nrmse(trajectoriesCsv$regenGrappleSwingYarderPMhPerHectare, trajectoriesFeather$regenGrappleSwingYarderPMhPerHectare),
                        regenGrappleSwingYarderProductivity = get_nrmse(trajectoriesCsv$regenGrappleSwingYarderProductivity, trajectoriesFeather$regenGrappleSwingYarderProductivity),
                        regenGrappleSwingYarderOverweightFirstLogsPerHectare = get_nrmse(trajectoriesCsv$regenGrappleSwingYarderOverweightFirstLogsPerHectare, trajectoriesFeather$regenGrappleSwingYarderOverweightFirstLogsPerHectare),
                        regenGrappleYoaderPMhPerHectare = get_nrmse(trajectoriesCsv$regenGrappleYoaderPMhPerHectare, trajectoriesFeather$regenGrappleYoaderPMhPerHectare),
                        regenGrappleYoaderProductivity = get_nrmse(trajectoriesCsv$regenGrappleYoaderProductivity, trajectoriesFeather$regenGrappleYoaderProductivity),
                        regenGrappleYoaderOverweightFirstLogsPerHectare = get_nrmse(trajectoriesCsv$regenGrappleYoaderOverweightFirstLogsPerHectare, trajectoriesFeather$regenGrappleYoaderOverweightFirstLogsPerHectare),
                        regenProcessorPMhWithGrappleSwingYarder = get_nrmse(trajectoriesCsv$regenProcessorPMhWithGrappleSwingYarder, trajectoriesFeather$regenProcessorPMhWithGrappleSwingYarder),
                        regenProcessorProductivityWithGrappleSwingYarder = get_nrmse(trajectoriesCsv$regenProcessorProductivityWithGrappleSwingYarder, trajectoriesFeather$regenProcessorProductivityWithGrappleSwingYarder),
                        regenProcessorPMhWithGrappleYoader = get_nrmse(trajectoriesCsv$regenProcessorPMhWithGrappleYoader, trajectoriesFeather$regenProcessorPMhWithGrappleYoader),
                        regenProcessorProductivityWithGrappleYoader = get_nrmse(trajectoriesCsv$regenProcessorProductivityWithGrappleYoader, trajectoriesFeather$regenProcessorProductivityWithGrappleYoader),
                        regenLoadedWeight = get_nrmse(trajectoriesCsv$regenLoadedWeight, trajectoriesFeather$regenLoadedWeight))
# csvFeatherDiff should have a single row indicating only small differences
#print(csvFeatherDiff, width = Inf)
# for readability, transpose the diff to long
csvFeatherDiffPivot = csvFeatherDiff %>% select(-starts_with("financialScenarios")) %>% pivot_longer(cols = everything(), names_to = "column", values_to = "diffScale")
print(csvFeatherDiffPivot, n = 150)
print(csvFeatherDiffPivot %>% filter(abs(diffScale) > 0.004)) # NPV and LEV at ~0.025 due to rounding to nearest dollar in .csv

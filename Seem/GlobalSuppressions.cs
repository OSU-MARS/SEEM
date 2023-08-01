// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "https://github.com/dotnet/roslyn-analyzers/issues/6802", Scope = "member", Target = "~M:Mars.Seem.Extensions.NativeMethods.CallNtPowerInformation(System.Int32,System.IntPtr,System.UInt32,Mars.Seem.Extensions.NativeMethods.PROCESSOR_POWER_INFORMATION[],System.UInt32)~System.UInt32")]
[assembly: SuppressMessage("Style", "IDE0220:Add explicit cast", Justification = "https://github.com/dotnet/roslyn/issues/63470", Scope = "member", Target = "~M:Mars.Seem.Optimization.OptimizationObjectiveDistribution.GetMaximumMoveIndex~System.Int32")]
[assembly: SuppressMessage("Style", "IDE0220:Add explicit cast", Justification = "https://github.com/dotnet/roslyn/issues/63470", Scope = "member", Target = "~M:Mars.Seem.Optimization.OptimizationObjectiveDistribution.GetFinancialStatisticsForMove(System.Int32)~Mars.Seem.Optimization.DistributionStatistics")]
[assembly: SuppressMessage("Style", "IDE0074:Use compound assignment", Justification = "readability", Scope = "member", Target = "~M:Mars.Seem.Organon.OrganonGrowth.ValidateArguments(System.Int32,Mars.Seem.Organon.OrganonConfiguration,Mars.Seem.Organon.OrganonTreatments,Mars.Seem.Organon.OrganonStand,System.Collections.Generic.SortedList{Mars.Seem.Tree.FiaCode,Mars.Seem.Organon.SpeciesCalibration},System.Int32@,System.Int32@)")]
[assembly: SuppressMessage("Style", "IDE0074:Use compound assignment", Justification = "readability", Scope = "member", Target = "~M:Mars.Seem.Organon.OrganonStandTrajectory.Simulate~System.Int32")]
[assembly: SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "readability", Scope = "type", Target = "~T:Mars.Seem.Cmdlets.OptimizeCmdlet`1")]
[assembly: SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "readability", Scope = "type", Target = "~T:Mars.Seem.Cmdlets.WriteGrasp")]
[assembly: SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "readability", Scope = "type", Target = "~T:Mars.Seem.Cmdlets.WriteHarvest")]
[assembly: SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "readability", Scope = "type", Target = "~T:Mars.Seem.Cmdlets.WriteHarvestSchedule")]
[assembly: SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "readability", Scope = "type", Target = "~T:Mars.Seem.Cmdlets.WriteSilviculturalTrajectoriesCmdlet")]
[assembly: SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "readability", Scope = "type", Target = "~T:Mars.Seem.Heuristics.Population")]
[assembly: SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "readability", Scope = "type", Target = "~T:Mars.Seem.Optimization.FinancialScenarios")]
[assembly: SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "readability", Scope = "type", Target = "~T:Mars.Seem.Organon.OrganonStandTrajectory")]
[assembly: SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "readability", Scope = "type", Target = "~T:Mars.Seem.XlsxReader")]

# ideally, Seem.dll would be loaded from Seem build directory to pick up the most current assembly created from the release build triggered by 
# Visual Studio's performance profiler 
# However, it's necessary to from the unit tests directory so .NET fusion finds DocumentFormat.OpenXml.dll and other dependencies. This means
# the unit tests should be built before profiling.
Set-Location -Path ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Seem\\UnitTests\\bin\\x64"))
#$buildDirectory = ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Seem\\UnitTests\\bin\\x64\\Debug\\net6.0"))
$buildDirectory = ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Seem\\UnitTests\\bin\\x64\\Release\\net6.0"))
# package Seem.dll from Visual Studio developer command line to obtain using
#   dotnet publish -r win-x64 --no-self-contained /p:Configuration=Release /p:Platform=win-x64 -v:n
# As of Visual Studio 2022 17.0.4, publishing from within Visual Studio does not yield a crossgen2 assembly.
$buildDirectory = ([System.IO.Path]::Combine($env:USERPROFILE, "source\repos\SEEM\Seem\bin\win-x64\Release\net6.0\win-x64\publish"))
Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "Seem.dll"));

# CPU
$nelder1 = Get-StandFromPlot -Plot 1 -Age 20 -Area 20 -ExpansionFactor 1.327 -PlantingDensity 1035 -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\Organon\\Malcolm Knapp Nelder 1.xlsx")) -XlsxSheet "1";
$hero = Optimize-Hero -MaxIterations 2 -Threads 1 -Stand $nelder1 -Verbose

# .NET object allocation -> memory
#$plot14y16 = Get-StandFromPlot -Plot (14, 16) -Age 30 -Area 20 -ExpansionFactor 2.127 -PlantingDensity 990 -SiteIndexInM 39.6 -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\Organon\\Malcolm Knapp plots 14-18+34 Ministry.xlsx")) -XlsxSheet "0.2 ha";
#$threeThinTrajectories = Optimize-Prescription -DiscountRates (0.02, 0.03, 0.04, 0.05, 0.06, 0.07) -FromAbovePercentageUpperLimit 40 -FromBelowPercentageUpperLimit 40 -MinimumIntensity 10 -MaximumIntensity 90 -DefaultStep 8 -FirstThinPeriod (-1, 1, 2, 3, 4, 5, 6, 7, 8) -SecondThinPeriod (-1, 2, 3, 4, 5, 6, 7, 8, 9) -ThirdThinPeriod (-1, 3, 4, 5, 6, 7, 8, 9, 10) -PlanningPeriods (11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1) -Stand $plot14y16 -Threads 4 -Verbose

#$plot14y16 = Get-StandFromPlot -Plot (14, 16) -Age 30 -Area 20 -ExpansionFactorPerHa 2.132 -PlantingDensityPerHa 990 -SiteIndexInM 39.6 -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\Organon\\Malcolm Knapp plots 14-18+34 Ministry.xlsx")) -XlsxSheet "0.2 ha"
#$harvestCosts10k = Get-FinancialScenarios -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "cost variations 10000.xlsx"))
#$threeThinTrajectories09 = Optimize-Prescription -Financial $harvestCosts10k -Enumerate -DefaultStep 5 -MinimumStep 5 -FromAbovePercentageUpperLimit 0 -FromBelowPercentageUpperLimit -0 -MinimumIntensity 25 -MaximumIntensity 40 -FirstThinPeriod (1) -SecondThinPeriod (3) -ThirdThinPeriod (5, 6, 7) -RotationLengths (9) -Stand $plot14y16 -Verbose
#Write-StandTrajectory -Results $threeThinTrajectories09 -CsvFile ([System.IO.Path]::Combine((Get-Location), "..\\..\\..\\TestResults\\psme14y16_threeThins10+310c65s10kfs_75.csv"));
#Start-Sleep -Seconds 60

# stand trajectory write speed (could also write to PerformanceResults)
#$tsheCosts31500 = Get-FinancialScenarios -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\PhD\\papers\\harvest cost\\tshe harvest cost Sobol first order N = 500 31500.xlsx"))
#$plot21 = Get-StandFromPlot -Plot 21 -Age 30 -Area 20 -ExpansionFactorPerHa 2.272 -ForwardingTethered 190 -PlantingDensityPerHa 1113 -HemlockSiteIndexInM 29.1 -SiteIndexInM 39.6 -SlopeInPercent 65 -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\Organon\\Malcolm Knapp plot 21 Ministry height imputed.xlsx")) -XlsxSheet "Sheet1"
#$unthinnedTrajectories = Get-StandTrajectory -Stand $plot21 -Financial $tsheCosts31500 -RotationLengths (3, 4, 5, 6, 7, 8, 9, 10, 11)
#Write-StandTrajectory -Trajectories $unthinnedTrajectories -HarvestsOnly -NoCarbon -NoEquipmentProductivity -NoFinancial -NoTimberSorts -NoTreeGrowth -CsvFile ([System.IO.Path]::Combine((Get-Location), "..\\..\\..\\TestResults\\tshe21_unthinned10+190c65s31500fs.csv"));

# solution pools
#$plot14y16 = Get-StandFromPlot -Plot (14, 16) -Age 30 -Area 20 -ExpansionFactor 2.127 -PlantingDensity 990 -SiteIndexInM 39.6 -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\Organon\\Malcolm Knapp plots 14-18+34 Ministry.xlsx")) -XlsxSheet "0.2 ha";
#$hero = Optimize-Hero -BestOf 4 -SolutionPoolSize 2 -FirstThinPeriod (1) -SecondThinPeriod (-1, 2) -ThirdThinPeriod (-1, 3) -RotationLength (4, 5) -Stand $plot14y16 -Verbose

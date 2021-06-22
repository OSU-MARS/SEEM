Set-Location -Path ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Seem\\UnitTests\\bin\\x64\\"))
$buildDirectory = ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Seem\\UnitTests\\bin\\x64\\Debug\\net5.0"))
#$buildDirectory = ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Seem\\UnitTests\\bin\\x64\\Release\\net5.0"))
Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "Seem.dll"));

# CPU
#$nelder1 = Get-StandFromPlot -Plot 1 -Age 20 -ExpansionFactor 1.327 -PlantingDensity 1035 -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\Organon\\Malcolm Knapp Nelder 1.xlsx")) -XlsxSheet "1";
#$hero = Optimize-Hero -MaxIterations 2 -Threads 1 -Stand $nelder1 -Verbose

# .NET object allocation -> memory
#$plot14y16 = Get-StandFromPlot -Plot (14, 16) -Age 30 -ExpansionFactor 2.127 -PlantingDensity 990 -SiteIndex 130 -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\Organon\\Malcolm Knapp plots 14-18+34 Ministry.xlsx")) -XlsxSheet "0.2 ha";
#$timberValue = New-Object -TypeName "Osu.Cof.Ferm.TimberValue" -ArgumentList @(120.0, 75.0, $false)
#$timberValue.TimberAppreciationRate = 0.0;
#$threeThinTrajectories = Optimize-Prescription -DiscountRates (0.02, 0.03, 0.04, 0.05, 0.06, 0.07) -FromAbovePercentageUpperLimit 40 -FromBelowPercentageUpperLimit 40 -MinimumIntensity 10 -MaximumIntensity 90 -DefaultStep 8 -FirstThinPeriod (-1, 1, 2, 3, 4, 5, 6, 7, 8) -SecondThinPeriod (-1, 2, 3, 4, 5, 6, 7, 8, 9) -ThirdThinPeriod (-1, 3, 4, 5, 6, 7, 8, 9, 10) -PlanningPeriods (11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1) -Stand $plot14y16 -Threads 4 -TimberValue $timberValue -Verbose

# solution pools
$plot14y16 = Get-StandFromPlot -Plot (14, 16) -Age 30 -ExpansionFactor 2.127 -PlantingDensity 990 -SiteIndex 130 -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\Organon\\Malcolm Knapp plots 14-18+34 Ministry.xlsx")) -XlsxSheet "0.2 ha";
$timberValue = New-Object -TypeName "Osu.Cof.Ferm.TimberValue" -ArgumentList @(120.0, 75.0, $false)
$timberValue.TimberAppreciationRate = 0.0;
$hero = Optimize-Hero -BestOf 4 -SolutionPoolSize 2 -DiscountRates 0.03 -FirstThinPeriod (1) -SecondThinPeriod (-1, 2) -ThirdThinPeriod (-1, 3) -PlanningPeriods (4, 5) -Stand $plot14y16 -TimberValue $timberValue -Verbose

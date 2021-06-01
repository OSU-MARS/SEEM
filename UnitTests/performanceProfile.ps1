Set-Location -Path ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Seem\\UnitTests\\bin\\x64\\Release"))
$buildDirectory = ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Seem\\UnitTests\\bin\\x64\\Release\\net5.0"))
Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "Seem.dll"));
$stand = Get-StandFromPlot -Plot 1 -Age 20 -ExpansionFactor 1.327 -PlantingDensity 1035 -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\Organon\\Malcolm Knapp Nelder 1.xlsx")) -XlsxSheet "1";
$hero = Optimize-Hero -MaxIterations 2 -Threads 1 -Stand $stand -Verbose

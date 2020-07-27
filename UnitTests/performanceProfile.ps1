Set-Location -Path ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Seem\\UnitTests\\bin\\x64\\Release"))
$buildDirectory = ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Seem\\UnitTests\\bin\\x64\\Release\\netcoreapp3.1"))
Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "Seem.dll"));
$stand = Get-StandFromPlot -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\Organon\\MalcolmKnappNelder1.xlsx"));
$hero = Optimize-Hero -MaxIterations 2 -Cores 1 -LandExpectationValue -Stand $stand -VolumeUnits ScribnerBoardFeetPerAcre -Verbose

Set-Location -Path ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Organon\\UnitTests\\bin\\x64\\Release"))
$buildDirectory = ([System.IO.Path]::Combine($env:USERPROFILE, "source\\repos\\Organon\\UnitTests\\bin\\x64\\Release\\netcoreapp3.1"))
Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "Organon.dll"));
$stand = Get-StandFromNelderPlot -Xlsx ([System.IO.Path]::Combine($env:USERPROFILE, "OSU\\Organon\\Nelder20.xlsx"));
$hero = Optimize-Hero -Iterations 2 -Cores 1 -NetPresentValue -Stand $stand -UniformHarvestProbability -VolumeUnits ScribnerBoardFeetPerAcre -Verbose

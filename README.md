### Overview
A research codebase for investigating optimization of individual tree selection thinning with emphasis on Pacific Northwest forests.
Peer-reviewed journal articles which use the code are indicated by tags with DOI identifiers and corresponding releases. Conference 
presentations or papers are also tagged.

The primary code components are

* PowerShell cmdlets for scripting silvicultural optimization and stand trajectory logging.
* Single searcher and evolutionary heuristics. See [West *et al.* 2021](https://doi.org/10.3390/f12030280).
* Log sort and live biomass estimation using [Poudel *et al.* 2018](https://doi.org/10.1186/s40663-018-0134-2), 
[Poudel *et al.* 2019](https://dx.doi.org/10.1139/cjfr-2018-0361), BC Firmwood, and Scribner.C scaling.
* A performance tuned version of Organon 2.2.4 and C# implementations of logarithms, exponents, and powers using 128 bit SIMD.

### Dependencies
SEEM is a .NET 7.0 assembly which includes C# cmdlets for PowerShell Core. It therefore makes use of both the System.Management.Automation
nuget package and the system's PowerShell Core installation, creating a requirement the PowerShell Core version be the same or newer than 
the nuget's. If Visual Studio Code is used for PowerShell Core execution then corresponding updates to Code and its PowerShell extension
are required.

SEEM is developed using current or near-current versions of [Visual Studio Community](https://visualstudio.microsoft.com/downloads/) edition. 
In principle it can be ported to any .NET supported platform with minimal effort but availability of x64 AVX instructions (Intel Core 
2<sup>nd</sup> generation, equivalent AMD processors, or newer) is assumed and SEEM is only tested on Windows 10.

### Relationship to CIPS-R 2.2.4 Organon (2014) and Organon 9.1 (2011)
This repo began with a C# port and object oriented refactoring of the [CIPS-R](http://r-forge.r-project.org/projects/cipsr/) 2.2.4 Organon
individual tree diameter and height growth, crown ratio, and mortality models. Organon data structures were converted to SoA (structure of
arrays), functions vectorized for SIMD, and Organon's 2000 tree limit was removed. SEEM Organon is mathematically equivalent to the Fortran
version but differs in numerical precision due to use of different libraries and IEEE 754 decompositions for logarithms and exponentiation.

Organon's volume code was also ported but is not included in the build due to deprecation of its 1986 taper model. Similarly, Organon's 
tripling, additional mortality, and rundll wood quality code were ported but later removed due to low maintenance priority. Organon's wood 
quality dll also wasn't ported due to low priority and the edit dll wasn't ported due to redundancy with much of the rundll. Other than one 
minor bug fix, CIPS-R 2.2.4 Organon is identical to [Organon 9.1](http://www.cof.orst.edu/cof/fr/research/organon/).
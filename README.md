### Overview
A research codebase for investigating harvest optimization methods.

### Relationsip to CIPS-R 2.2.4 (2014) and Organon 9.1 (2011)
This repo began with a C# port and object oriented refactoring of the [CIPS-R](http://r-forge.r-project.org/projects/cipsr/) 2.2.4 Organon
individual tree diameter and height growth, crown ratio, and mortality models. Organon's volume code was also ported but is not included in
the build due to deprecation of its 1986 taper model. Similarly, Organon's tripling and rundll wood quality code was ported but later removed 
due to low maintenance priority in this fork of the codebase. Organon's wood quality dll also wasn't ported due to low priority and the edit 
dll wasn't ported due to redundancy with much of the rundll. Other than one minor bug fix, CIPS-R 2.2.4 Organon is identical to 
[Organon 9.1](http://www.cof.orst.edu/cof/fr/research/organon/).
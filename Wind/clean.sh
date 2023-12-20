#!/bin/sh

find . -name "*~"|xargs rm -f 

rm -rf bin
rm -f  SFS_Wind*/OpenSim.SFS_Wind.sln
rm -f  SFS_Wind*/*.build
rm -f  SFS_Wind*/Modules/*.build
rm -f  SFS_Wind*/Modules/*.csproj*
rm -rf SFS_Wind*/Modules/bin
rm -rf SFS_Wind*/Modules/obj

(cd FFTW_Wind && ./clean.sh)

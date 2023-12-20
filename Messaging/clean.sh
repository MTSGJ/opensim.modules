#!/bin/sh

find . -name "*~"|xargs rm -f 

rm -rf bin
rm -f  MuteList*/OpenSim.sln
rm -f  MuteList*/*.build
rm -f  MuteList*/Modules/*.build
rm -f  MuteList*/Modules/*.csproj*
rm -rf MuteList*/Modules/bin
rm -rf MuteList*/Modules/obj

#!/bin/sh

find . -name "*~"|xargs rm -f 

rm -rf bin
rm -f  Physics*/OpenSim.sln
rm -f  Physics*/*.build
rm -f  Physics*/Modules/*.build
rm -f  Physics*/Modules/*.csproj*
rm -rf Physics*/Modules/bin
rm -rf Physics*/Modules/obj

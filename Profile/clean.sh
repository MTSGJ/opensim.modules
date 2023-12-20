#!/bin/sh

find . -name "*~"|xargs rm -f 

rm -rf bin
rm -f  OpenSimProfile*/OpenSim.sln
rm -f  OpenSimProfile*/*.build
rm -f  OpenSimProfile*/Modules/*.build
rm -f  OpenSimProfile*/Modules/*.csproj*
rm -rf OpenSimProfile*/Modules/bin
rm -rf OpenSimProfile*/Modules/obj


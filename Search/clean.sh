#!/bin/sh

find . -name "*~"|xargs rm -f 

rm -rf bin
rm -f  OpenSimSearch*/OpenSim.sln
rm -f  OpenSimSearch*/*.build
rm -f  OpenSimSearch*/Modules/*.build
rm -f  OpenSimSearch*/Modules/*.csproj*
rm -rf OpenSimSearch*/Modules/bin
rm -rf OpenSimSearch*/Modules/obj

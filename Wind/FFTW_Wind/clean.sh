#!/bin/sh

find . -name "*~"|xargs rm -f 

rm -rf bin
rm -f  OpenSim.sln
rm -f  *.build
rm -f  Modules/*.build
rm -f  Modules/*.csproj*
rm -rf Modules/bin
rm -rf Modules/obj

(cd sfsw_v2 && make clean >/dev/null 2>&1)
(cd sfsw_v3 && make clean >/dev/null 2>&1)


#!/bin/sh

WNDDLL="SimpleFluidSolverWind.dll"

VER=""
if [ "$1" != "" ]; then
	VER="_"$1
fi

echo "=========================="
echo "SFS_Wind$VER"
echo "=========================="

/bin/bash clean.sh

cd SFS_Wind$VER
/bin/bash runprebuild.sh

if [ -f build.sh ]; then
    /bin/bash build.sh || exit 1
else
    xbuild /target:CLean || exit 1
    #xbuild /p:Configuration=Release || exit 1
    #xbuild || exit 1
    xbuild /p:DebugSymbols=False /p:Configuration=Release || exit 1

    cp -f ../bin/$WNDDLL ../../bin
fi
cp -f ../AForge.NET/AForge.Math.dll ../../bin

echo

#!/bin/sh

WNDDLL="SimpleFluidSolverWind.dll"

VER=""
DOTNETVER=9.0

if [ "$1" != "" ]; then
    VER="_"$1
fi
if [ "$2" != "" ]; then
    DOTNETVER=$2
fi

echo "=========================="
echo "  SFS_Wind$VER"
echo "=========================="

/bin/bash clean.sh

cd SFS_Wind$VER

if [ -f build.sh ]; then
    /bin/bash build.sh $DOTNETVER || exit 1
else
    /bin/bash clean.sh 
    /bin/bash runprebuild.sh 
    xbuild /target:CLean || exit 1
    #xbuild /p:Configuration=Release || exit 1
    xbuild /p:DebugSymbols=False /p:Configuration=Release || exit 1
    cp -f ../bin/$DLL ../../bin || exit 1
fi

echo

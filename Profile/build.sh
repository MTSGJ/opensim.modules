#!/bin/sh

DLL=OpenSimProfile.Modules.dll

VER=""
DOTNETVER=8.0

if [ "$1" != "" ]; then
    VER="_"$1
fi
if [ "$2" != "" ]; then
    DOTNETVER=$2
fi

echo "=========================="
echo "  OpenSimProfile$VER"
echo "=========================="

/bin/bash clean.sh

cd OpenSimProfile$VER

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

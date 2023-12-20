#!/bin/sh

DLL=OpenSimProfile.Modules.dll

VER=""
if [ "$1" != "" ]; then
	VER="_"$1
fi

echo "=========================="
echo "OpenSimProfile$VER"
echo "=========================="

/bin/bash clean.sh

cd OpenSimProfile$VER
/bin/bash runprebuild.sh

if [ -f build.sh ]; then
    /bin/bash build.sh || exit 1
else
    xbuild /target:CLean || exit 1
    #xbuild /p:Configuration=Release || exit 1
    xbuild /p:DebugSymbols=False /p:Configuration=Release || exit 1
    cp -f ../bin/$DLL ../../bin || exit 1
fi

echo

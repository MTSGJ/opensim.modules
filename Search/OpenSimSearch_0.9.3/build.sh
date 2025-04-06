#!/bin/sh
#

MTLSTDLL=OpenSimSearch.Modules.dll
DOTNETVER=9.0

if [ "$1" != "" ]; then
    DOTNETVER=$1
fi
#
./clean.sh
./runprebuild.sh $DOTNETVER
dotnet build -c Release OpenSim.Search.sln || exit 1

cp -f ../bin/net${DOTNETVER}/$MTLSTDLL ../../bin || exit 1

exit 0

#!/bin/sh

SRCHDLL=OpenSimSearch.Modules.dll
DOTNETVER=6.0

#
dotnet build -c Release OpenSim.Search.sln || exit 1

cp -f ../bin/net${DOTNETVER}/$SRCHDLL ../../bin || exit 1

exit 0

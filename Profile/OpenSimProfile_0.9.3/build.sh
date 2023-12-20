#!/bin/sh

PRFLDLL=OpenSimProfile.Modules.dll
DOTNETVER=6.0

#
dotnet build -c Release OpenSim.Profile.sln || exit 1

cp -f ../bin/net${DOTNETVER}/$PRFLDLL ../../bin || exit 1

exit 0

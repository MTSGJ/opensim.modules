#!/bin/sh

MTLSTDLL=Messaging.NSLMuteList.dll
DOTNETVER=6.0

#
dotnet build -c Release OpenSim.MuteList.sln || exit 1

cp -f ../bin/net${DOTNETVER}/$MTLSTDLL ../../bin || exit 1

exit 0

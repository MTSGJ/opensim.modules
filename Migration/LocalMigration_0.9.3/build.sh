#!/bin/sh

DLL=LocalMigration.Modules.dll
DOTNETVER=6.0

#
dotnet build -c Release OpenSim.Migration.sln || exit 1

cp -f ../bin/net${DOTNETVER}/$DLL ../../bin || exit 1

exit 0

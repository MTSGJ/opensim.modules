#!/bin/sh

WINDDLL=SimpleFluidSolverWind.dll
DOTNETVER=6.0

#
dotnet build -c Release OpenSim.SFS_Wind.sln || exit 1

cp -f ../bin/net${DOTNETVER}/$WINDDLL ../../bin || exit 1

exit 0

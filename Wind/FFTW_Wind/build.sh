#!/bin/sh

DLL="SimpleFluidSolverWind.dll"

VER=""
if [ "$1" != "" ]; then
	VER="_"$1
fi

echo "=========================="
echo "SFS_Wind$VER (with FFTW)"
echo "=========================="

./clean.sh
./runprebuild.sh
xbuild || exit 1

cd sfsw
make
cd ..

cp -f ./bin/$DLL ../../bin
cp -f ./sfsw/libsfsw.so ../../bin
cp -f ./conf/sfsw.dll.config ../../bin

echo

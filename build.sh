#!/bin/sh

VER=0.9.3
DOTNETVER=8.0 

#
if [ "$1" != "" ]; then
    VER=$1
fi
DVER=""
if [ "$VER" != "" ]; then
	DVER="_"$VER
fi

echo "=========================="
echo "  NSL_MODULES$DVER"
echo "=========================="

./clean.sh

# Migration
cd Migration
./build.sh $VER $DOTNETVER || exit 1
cd ..

# MuteList
cd Messaging
./build.sh $VER $DOTNETVER || exit 1
cd ..

# OS Profile
cd Profile
./build.sh $VER $DOTNETVER || exit 1
cd ..

# OS Search
cd Search
./build.sh $VER $DOTNETVER || exit 1
cd ..

# Physics
cd Physics
#./build.sh $VER $DOTNETVER || exit 1
cd ..

# Wind
cd Wind
./build.sh $VER $DOTNETVER || exit 1
cd ..

#
cp bin/* ../bin || exit 1

#
#
echo


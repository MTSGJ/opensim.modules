#!/bin/sh

find . -name "*~"|xargs rm -f 

rm -rf bin
rm -f  LocalMigration*/OpenSim.sln
rm -f  LocalMigration*/*.build
rm -f  LocalMigration*/Modules/*.build
rm -f  LocalMigration*/Modules/*.csproj*
rm -rf LocalMigration*/Modules/bin
rm -rf LocalMigration*/Modules/obj


#!/bin/sh

find . -name "*~"|xargs rm -f 

rm -f bin/*

(cd Migration && ./clean.sh)
(cd Messaging && ./clean.sh)
(cd Profile && ./clean.sh)
(cd Search && ./clean.sh)
(cd Physics && ./clean.sh)
(cd Wind && ./clean.sh)

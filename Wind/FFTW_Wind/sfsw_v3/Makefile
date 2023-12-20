

#all: libsfsw.so sfsw
all: libsfsw.so

libsfsw.so: sfsw.o
	gcc sfsw.o -lfftw3_threads -lfftw3 -lm -shared -O2 -o libsfsw.so
	
sfsw.o: sfsw.c 
	gcc sfsw.c -fPIC -W -Wall -c


clean:
	rm -f *.o *.so sfsw


sfsw: sfsw.o
	gcc sfsw.o -lm -lfftw3_threads -lfftw3 -O2 -o sfsw


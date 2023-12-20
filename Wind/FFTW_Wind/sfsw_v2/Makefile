

#all: libsfsw.so sfsw
all: libsfsw.so

libsfsw.so: sfsw.o
	gcc sfsw.o -L/usr/local/lib -lrfftw -lfftw -lm -shared -O2 -o libsfsw.so
	
sfsw.o: sfsw.c 
	gcc sfsw.c -fPIC -W -Wall -I/usr/local/include -c


clean:
	rm -f *.o *.so sfsw


sfsw: sfsw.o
	gcc sfsw.o -L/usr/local/lib -lm -lrfftw -lfftw -O2 -o sfsw


CC=g++
CFLAGS = -g -Wall -DLINUX -I../../../inc/
LDFLAGS = -ldcamapi -lpthread
EXECUTABLE = attach_imagebuf
SOURCES = $(EXECUTABLE).cpp

all:	$(EXECUTABLE)

$(EXECUTABLE): $(EXECUTABLE).o common.o qthread.o
	$(CC) $(EXECUTABLE).o common.o qthread.o -o $(EXECUTABLE) $(LDFLAGS)

$(EXECUTABLE).o:	$(SOURCES)
	$(CC) $(CFLAGS) $(LDFLAGS) -c $(SOURCES)

common.o:	../misc/common.cpp
	$(CC) $(CFLAGS) -c ../misc/common.cpp 

qthread.o:	../misc/qthread.cpp
	$(CC) $(CFLAGS) -c ../misc/qthread.cpp 

clean:
	rm -rf *.o $(EXECUTABLE)

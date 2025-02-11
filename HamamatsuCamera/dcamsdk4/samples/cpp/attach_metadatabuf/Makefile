CC=g++
CFLAGS = -g -Wall -DLINUX -I../../../inc/
LDFLAGS = -ldcamapi
EXECUTABLE = attach_metadatabuf
SOURCES = $(EXECUTABLE).cpp

all:	$(EXECUTABLE)

$(EXECUTABLE): $(EXECUTABLE).o common.o
	$(CC) $(EXECUTABLE).o common.o -o $(EXECUTABLE) $(LDFLAGS)

$(EXECUTABLE).o:	$(SOURCES)
	$(CC) $(CFLAGS) $(LDFLAGS) -c $(SOURCES)

common.o:	../misc/common.cpp
	$(CC) $(CFLAGS) -c ../misc/common.cpp 

clean:
	rm -rf *.o $(EXECUTABLE)

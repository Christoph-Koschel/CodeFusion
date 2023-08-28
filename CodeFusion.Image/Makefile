CC = gcc
LD = ld
CFLAGS = -Wall -Wpointer-arith -Wextra -Wswitch-enum -Wmissing-prototypes -Wimplicit-fallthrough -Wconversion -fno-strict-aliasing -O3 -std=c11 -pedantic
HEADERS = cf/CodeFusion.h cf/hashmap.h cf/loader.h cf/machine.h cf/opcode.h
SOURCES = cf/hashmap.c cf/loader.c cf/machine.c cf/opcode.c main.c
OBJECTS = $(SOURCES:.c=.o)
OBJECT = image.o

.PHONY: all clean

all: $(OBJECT)

%.o: %.c
	$(CC) $(CFLAGS) -c $< -o $@

$(OBJECT): $(OBJECTS)
	$(LD) -r $^ -o $(OBJECT)

clean:
	rm -f $(OBJECTS) $(EXECUTABLE)
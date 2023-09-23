CC = gcc
LD = ld
CFLAGS = -Wall -Wpointer-arith -Wextra -Wswitch-enum -Wmissing-prototypes -Wimplicit-fallthrough -Wconversion -fno-strict-aliasing -O3 -std=c11 -pedantic

HEADERS = cf/CodeFusion.h cf/hashmap.h cf/loader.h cf/machine.h cf/opcode.h bridge/bridge.h

IMAGES_SRC = cf/hashmap.c cf/loader.c cf/machine.c cf/opcode.c main.c
TABLES_SRC = interrupt/cross.c
LIBRARY_SRC = cf/hashmap.c cf/loader.c cf/opcode.c library.c

IMAGES_OBJ = $(IMAGES_SRC:.c=.o)
TABLES_OBJ = $(TABLES_SRC:.c=.o)
LIBRARY_OBJ = $(LIBRARY_SRC:.c=.o)

ifdef OS
	OUTDIR = "win/"
	LOADERS_SRC = loader/win.c
else
	OUTDIR = "linux/"
	LOADERS_SRC = loader/linux.c
endif

LOADERS_OBJ = $(LOADERS_SRC:.c=.o)

IMAGE_O = $(OUTDIR)image.o
TABLE_O = $(OUTDIR)table.o
LIBRARY_O = $(OUTDIR)library.o
LOADER_O = $(OUTDIR)loader.o



.PHONY: all clean

all: $(IMAGE_O) $(LIBRARY_O) $(TABLE_O) $(LOADER_O)

$(IMAGE_O): $(IMAGES_OBJ)
	$(LD) -r $^ -o $(IMAGE_O)

$(LIBRARY_O): $(LIBRARY_OBJ)
	$(LD) -r $^ -o $(LIBRARY_O)

$(TABLE_O): $(TABLES_OBJ)
	$(LD) -r $^ -o $(TABLE_O)

$(LOADER_O): $(LOADERS_OBJ)
	$(LD) -r $^ -o $(LOADER_O)

%.o: %.c
	$(CC) $(CFLAGS) -c $< -o $@

clean:
	rm -f  $(IMAGES_OBJ) $(TABLES_OBJ) $(LIBRARY_OBJ) $(LOADERS_OBJ) $(IMAGE_O) $(TABLE_O) $(LIBRARY_O) $(LOADER_O)
CC = gcc
LD = ld
CFLAGS = -Wall -Wpointer-arith -Wextra -Wswitch-enum -Wmissing-prototypes -Wimplicit-fallthrough -Wconversion -fno-strict-aliasing -O3 -std=c11 -pedantic

HEADERS = cf/CodeFusion.h cf/hashmap.h cf/loader.h cf/machine.h cf/opcode.h bridge/bridge.h

IMAGES_SRC = cf/hashmap.c cf/loader.c cf/machine.c cf/opcode.c main.c
TABLES_SRC = interrupt/cross.c

IMAGES_OBJ = $(IMAGES_SRC:.c=.o)
TABLES_OBJ = $(TABLES_SRC:.c=.o)

IMAGE_O = image.o
TABLE_O = table.o

ifdef OS
	OUTDIR = "win/"
else
	OUTDIR = "linux/"
endif

.PHONY: all clean

all: $(IMAGE_O) $(TABLE_O)

$(IMAGE_O): $(IMAGES_OBJ)
	$(LD) -r $^ -o $(OUTDIR)$(IMAGE_O)

$(TABLE_O): $(TABLES_OBJ)
	$(LD) -r $^ -o $(OUTDIR)$(TABLE_O)


%.o: %.c
	$(CC) -c $< -o $@

clean:
	rm -f  $(IMAGES_OBJ) $(TABLES_OBJ)
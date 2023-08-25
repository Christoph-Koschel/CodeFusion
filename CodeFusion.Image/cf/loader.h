#ifndef CF_LOADER_H
#define CF_LOADER_H

#include <stdio.h>
#include <inttypes.h>
#include "machine.h"

#define CURRENT_VERSION 1
#define FLAG_RELOCATABLE 0b1
#define FLAG_EXECUTABLE 0b10
#define FLAG_CONTAINS_ERROR 0b100

typedef struct {
    char magic[3];
    uint16_t version;
    uint8_t flags;
    uint64_t entry_point;
    uint16_t pool_size;
    uint64_t program_size;
} Metadata;

void cf_load_metadata(char* buff, Metadata* metadata);
void cf_load_pool(char* buff, Metadata* metadata);
void cf_load_program(char* buff, Metadata* metadata, CF_Machine* cf);

#endif
#include <memory.h>
#include <stdlib.h>
#include "loader.h"

static void read_buff(void *dst, size_t item_size, size_t count, void *buff) {
    void *dst2 = dst;

    for (size_t i = 0; i < count; ++i) {
        memcpy(dst2, buff, item_size);
        buff += item_size;
        dst2 += item_size;
    }
}

void cf_load_metadata(char *buff, Metadata *metadata) {
    read_buff(metadata->magic, 1, 3, buff);
    read_buff(&metadata->version, sizeof(metadata->version), 1, buff);
    read_buff(&metadata->flags, sizeof(metadata->flags), 1, buff);
    read_buff(&metadata->pool_size, sizeof(metadata->pool_size), 1, buff);
    read_buff(&metadata->program_size, sizeof(metadata->program_size), 1, buff);

    if (metadata->magic[0] != '.' || metadata->magic[1] != 'C' || metadata->magic[2] != 'F') {
        fprintf(stderr, "Program has not the correct file format\n");
        exit(1);
    }
    if (metadata->version != CURRENT_VERSION) {
        fprintf(stderr, "Program is not compatible with the VM file expect '%"PRIu16"' VM has '%"PRIu16"'",
                metadata->version, CURRENT_VERSION);
        exit(1);
    }
}

void cf_load_pool(char *buff, Metadata *metadata) {
    // TODO Implement cf_load_pool function
}

void cf_load_program(char *buff, Metadata *metadata, CF_Machine *cf) {
    // TODO Implement cf_load_program function 
}

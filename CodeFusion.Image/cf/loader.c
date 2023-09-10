#include <memory.h>
#include <stdlib.h>
#include "loader.h"
#include "opcode.h"

static void read_buff(void *dst, size_t item_size, size_t count, void **buff) {
    void *dst2 = dst;

    for (size_t i = 0; i < count; i++) {
        memcpy(dst2, *buff, item_size);
        *buff += item_size;
        dst2 += item_size;
    }
}

void cf_load_metadata(void **buff, Metadata *metadata) {
    read_buff(metadata->magic, 1, 3, buff);
    read_buff(&metadata->version, sizeof(metadata->version), 1, buff);
    read_buff(&metadata->flags, sizeof(metadata->flags), 1, buff);
    read_buff(&metadata->entry_point, sizeof(metadata->entry_point), 1, buff);
    read_buff(&metadata->pool_size, sizeof(metadata->pool_size), 1, buff);
    read_buff(&metadata->program_size, sizeof(metadata->program_size), 1, buff);
    read_buff(&metadata->symbol_size, sizeof(metadata->symbol_size), 1, buff);

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

void cf_load_pool(void **buff, Metadata *metadata, HashMap *pool) {
    for (uint16_t i = 0; i < metadata->pool_size; i++) {
        uint64_t address;
        uint16_t value;
        read_buff(&address, sizeof(Word), 1, buff);
        read_buff(&value, sizeof(uint16_t), 1, buff);

        put_hash_map(pool, address, value);
    }
}

void cf_load_program(void **buff, Metadata *metadata, CF_Library *library) {
    for (size_t i = 0; i < metadata->program_size; i++) {
        Inst inst;
        read_buff(&inst.opcode, 1, 1, buff);
        if (cf_inst_has_operand(inst.opcode)) {
            uint8_t size;
            read_buff(&size, 1, 1, buff);
            if (size == 0) {
                inst.operand = WORD_U64(0);
            } else {
                read_buff(&inst.operand.as_u64, size, 1, buff);
            }
        }
        library->program[library->program_size++] = inst;
    }
}

void cf_load_symbols(void **buff, Metadata *metadata, CF_Library *library) {
    for (size_t i = 0; i < metadata->symbol_size; i++) {
        uint64_t address;
        uint16_t size;
        read_buff(&size, sizeof(uint16_t), 1, buff);
        char *name = malloc(size + 1);
        read_buff(name, 1, size, buff);
        name[size] = '\0';

        read_buff(&address, sizeof(uint64_t), 1, buff);

        library->symbols[library->symbol_size++] = ((CF_Symbol) {
                .name = name,
                .address = address
        });
    }
}

#include "opcode.h"

int cf_inst_has_operand(uint8_t opcode) {
    switch (opcode) {
        case INST_PUSH:
        case INST_LOAD:
        case INST_STORE:
        case INST_MALLOC_POOL:
        case INST_PUSH_PTR:
        case INST_LOAD_PTR:
        case INST_STORE_PTR:
        case INST_DUP:
        case INST_LOAD_ARRAY:
        case INST_STORE_ARRAY:
        case INST_INT:
        case INST_JMP:
        case INST_JMP_ZERO:
        case INST_JMP_NOT_ZERO:
        case INST_CALL:
        case INST_LOAD_MEMORY:
            return 1;
        default:
            return 0;
    }
}

#include "opcode.h"

int cf_inst_has_operand(uint8_t opcode) {
    switch(opcode) {
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
            return 1;
        default:
            return 0;
    }
}
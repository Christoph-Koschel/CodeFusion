#ifndef CF_OPCODE_H
#define CF_OPCODE_H

#include <inttypes.h>

#define INST_NOP ((uint8_t)0)
#define INST_PUSH ((uint8_t)1)
#define INST_POP ((uint8_t)2)
#define INST_LOAD ((uint8_t)3)
#define INST_STORE ((uint8_t)4)
#define INST_MALLOC_POOL ((uint8_t)5)
#define INST_FREE_POOL ((uint8_t)6)
#define INST_PUSH_PTR ((uint8_t)7)
#define INST_LOAD_PTR ((uint8_t)8)
#define INST_STORE_PTR ((uint8_t)9)
#define INST_DUP ((uint8_t)10)
#define INST_PUSH_ARRAY ((uint8_t)11)
#define INST_LOAD_ARRAY ((uint8_t)12)
#define INST_STORE_ARRAY ((uint8_t)13)
#define INST_IADD ((uint8_t)14)
#define INST_FADD ((uint8_t)15)
#define INST_UADD ((uint8_t)16)
#define INST_ISUB ((uint8_t)17)
#define INST_FSUB ((uint8_t)18)
#define INST_USUB ((uint8_t)19)
#define INST_IMUL ((uint8_t)20)
#define INST_FMUL ((uint8_t)21)
#define INST_UMUL ((uint8_t)22)
#define INST_IDIV ((uint8_t)23)
#define INST_FDIV ((uint8_t)24)
#define INST_UDIV ((uint8_t)25)
#define INST_IMOD ((uint8_t)26)
#define INST_FMOD ((uint8_t)27)
#define INST_UMOD ((uint8_t)28)
#define INST_INT ((uint8_t)29)
#define INST_JMP ((uint8_t)30)
#define INST_JMP_ZERO ((uint8_t)31)
#define INST_JMP_NOT_ZERO ((uint8_t)32)
#define INST_CALL ((uint8_t)33)
#define INST_RET ((uint8_t)34)

int cf_inst_has_operand(uint8_t opcode);

#endif

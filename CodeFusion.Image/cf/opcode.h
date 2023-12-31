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
#define INST_ILESS ((uint8_t)29)
#define INST_FLESS ((uint8_t)30)
#define INST_ULESS ((uint8_t) 31)
#define INST_ILESS_EQUAL ((uint8_t)32)
#define INST_FLESS_EQUAL ((uint8_t)33)
#define INST_ULESS_EQUAL ((uint8_t)34)
#define INST_IGREATER ((uint8_t)35)
#define INST_FGREATER ((uint8_t)36)
#define INST_UGREATER ((uint8_t)37)
#define INST_IGREATER_EQUALS ((uint8_t)38)
#define INST_FGREATER_EQUALS ((uint8_t)39)
#define INST_UGREATER_EQUALS ((uint8_t)40)
#define INST_EQ ((uint8_t)41)
#define INST_NEQ ((uint8_t)42)
#define INST_AND ((uint8_t)43)
#define INST_OR ((uint8_t)44)
#define INST_XOR ((uint8_t)45)
#define INST_LSHIFT ((uint8_t)46)
#define INST_RSHIFT ((uint8_t)47)
#define INST_INEG (uint8_t)(49)
#define INST_FNEG (uint8_t)(50)
#define INST_UNEG (uint8_t)(51)
#define INST_NOT (uint8_t)(52)
#define INST_ONES (uint8_t)(53)
#define INST_INT ((uint8_t)54)
#define INST_JMP ((uint8_t)55)
#define INST_JMP_ZERO ((uint8_t)56)
#define INST_JMP_NOT_ZERO ((uint8_t) 57)
#define INST_CALL ((uint8_t)58)
#define INST_VCALL ((uint8_t)59)
#define INST_RET ((uint8_t)60)
#define INST_ITU ((uint8_t)61)
#define INST_ITF ((uint8_t)62)
#define INST_FTI ((uint8_t)63)
#define INST_FTU ((uint8_t)64)
#define INST_UTI ((uint8_t)65)
#define INST_UTF ((uint8_t)66)
#define INST_LOAD_MEMORY ((uint8_t)67)

int cf_inst_has_operand(uint8_t opcode);

#endif

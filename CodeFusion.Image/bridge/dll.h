#ifndef CF_DLL_H
#define CF_DLL_H

#include "../cf/machine.h"

CF_Library cf_load_dll(const char *path);

void cf_free_dll(CF_Library *lib);

#endif
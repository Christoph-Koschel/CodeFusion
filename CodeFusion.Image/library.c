#include <stdlib.h>
#include "cf/CodeFusion.h"

#define OBJECT_EXPORT __declspec(dllexport)

extern char _binary_cf_code_bin_start[];
extern char _binary_cf_code_bin_end[];

OBJECT_EXPORT void init(CF_Library *lib) {
    Metadata metadata = {0};
    void *buff = (void *) _binary_cf_code_bin_start;

    cf_load_metadata(&buff, &metadata);

    if ((metadata.flags & FLAG_LIBRARY) != FLAG_LIBRARY) {
        fprintf(stderr, "DLL is not a CF library\n");
        exit(1);
    }

    lib->address_pool = create_hash_map(metadata.pool_size);
    lib->symbols = malloc(sizeof(CF_Symbol) * metadata.symbol_size);

    cf_load_pool(&buff, &metadata, lib->address_pool);
    cf_load_program(&buff, &metadata, lib);
    cf_load_symbols(&buff, &metadata, lib);
}

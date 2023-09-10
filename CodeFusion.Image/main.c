#include <stdio.h>
#include "cf/CodeFusion.h"

CF_Machine cf = {0};

extern char _binary_cf_code_bin_start[];
extern char _binary_cf_code_bin_end[];

int main(void) {
    Metadata metadata = {0};
    void *buff = (void *) _binary_cf_code_bin_start;
    cf_load_metadata(&buff, &metadata);
    CF_Library main_program = {0};
    main_program.address_pool = create_hash_map(metadata.pool_size);
    cf_load_pool(&buff, &metadata, main_program.address_pool);
    cf_load_program(&buff, &metadata, &main_program);

    cf.libraries[cf.library_size++] = main_program;

    Status status;
    do {
        status = cf_execute_inst(&cf);
#ifdef SLOW
        for (size_t i = 0; i < cf.stack_size; i++) {
            printf("%"PRIu64"\n", cf.stack[i].as_u64);
        }
        getchar();
#endif
    } while (status == STATUS_OK);

    printf("VM stops with code '%x'\n", status);
}

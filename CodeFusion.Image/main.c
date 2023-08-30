#include <stdio.h>
#include "cf/CodeFusion.h"
#include "bridge/interrupt.h"

CF_Machine cf = {0};

extern char _binary_cf_code_bin_start[];
extern char _binary_cf_code_bin_end[];

int main(void) {
    Metadata metadata = {0};
    void *buff = (void *) _binary_cf_code_bin_start;
    cf_load_metadata(&buff, &metadata);
    cf.address_pool = create_hash_map(metadata.pool_size);

    cf_load_pool(&buff, &metadata, &cf);
    cf_load_program(&buff, &metadata, &cf);

    Status status;
    do {
        status = cf_execute_inst(&cf);
    } while (status == STATUS_OK);

    printf("VM stops with code '%x'", status);
}

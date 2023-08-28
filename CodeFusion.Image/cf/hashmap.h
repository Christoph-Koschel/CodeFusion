#ifndef CF_IMAGE_HASHMAP_H
#define CF_IMAGE_HASHMAP_H

#include <inttypes.h>

#define HASH_MAP_SIZE 65536

struct HashNode {
    uint64_t key;
    uint16_t value;
    struct HashNode *next;
};

typedef struct HashNode HashNode;

typedef struct {
    HashNode **buckets;
    size_t size;
} HashMap;

HashMap *create_hash_map(size_t size);

uint16_t get_hash_map(HashMap* map, uint64_t key);

void put_hash_map(HashMap* map, uint64_t key, uint16_t value);


#endif

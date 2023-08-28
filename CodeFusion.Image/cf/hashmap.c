#include <stdlib.h>
#include "hashmap.h"

HashMap *create_hash_map(size_t size) {
    HashMap *map = (HashMap *) malloc(sizeof(HashMap));
    map->buckets = (HashNode **) calloc(size, sizeof(HashNode *));
    map->size = size;

    return map;
}

uint16_t get_hash_map(HashMap *map, uint64_t key) {
    size_t index = key % map->size;
    HashNode *current = map->buckets[index];
    while (current != NULL) {
        if (current->key == key) {
            return current->value;
        }
        current = current->next;
    }
    return 0;
}

void put_hash_map(HashMap *map, uint64_t key, uint16_t value) {
    size_t index = key % map->size;
    HashNode * node = (HashNode*) malloc(sizeof(HashNode));

    node->key = key;
    node->value = value;

    node->next = map->buckets[index];
    map->buckets[index] = node;
}

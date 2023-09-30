#ifndef CF_DEBUG_H
#define CF_DEBUG_H

//#define ENABLE_DEBUG
//#define SLOW


#ifdef ENABLE_DEBUG
#define PRINT_DEBUG(...) printf(__VA_ARGS__)
#else
#define PRINT_DEBUG(...)
#endif
#endif

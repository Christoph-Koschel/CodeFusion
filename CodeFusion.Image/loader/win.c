#include "../bridge/dll.h"
#include "Windows.h"

CF_Library cf_load_dll(const char *path) {
    HMODULE dll = LoadLibrary(path);
    if (dll == NULL) {
        printf("Failed to load DLL: %s\n", path);
        // Print error reason
        LPVOID lpMsgBuf;
        DWORD dw = GetLastError();
        FormatMessage(
                FORMAT_MESSAGE_ALLOCATE_BUFFER |
                FORMAT_MESSAGE_FROM_SYSTEM |
                FORMAT_MESSAGE_IGNORE_INSERTS,
                NULL,
                dw,
                MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
                (LPTSTR) &lpMsgBuf,
                0, NULL);
        printf("%s\n", lpMsgBuf);
        exit(1);
    }
    CF_Library lib = {0};

    void (*init)(CF_Library *) = (void (*)(CF_Library *)) GetProcAddress(dll, "init");
    if (init == NULL) {
        printf("Failed to load init function from DLL: %s\n", path);
        exit(1);
    }
    init(&lib);
    lib.path = path;
    lib.handler = dll;
    return lib;
}

void cf_free_dll(CF_Library *lib) {
    FreeLibrary((HMODULE) lib->handler);
}
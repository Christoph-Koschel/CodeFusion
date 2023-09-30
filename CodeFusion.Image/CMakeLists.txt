# This is just a CMake file for Clion to be able to load the project
# everything is build with make
cmake_minimum_required(VERSION 3.25)
project(CodeFusion_Image C)

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_FLAGS "-Wall -Wpointer-arith -Wextra -Wswitch-enum -Wmissing-prototypes -Wimplicit-fallthrough -Wconversion -fno-strict-aliasing -O3 -std=c11 -pedantic")

add_executable(dummy main.c library.c loader/win.c interrupt/cross.c cf/CodeFusion.h cf/hashmap.c cf/hashmap.h cf/loader.c cf/loader.h cf/machine.c cf/machine.h cf/opcode.c cf/opcode.h bridge/dll.h bridge/interrupt.h
        cf/debug.h)
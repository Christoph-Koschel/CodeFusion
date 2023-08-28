# CodeFusion

## Table of Contents
* [Goals & Features](#goals--features)
* [Dependencies](#dependencies)
    * [Building CodeFusion](#building-codefusion)
    * [Working with CodeFusion](#working-with-codefusion)
* [Tools](#tools)
    * [CodeFusion (Library)](#codefusion-library)
    * [CodeFusion.ASM](#codefusionasm)
    * [CodeFusion.Builder](#codefusionbuilder)
    * [CodeFusion.Dumper](#codefusiondumper)
    * [CodeFusion.Execution](#codefusionexecution)
    * [CodeFusion.Image](#codefusionimage)
* [Structure](#structure)
    * [Program](#program)
    * [Interrupt Table](#interrupt-table)
    * [DLL & LIB Loader](#dll--lib-loader)
    * [VM Image](#vm-image)

## Goals & Features

CodeFusion aims to provide a unique set of features in the realm of Virtual Machines and ByteCode Interpreters. Here are the key goals and features:

* **No Pre-installation Requirement:** CodeFusion eliminates the need for pre-installing specific utilities, such as a particular version of Java, when running programs.

* **Efficient Output:** Unlike other approaches, CodeFusion avoids excessive output production while releasing programs, ensuring clean and concise results.

* **Flexibility and Extensibility:** CodeFusion empowers developers to write both static and dynamic libraries, enabling seamless code interaction with native components through C/C++ integration.

* **Minimalistic Executable Code:** CodeFusion optimizes executable code size by including only essential components, ranging from the VM-Image to the STD-Lib and the program itself.

## Dependencies

To ensure CodeFusion's proper functioning, the following tools are essential:

### Building CodeFusion

* Any C/C++ compiler (e.g., gcc)
* .NET Core 7.0

### Working with CodeFusion

* .NET Core 7.0 Runtime
* The GNU-Linker (ld) (Included in Windows binaries)

## Tools 

### CodeFusion (Library)
This component contains fundamental information like Opcodes and Metadata of CodeFusion. For creating custom tools, it's recommended to leverage the library for foundational definitions.

### CodeFusion.ASM
CodeFusion.ASM serves as the CLI tool for working with assembly files (.cf). It compiles assembly files into executable or relocatable objects. Additionally, it offers functions like object combination.

### CodeFusion.Builder
CodeFusion.Builder functions as the CLI tool for combining an executable object and its VM Image into a native executable for the operating system.

### CodeFusion.Dumper
CodeFusion.Dumper is a utility to extract information from an object file. Note that this tool emerged as a byproduct during development.

### CodeFusion.Execution
CodeFusion.Execution is a compact CodeFusion Interface for executing raw executable object files.

### CodeFusion.Image
This component holds the source code for a basic CodeFusion VM Image.

## Structure

![CodeFusion Structure](assets/structure.png)

### Program
The Program section houses user-written code, along with the standard library when necessary.

### Interrupt Table
The Interrupt Table incorporates all implemented interrupts, including OS-dependent syscalls and custom interrupts coded in native languages.

### DLL & LIB Loader
The DLL and LIB loader's purpose is to load Libraries (**Linux**: .so, .a, **Windows**: .dll, .lib) into the program. Its functionality depends on the image used and the underlying Operating System, with native code implementation.

### VM Image
The VM Image contains the execution code responsible for processing instructions. It's often a compressed version of the standard CodeFusion executable, written in native code.

If the CodeFusion source code is compiled into a library, the image becomes a small bridge connecting the library and the executable.

Each OS requires its own VM Image and a basic Interrupt Table for running CodeFusion programs. But when the image is fully developed, it should be capable of running any existing CodeFusion program of the VM Version. This versatile capability ensures that CodeFusion's VM Image becomes a universal bridge between the executable and the operating system. This guarantees optimal cross-platform functionality.

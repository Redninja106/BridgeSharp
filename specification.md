# Bridge Spec

## Introduction

Bridge is an assembly-like standard for low-level machine code generation. It is designed to be emitted 
by compilers to simplify the process of writing a working programming language. 

A bridge program consists of a series of routines, including one named `main`, which serves as the entry point of the program.
A routine is a series of instructions, each of which have at least opcode, and possibly other arguments.

Bridge can be in both text (.br) and binary (.bll) formats.

All values are 64-bit integers.
## Instructions

This section serves as a reference for bridge instructions and how they are encoded in the text format. For how instructions are encoded in a binary format, see Binary Format.

### PushConst

```
pushconst [value]
```

pushes a constant onto the stack

### Push

### Pop

### Call

### CallIf

### Ret

### Define

### Local

```
local x
```

Explicitly declares a local in the current routine. `local` instructions must be the first local

## Binary Format

The first four bytes of a bridge file are always the string "bll\0", or the bytes 0x62, 0x6C, 0x6C, 0x00. This is the bridge binary file signature.
The file signature is immediately followed by a series of sections, each of which begins with a 1-byte section kind, a 4-byte data size, and then a 
series of bytes whose length is defined by the data size field.
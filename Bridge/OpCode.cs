using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;
public enum OpCode : byte
{
    // does nothing
    Nop,
    // pushes a local, arg, or constant value to the stack
    Push,
    // pops a value from the stack, optionally into a local or arg
    Pop,
    // loads the address of an arg or local onto the stack
    Loada,
    // pops a pointer from the stack and pushes the value at that location
    Load,
    // pops a value from the stack, then a pointer, and writes the value to the pointer
    Store,
    // calls a routine, must point to a routine instruction
    Call,
    // returns from the current routine
    Return,
    // jumps to another place in the routine
    Jump,
    // modifies the instruction immediately following to only execute if a condition is met
    If,
    // pops one value to the stack, converts it to another type, and pushes it back on
    Cast,
    // add two numbers on top of the stack
    Add,
    // subtract two numbers on top of the stack
    Subtract,
    // multiply two numbers on top of the stack
    Multiply,
    // divide two numbers on top of the stack
    Divide,
    // take the remainder of the division of two numbers on top of the stack
    Modulo,
    // pops x, performs 0-x, and pushes result
    Negate,
    // to be implemented
    And,
    // to be implemented
    Or,
    // to be implemented
    Xor,
    // to be implemented
    Not,
    // pops two values off the stack, shifts the second left by the first, and pushes the result
    Shl,
    // pops two values off the stack, shifts the second right by the first, and pushes the result
    Shr,

    // performs a comparason between top two numbers on the stack and pushes an i8 1 if true, or 0 if false
    Compare,

    // prints the top value on the stack to the console
    Print,
}

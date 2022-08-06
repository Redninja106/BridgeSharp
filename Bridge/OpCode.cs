namespace Bridge;
public enum OpCode : byte
{
    // does nothing
    Nop,
    // pushes a local, arg, or constant value to the stack
    Push,
    // pops a value from the stack, optionally into a local or arg
    Pop,
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
    // pops x, pushes x+1
    Increment,
    // pops x, pushes x-1
    Decrement,
    // pops x, y, pushes x & y, float types not supported
    And,
    // pops x, y, pushes x | y, float types not supported
    Or,
    // pops x, y, pushes x ^ y, float types not supported
    Xor,
    // pops x, pushes ~x, float types not supported
    Not,
    // pops x, y, pushes x << y, float types not supported
    ShiftLeft,
    // pops x, y, pushes x >> y, float types not supported
    ShiftRight,
    // performs a comparason between top two numbers on the stack and pushes an i8 1 if true, or 0 if false
    Compare,

    // prints the top value on the stack to the console
    Print,
    PrintChar,
    ReadChar
}

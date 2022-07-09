using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;

public enum OpCode : byte
{
    // core ops
    PushConst,
    Push,
    Pop,
    Call,
    CallIf,
    Ret,
    Define,
    Local,

    // temp (lol)
    Print,

    // arithmetic
    Add,
    Sub,
    Mul,
    Div,
    Mod,

    // comparisons
    // Lt,
    // Lte,
    // Gt,
    // Gte,
    // Eq,
    // Neq,

    // bitwise
    
    // RShift,
    // LShift,
    // And,
    // Or,
}

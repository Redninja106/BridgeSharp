using Bridge.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;
public record Instruction(OpCode OpCode)
{
    public virtual string ToString(Module module, Routine define)
    {
        return this.ToString();
    }

    public override string ToString()
    {
        return OpCode.ToString().ToLower();
    }
}

public record LocalInstruction(OpCode OpCode, LocalAccessMode Mode, byte Local) : Instruction(OpCode)
{
    public override string ToString(Module module, Routine define)
    {
        var prefix = Mode switch
        {
            LocalAccessMode.Address => "@",
            LocalAccessMode.Dereference => ".",
            _ => string.Empty,
        };
        
        return $"{OpCode.ToString().ToLower()} {prefix}{module.GetDataEntryString(define.GetLocalNameEntry(Local))}";
    }
}

public record ConstInstruction(OpCode OpCode, long Value) : Instruction(OpCode)
{
    public override string ToString()
    {
        return $"{OpCode.ToString().ToLower()} {Value}";
    }
}

public record DataEntryInstruction(OpCode OpCode, DataEntry DataEntry) : Instruction(OpCode)
{
    public override string ToString(Module module, Routine define) 
    {
        return $"{OpCode.ToString().ToLower()} {module.GetDataEntryString(DataEntry)}";
    }
}
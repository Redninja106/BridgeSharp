using Bridge.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;

public sealed class CodeBuilder
{
    Routine routine;

    internal CodeBuilder(Routine define)
    {
        this.routine = define;
    }

    public void Emit(OpCode op)
    {
        routine.EmitInstruction(new(op));
    }

    public void EmitPushConst(long value)
    {
        routine.EmitInstruction(new ConstInstruction(OpCode.PushConst, value));
    }
    
    public void EmitPush(string name, LocalAccessMode mode = LocalAccessMode.Normal)
    {
        EmitPush(routine.GetLocal(name), mode);
    }

    public void EmitPush(byte local, LocalAccessMode mode = LocalAccessMode.Normal)
    {
        routine.EmitInstruction(new LocalInstruction(OpCode.Push, mode, local));
    }

    public void EmitPop(byte local, LocalAccessMode mode = LocalAccessMode.Normal)
    {
        routine.EmitInstruction(new LocalInstruction(OpCode.Pop, mode, local));
    }
    
    public void EmitPop(string localName, LocalAccessMode mode = LocalAccessMode.Normal)
    {
        EmitPop(routine.GetLocal(localName), mode);
    }

    public byte EmitLocal(string name)
    {
        return routine.AddLocal(name);
    }

    public void EmitCall(string name)
    {
        EmitCall(this.routine.Module.AddDataEntry(name));
    }

    public void EmitCall(DataEntry name)
    {
        routine.EmitInstruction(new DataEntryInstruction(OpCode.Call, name));
    }

    public bool HasLocal(string name)
    {
        return routine.Locals.Any(e => routine.Module.GetDataEntryString(e) == name);
    }
}

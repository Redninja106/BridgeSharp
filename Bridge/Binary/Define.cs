using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Binary;
public class Routine
{
    public DataEntry Name { get; private set; }
    public List<Instruction> Instructions { get; private set; }
    public List<DataEntry> Locals { get; private set; }
    public Module Module { get; private set; }

    public Routine(Module module, DataEntry name, IEnumerable<DataEntry> locals)
    {
        Module = module;
        Name = name;
        Locals = new(locals ?? Array.Empty<DataEntry>());
        Instructions = new();
    }

    public void EmitInstruction(Instruction instruction)
    {
        Instructions.Add(instruction);
    }

    public byte AddLocal(string name)
    {
        var dataEntry = Module.AddDataEntry(name);

        if (Locals.Contains(dataEntry))
            throw new Exception("This definition already has a local named " + name);

        Locals.Add(dataEntry);
        return (byte)(Locals.Count - 1);
    }

    public byte GetLocal(string name)
    {
        var entry = Locals.First(entry => Module.GetDataEntryString(entry) == name);
        var index = Locals.IndexOf(entry);

        if (index is -1)
            throw new Exception("Local '" + name + "' not found");

        return (byte)index;
    }

    public DataEntry GetLocalNameEntry(byte local)
    {
        return Locals[local];
    }
}

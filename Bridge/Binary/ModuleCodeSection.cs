using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Binary;

public sealed class ModuleCodeSection : ModuleSection
{
    public override SectionKind Kind => SectionKind.Code;

    internal IReadOnlyList<Routine> Defines => defines;

    List<Routine> defines = new();

    public ModuleCodeSection()
    {

    }

    public CodeBuilder AddRoutine(DataEntry nameEntry, IEnumerable<DataEntry> locals = null)
    {
        var define = new Routine(Module, nameEntry, locals);
        defines.Add(define);

        return new CodeBuilder(define);
    }

    protected override void LoadData(Stream stream)
    {
        CodeBuilder builder = null;
        while (true)
        {
            int current = stream.ReadByte();

            if (current == -1)
                break;

            var opCode = (OpCode)current;

            switch (opCode)
            {
                case OpCode.Define:
                    var name = stream.Read<DataEntry>();
                    var localCount = stream.Read<byte>();
                    var locals = new DataEntry[localCount];
                    stream.Read(locals.AsSpan());
                    builder = AddRoutine(name, locals);
                    break;
                case OpCode.PushConst:
                    builder.EmitPushConst(stream.Read<long>());
                    break;
                case OpCode.Push:
                    var mode = stream.Read<LocalAccessMode>();
                    builder.EmitPush(stream.Read<byte>(), mode);
                    break;
                case OpCode.Pop:
                    mode = stream.Read<LocalAccessMode>();
                    builder.EmitPop(stream.Read<byte>(), mode);
                    break;
                case OpCode.Call:
                    builder.EmitCall(stream.Read<DataEntry>());
                    break;
                default:
                    builder.Emit(opCode);
                    break;
            }
        }
    }

    protected override void SaveData(Stream stream)
    {
        foreach (var define in defines)
        {
            stream.Write(OpCode.Define);
            stream.Write(define.Name);
            stream.Write((byte)define.Locals.Count);
            stream.Write(CollectionsMarshal.AsSpan(define.Locals));

            foreach (var instruction in define.Instructions)
            {
                stream.Write(instruction.OpCode);

                switch (instruction)
                {
                    case LocalInstruction loc:
                        stream.Write(loc.Mode);
                        stream.Write(loc.Local);
                        break;
                    case ConstInstruction con:
                        stream.Write(con.Value);
                        break;
                    case DataEntryInstruction dat:
                        stream.Write(dat.DataEntry);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public override void Dump(TextWriter writer)
    {
        var dataSection = Module.GetSection<ModuleDataSection>();

        foreach (var define in defines)
        {
            writer.WriteLine($"define {Module.GetDataEntryString(define.Name)} ({define.Locals.Count} locals)");
            writer.WriteLine("{");
            for (int i = 0; i < define.Instructions.Count; i++)
            {
                writer.WriteLine("    " + define.Instructions[i].ToString(Module, define));
            }
            writer.WriteLine("}");

        }
    }
}

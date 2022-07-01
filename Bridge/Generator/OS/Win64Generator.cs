namespace Bridge;

internal class Win64Generator : OSGenerator
{
    public override IEnumerable<byte> Compile(IEnumerable<Data> data, IEnumerable<Import> imports, IEnumerable<Instruction> instructions)
    {
        List<byte> bytes = new List<byte>();

        bytes.AddRange(CompileHeader(data, imports, instructions));

        bytes.AddRange(CompileInstructions(instructions));

        bytes.AddRange(CompileImports(imports));

        bytes.AddRange(CompileData(data));

        return bytes;
    }

    private IEnumerable<byte> CompileHeader(IEnumerable<Data> data, IEnumerable<Import> imports, IEnumerable<Instruction> instructions)
    {
        return null;
    }

    private IEnumerable<byte> CompileInstructions(IEnumerable<Instruction> instructions)
    {
        ArchGenerator generator = Config.Arch switch
        {
            Config.ArchKind.x86_64 => new X86_64Generator(),
            _ => throw new Exception("Unsupported architecture")
        };

        return generator.Compile(instructions);
    }
    
    private IEnumerable<byte> CompileImports(IEnumerable<Import> imports)
    {
        return null;
    }

    private IEnumerable<byte> CompileData(IEnumerable<Data> data)
    {
        List<byte> bytes = new List<byte>();

        for (int i = 0; i < data.Count(); i++)
            bytes.Add(data.ElementAt(i).Value);

        return bytes;
    }
}
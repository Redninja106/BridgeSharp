namespace Bridge;

internal class Win64Generator : OSGenerator
{
    public override void Compile(Stream stream, IEnumerable<Data> data, IEnumerable<Import> imports, IEnumerable<Instruction> instructions)
    {
        CompileHeader(stream, data, imports, instructions);

        CompileInstructions(stream, instructions);

        CompileImports(stream, imports);

        CompileData(stream, data);
    }

    private void CompileHeader(Stream stream, IEnumerable<Data> data, IEnumerable<Import> imports, IEnumerable<Instruction> instructions)
    {
    }

    private void CompileInstructions(Stream stream, IEnumerable<Instruction> instructions)
    {
        ArchGenerator generator = Config.Arch switch
        {
            Config.ArchKind.x86_64 => new X86_64Generator(),
            _ => throw new Exception("Unsupported architecture")
        };

        generator.Compile(stream, instructions);
        stream.Pad(512);
    }
    
    private void CompileImports(Stream stream, IEnumerable<Import> imports)
    {
    }

    private void CompileData(Stream stream, IEnumerable<Data> data)
    {
        for (int i = 0; i < data.Count(); i++)
            stream.Write(data.ElementAt(i).Value);
        
        stream.Pad(512);
    }
}
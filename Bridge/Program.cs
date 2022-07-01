namespace Bridge;

public class Program
{
    private List<Data> Data { get; set; }
    private List<Import> Imports { get; set; }
    private List<Instruction> Instructions { get; set; }
        
    public Program()
    {
        Data = new();
        Imports = new();
        Instructions = new();
    }

    public void Store(byte value)
    {
        Data.Add(new Data(value));
    }

    public void Import(string name, string file)
    {
        Imports.Add(new Import(name, file));
    }
        
    public void Push(long value) => Instructions.Add(new Push(value));

    public void Pop() => Instructions.Add(new Pop());

    public void Call(long address, params long[] parameters) => Instructions.Add(new Call(address, parameters));

    public void Compile(Stream stream)
    {
        OSGenerator generator = Config.OS switch
        {
            Config.OSKind.win64 => new Win64Generator(),
            _ => throw new Exception("Unsupported operating system")
        };

        generator.Compile(stream, Data, Imports, Instructions);
    }
}
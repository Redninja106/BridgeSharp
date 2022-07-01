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

    public IEnumerable<byte> Compile()
    {
        OSGenerator generator = Config.OS switch
        {
            Config.OSKind.win64 => new Win64Generator(),
            _ => throw new Exception("Unsupported operating system")
        };

        return generator.Compile(Data, Imports, Instructions);
    }
}

// b.Call(b.GetImportAddress("MessageBoxA"), 0, b.GetDataAddress("title"), b.GetDataAddress("content"), 0);

// b.Push(0);
// b.Push(1);
// b.Push(2);
// b.Push(3);
// b.Push(4);
// b.Push(5);
// b.Push(6);
// b.Push(7);

// edx == 3 [2]
// ecx == 2 [1]
// ebx == 1 [3]
// eax == 4 [0]

// STACK
// 0 [4]

// -----

// let x = 10;
// print(x);

// b.Push("x", 10);
// b.GetStackAddress("x");
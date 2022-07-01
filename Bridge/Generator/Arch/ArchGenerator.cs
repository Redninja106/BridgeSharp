namespace Bridge;

internal abstract class ArchGenerator
{
    public abstract IEnumerable<byte> Compile(IEnumerable<Instruction> instructions);

    public abstract IEnumerable<byte> CompilePush(Instruction inst);
    
    public abstract IEnumerable<byte> CompilePop(Instruction inst);
    
    public abstract IEnumerable<byte> CompileCall(Instruction inst);

    private protected Func<Instruction, IEnumerable<byte>> GetHandler(Type type)
    {
        if (type == typeof(Push))
            return CompilePush;
        
        if (type == typeof(Pop))
            return CompilePop;
        
        if (type == typeof(Call))
            return CompileCall;

        throw new ArgumentException("Unsupported type");
    }
}
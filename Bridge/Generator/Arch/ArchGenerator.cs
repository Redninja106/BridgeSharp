namespace Bridge;

internal abstract class ArchGenerator
{
    public abstract void Compile(Stream stream, IEnumerable<Instruction> instructions);

    public abstract void CompilePush(Stream stream, Instruction inst);
    
    public abstract void CompilePop(Stream stream, Instruction inst);
    
    public abstract void CompileCall(Stream stream, Instruction inst);

    private protected Action<Stream, Instruction> GetHandler(Type type)
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
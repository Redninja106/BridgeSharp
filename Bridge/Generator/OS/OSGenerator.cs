namespace Bridge;

internal abstract class OSGenerator
{
    public abstract IEnumerable<byte> Compile(IEnumerable<Data> data, IEnumerable<Import> imports, IEnumerable<Instruction> instructions);
}
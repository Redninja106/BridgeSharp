namespace Bridge;

internal abstract class OSGenerator
{
    public abstract void Compile(Stream stream, IEnumerable<Data> data, IEnumerable<Import> imports, IEnumerable<Instruction> instructions);
}
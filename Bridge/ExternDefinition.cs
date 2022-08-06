namespace Bridge;
public record ExternDefinition(int ID, string Name, string Library, CallingConvention CallingConvention, DataType ReturnType, DataType[] Parameters) : Definition(ID, Name)
{
}
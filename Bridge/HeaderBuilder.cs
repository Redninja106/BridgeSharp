namespace Bridge;
public abstract class HeaderBuilder : DefinitionBuilder
{
    public DataType ReturnType { get; set; }
    public IEnumerable<DataType> Parameters => parameters;

    private readonly List<DataType> parameters = new();

    public HeaderBuilder(ModuleBuilder parent, int id, string name) : base(parent, id, name)
    {
    }
    
    public void AddParameter(DataType parameterType)
    {
        if (parameterType is DataType.Void)
            throw new Exception("A routine can't have void parameters");

        if (parameters.Count >= byte.MaxValue)
            throw new Exception("A routine can't have more than 255 parameters");

        parameters.Add(parameterType);
    }
}

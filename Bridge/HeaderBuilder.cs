using System;
using System.Collections.Generic;

namespace Bridge;
public abstract class HeaderBuilder : DefinitionBuilder
{
    public DataType ReturnType => returnType;
    public IEnumerable<DataType> Parameters => parameters;
    
    private DataType returnType = DataType.Void;
    private readonly List<DataType> parameters = new();

    public HeaderBuilder(ModuleBuilder parent, int id, string name) : base(parent, id, name)
    {
    }
    
    public void SetReturn(DataType returnType)
    {
        this.returnType = returnType;
    }

    public void AddParameter(DataType parameterType)
    {
        if (parameterType is DataType.Void)
            throw new Exception("A routine can't have void parameters");

        if (this.parameters.Count >= byte.MaxValue)
            throw new Exception("A routine can't have more than 255 parameters");

        this.parameters.Add(parameterType);
    }
}

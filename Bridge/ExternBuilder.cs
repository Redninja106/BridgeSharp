using System.Linq;

namespace Bridge;

public sealed class ExternBuilder : HeaderBuilder
{
    public string Library { get; set; }
    public CallingConvention CallingConvention { get; set; }

    public ExternBuilder(ModuleBuilder parent, int id, string name) : base(parent, id, name)
    {
    }

    public ExternDefinition CreateExtern()
    {
        return new(this.ID, this.Name, this.Library, this.CallingConvention, this.ReturnType, this.Parameters.ToArray());
    }

    protected override void OnClose(ModuleBuilder moduleBuilder)
    {
        moduleBuilder.AddDefinition(CreateExtern());
        base.OnClose(moduleBuilder);
    }
}
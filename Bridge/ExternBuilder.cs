using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;

public sealed class ExternBuilder : HeaderBuilder
{
    public string Library { get; set; }

    public ExternBuilder(ModuleBuilder parent, int id, string name) : base(parent, id, name)
    {
    }

    public ExternDefinition CreateExtern()
    {
        return new(this.ID, this.Name, this.Library, this.ReturnType, this.Parameters.ToArray());
    }

    protected override void OnClose(ModuleBuilder moduleBuilder)
    {
        moduleBuilder.AddDefinition(CreateExtern());
        base.OnClose(moduleBuilder);
    }
}
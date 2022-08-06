using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Text;
/// <summary>
/// Represents a parsed bridge text file.
/// </summary>
internal sealed record Document(DocumentDefinition[] Definitions)
{
    public void Build(ModuleBuilder builder)
    {
        var context = new DocumentBuildContext(this);
        
        foreach (var definition in Definitions)
        {
            definition.Build(context, builder);
        }
    }
}

using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Text;

internal sealed record DocumentRoutine(DocumentDefinitionHeader Header, DocumentInstruction[] Instructions) : DocumentDefinition()
{
    public override void Build(DocumentBuildContext context, ModuleBuilder builder)
    {
        var routineBuilder = builder.AddRoutine(Header.Name.ToStringValue());
        context.Definitions.Add(routineBuilder.Name, routineBuilder);

        Header.Build(context, routineBuilder);

        var codeBuilder = routineBuilder.GetCodeBuilder();
        foreach (var instruction in Instructions)
        {
            instruction.Build(context, codeBuilder);
        }

    }
}

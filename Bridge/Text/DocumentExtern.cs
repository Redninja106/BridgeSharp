using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Text;

internal record DocumentExtern() : DocumentDefinition
{
    public override void Build(DocumentBuildContext context, ModuleBuilder builder)
    {
    }
}

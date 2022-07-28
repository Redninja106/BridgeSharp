using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;

public class ModuleBuilder
{
    private readonly List<IBuilder> builders = new();
    private readonly List<Definition> definitions = new();

    public RoutineBuilder AddRoutine(string name)
    {
        var builder = new RoutineBuilder(this, Definition.GetNextID(), name);

        builders.Add(builder);

        return builder;
    }
    
    public ExternBuilder AddExtern(string name)
    {
        var builder = new ExternBuilder(this, Definition.GetNextID(), name);

        builders.Add(builder);
        
        return builder;
    }

    public Module CreateModule()
    {
        while (builders.Any())
        {
            builders.First().Close();
        }

        return new Module(definitions);
    }

    internal void OnBuilderCompleted(IBuilder builder)
    {
        builders.Remove(builder);
    }

    internal void AddDefinition(Definition definition)
    {
        this.definitions.Add(definition);
    }
}

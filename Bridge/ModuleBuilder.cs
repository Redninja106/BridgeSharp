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
    private readonly List<(ResourceKind, byte[])> resources = new();

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
    
    public Index AddResource(string resource)
    {
        return AddResource(resource, Encoding.UTF8);
    }

    public Index AddResource(string resource, Encoding encoding)
    {
        Span<byte> bytes = stackalloc byte[encoding.GetByteCount(resource)];
        encoding.GetBytes(resource, bytes);
        return AddResource(bytes, encoding.BodyName switch
        {
            "utf-8" => ResourceKind.String8,
            "utf-16" => ResourceKind.String16,
            _ => ResourceKind.Unknown
        });
    } 
    
    public Index AddResource(ReadOnlySpan<byte> bytes)
    {
        return AddResource(bytes, ResourceKind.Unknown);
    }

    public Index AddResource(ReadOnlySpan<byte> bytes, ResourceKind kind)
    {
        var index = (Index)resources.Count;

        var resource = new byte[bytes.Length];
        bytes.CopyTo(resource);

        resources.Add((kind, resource));

        return index;
    }

    public Module CreateModule()
    {
        while (builders.Any())
        {
            builders.First().Close();
        }

        return new Module(definitions, resources);
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

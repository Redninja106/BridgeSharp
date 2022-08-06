using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bridge;

/* The ModuleBuilder class is the root in a sort of hierarchy of builders,
 * where any it creates become it's children. That means that whenever a 
 * builder is closed, it also needs to close any builders below it to make 
 * sure their are properly included it's product.
 */

/// <summary>
/// Provides an API for creating modules.
/// </summary>
public sealed class ModuleBuilder
{
    // the name of the module, right now module naming is not used for much
    private string name = "module";
    
    // list of this object's active builders, so that they can all be closed this object
    private readonly List<IBuilder> builders = new();
    
    // finished definitions (routines, externs, inlines)
    private readonly List<Definition> definitions = new();
    
    // resource kind+data pairs to add to final module
    private readonly List<ResourceTableEntry> resources = new();

    /// <summary>
    /// Adds a routine to the module with the provided name.
    /// </summary>
    /// <param name="name">The name of the routine.</param>
    /// <returns>A builder for the routine.</returns>
    public RoutineBuilder AddRoutine(string name)
    {
        var builder = new RoutineBuilder(this, Definition.GetNextID(), name);

        builders.Add(builder);

        return builder;
    }
    
    /// <summary>
    /// Adds an extern to the module with the provided name.
    /// </summary>
    /// <param name="name">The name of the extern.</param>
    /// <returns>A builder for the extern.</returns>
    public ExternBuilder AddExtern(string name)
    {
        var builder = new ExternBuilder(this, Definition.GetNextID(), name);

        builders.Add(builder);
        
        return builder;
    }

    /// <summary>
    /// Adds a UTF-8 string resource to the module.
    /// </summary>
    /// <param name="resource">The string data making up the resource.</param>
    /// <returns>The resource's index into the resource table</returns>
    public Index AddResource(string resource)
    {
        return AddResource(resource, Encoding.UTF8);
    }

    /// <summary>
    /// Adds a string resource to the module with the provided encoding.
    /// </summary>
    /// <param name="resource">The string data making up the resource.</param>
    /// <param name="encoding">The string encoding of resource.</param>
    /// <returns>The resource's index into the resource table</returns>
    public Index AddResource(string resource, Encoding encoding)
    {
        // alloc bytes for raw string data
        Span<byte> bytes = stackalloc byte[encoding.GetByteCount(resource)];

        // convert string into bytes
        encoding.GetBytes(resource, bytes);

        // try to figure out resource kind from encoding
        var kind = encoding.BodyName switch
        {
            "utf-8" => ResourceKind.String8,
            "utf-16" => ResourceKind.String16,
            _ => ResourceKind.Bytes
        };

        return AddResource(bytes, kind);
    }
    
    /// <summary>
    /// Adds a resource to the module.
    /// </summary>
    /// <param name="bytes">The resource data.</param>
    /// <returns>The resource's index into the resource table.</returns>
    public Index AddResource(ReadOnlySpan<byte> bytes)
    {
        return AddResource(bytes, ResourceKind.Bytes);
    }

    /// <summary>
    /// Adds a resource to the module.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="kind"></param>
    /// <returns>The resource's index into the resource table.</returns>
    public Index AddResource(ReadOnlySpan<byte> bytes, ResourceKind kind)
    {
        // resource will be added to end of list, so our index will just be the size of the list
        var index = (Index)resources.Count;

        // copy the resoure data into a new array
        var resource = new byte[bytes.Length];
        bytes.CopyTo(resource);

        // add to resource entry list
        resources.Add(new(kind, resource));

        return index;
    }

    public Module CreateModule()
    {
        // close all builders
        while (builders.Any())
        {
            builders.First().Close();
        }

        // create the module
        return new Module(name, definitions, resources);
    }

    // dependent builders use this to notify us that they are were closed, and that we no longer need to close them
    internal void OnBuilderCompleted(IBuilder builder)
    {
        builders.Remove(builder);
    }

    // allows dependent builders to give us the result here
    internal void AddDefinition(Definition definition)
    {
        this.definitions.Add(definition);
    }
}

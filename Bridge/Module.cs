using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;

public sealed class Module
{
    private Definition[] definitions;

    public ResourceTable Resources { get; }

    public IEnumerable<Definition> Definitions => definitions;
    public IEnumerable<RoutineDefinition> Routines => definitions.OfType<RoutineDefinition>();
    public IEnumerable<ExternDefinition> Externs => definitions.OfType<ExternDefinition>();

    internal Module(IEnumerable<Definition> declarations, IEnumerable<byte[]> resources)
    {
        Resources = new(resources);
        this.definitions = declarations.ToArray();
    }

    public static ModuleBuilder Create()
    {
        return new ModuleBuilder();
    }
    public static Module Load(string path) => throw new NotImplementedException();
    public static Module Load(Stream stream) => throw new NotImplementedException();
    public static Module Parse(string file)
    {
        using var fs = File.OpenRead(file);
        using var reader = new StreamReader(fs);
        return Parse(reader);
    }
    public static Module Parse(TextReader reader)
    {
        var parser = new Text.Parser();
        parser.AddSource(reader.ReadToEnd());
        return parser.CreateModule();
    }
    public static Module Link(params Module[] modules) => throw new NotImplementedException();
    public static void Save(Module module, string path) => throw new NotImplementedException();
    public static void Save(Module module, Stream stream) => throw new NotImplementedException();

}

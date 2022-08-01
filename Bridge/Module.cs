using Bridge.Text;
using Bridge.Verification;
using System.Reflection;
using System.Reflection.Emit;

namespace Bridge;

public sealed class Module
{
    private Definition[] definitions;

    public string Name { get; }
    public ResourceTable ResourceTable { get; }

    public IEnumerable<Definition> Definitions => definitions;
    public IEnumerable<RoutineDefinition> Routines => definitions.OfType<RoutineDefinition>();
    public IEnumerable<ExternDefinition> Externs => definitions.OfType<ExternDefinition>();

    internal Module(string name, IEnumerable<Definition> declarations, IEnumerable<ResourceTableEntry> resources)
    {
        this.Name = name;
        ResourceTable = new(resources);
        this.definitions = declarations.ToArray();
    }

    public Definition FindDefinition(int id)
    {
        return definitions.Single(def => def.ID == id);
    }

    public static ModuleBuilder CreateBuilder()
    {
        return new ModuleBuilder();
    }

    public static Module Load(string path) => throw new NotImplementedException();
    public static Module Load(Stream stream) => throw new NotImplementedException();
    public static Module Parse(string file) => Parse(file, out _);
    public static Module Parse(string file, out string[] errors)
    {
        using var fs = File.OpenRead(file);
        using var reader = new StreamReader(fs);
        return Parse(reader, out errors);
    }
    public static Module Parse(TextReader reader) => Parse(reader, out _);
    public static Module Parse(TextReader reader, out string[] errors)
    {
        var builder = CreateBuilder();

        var parser = new Parser(builder);
        if (parser.TryParse(reader.ReadToEnd(), out Document document, out errors))
        {
            document.Build(builder);
            return builder.CreateModule();
        }
        else
        {
            return null;
        }
    }
    public static Module Link(params Module[] modules) => throw new NotImplementedException();
    public static void Save(Module module, string path) => throw new NotImplementedException();
    public static void Save(Module module, Stream stream) => throw new NotImplementedException();
    public static void Dump(Module module, TextWriter writer)
    {
        var modWriter = new ModuleWriter(module);
        modWriter.WriteModule(writer);
    }
    public static MethodInfo Compile(Module module)
    {
        return Compile(module, out _);
    }

    public static MethodInfo Compile(Module module, out Assembly assembly)
    {
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("bridge"), AssemblyBuilderAccess.RunAndCollect);
        var compiler = new CILCompiler(module);
        MethodInfo main = compiler.Compile(assemblyBuilder);
        assembly = assemblyBuilder;
        return main;
    }

    public static bool Verify(Module module, out VerificationMessage[] messages)
    {
        messages = Verifier.Verify(module);
        return !messages.HasErrors();
    }
}

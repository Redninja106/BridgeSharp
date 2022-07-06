using Bridge.Binary;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;

/// <summary>
/// Provides ways to save, load, create, and modify bridge files.
/// </summary>
public sealed class Module
{
    // file signature, identifies a bridge binary file (.bll)
    private static readonly byte[] bridgeSignature = new byte[] { (byte)'b', (byte)'r', 0, 0 };

    // each module is an ordered set of sections
    private readonly ModuleSection[] sections;

    /// <summary>
    /// A list of the module's sections.
    /// </summary>
    public IReadOnlyList<ModuleSection> Sections => sections;

    // initializes a new empty module (i.e. no data or code, header fields all set to 0)
    private Module()
    {
        // right now there are always 3 sections
        sections = new ModuleSection[3];
        sections[0] = new ModuleHeader() { Module = this };
        sections[1] = new ModuleDataSection() { Module = this };
        sections[2] = new ModuleCodeSection() { Module = this };
    }

    /// <summary>
    /// Creates a new empty module.
    /// </summary>
    public static Module Create()
    {
        // can do argument processing here later (module name, flags/options)
        // here should be stuff that should only happen when a new module is created (not loaded)
        // since the constructor is called in load as well
        return new Module();
    }

    /// <summary>
    /// Reads a bridge module from file.
    /// </summary>
    public static Module Load(string path)
    {
        using var fs = File.OpenRead(path);
        return Load(fs);
    }

    /// <summary>
    /// Reads a bridge module from a binary stream.
    /// </summary>
    public static Module Load(Stream stream)
    {
        // read and check file signature
        Span<byte> bytes = stackalloc byte[4];
        stream.Read(bytes);
        if (!bytes.SequenceEqual(bridgeSignature))
        {
            throw new Exception("File is not a valid bridge binary.");
        }

        // create an empty module and load it's sections from the stream
        Module module = new();

        foreach (var section in module.sections)
        {
            section.Load(stream);
        }

        return module;
    }

    /// <summary>
    /// Writes a bridge module to a file.
    /// </summary>
    public static void Save(string path, Module module)
    {
        using var fs = File.Create(path);
        Save(fs, module);
    }

    /// <summary>
    /// Writes a bridge module to a binary stream.
    /// </summary>
    public static void Save(Stream stream, Module module)
    {
        // writes the file signature then each section in order

        stream.Write(bridgeSignature);

        foreach (var section in module.sections)
        {
            section.Save(stream);
        }
    }

    /// <summary>
    /// Combines an array modules into a single module.
    /// </summary>
    public static Module Link(params Module[] modules)
    {
        // this is going to be alot of work
        // first create a result module
        // next merge the data sections, this is not as simple as appending them together, since we also need to update every entry to point to the same data

        throw new NotImplementedException();
    }

    public static Module Parse(string source)
    {
        return null;
    }

    /// <summary>
    /// Returns this module's section of the specified type or null if the module has no section of that type.
    /// </summary>
    internal T GetSection<T>() where T : ModuleSection
    {
        return sections.FirstOrDefault(s => s is T) as T;
    }

    /// <summary>
    /// Adds a string to the module's data section as UTF8 bytes. 
    /// <para>
    /// If the module already has an identical value, it is not added and that entry is retured.
    /// </para>
    /// </summary>
    public DataEntry AddDataEntry(string dataString)
    {
        var length = Encoding.UTF8.GetByteCount(dataString);
        Span<byte> bytes = stackalloc byte[length];
        Encoding.UTF8.GetBytes(dataString, bytes);
        return AddDataEntry(bytes);
    }
    
    /// <summary>
    /// Adds a span of bytes to the module's data section. 
    /// <para>
    /// If the module already has an identical value, it is not added and that entry is retured.
    /// </para>
    /// </summary>
    public DataEntry AddDataEntry(Span<byte> data)
    {
        return GetSection<ModuleDataSection>().AddEntry(data);
    }

    /// <summary>
    /// Reads a UTF8 string from the module's data section. 
    /// </summary>
    public string GetDataEntryString(DataEntry entry)
    {
        return Encoding.UTF8.GetString(GetDataEntry(entry));
    }

    /// <summary>
    /// Retrieves a span of bytes from the module's data section.
    /// </summary>
    /// <param name="entry">The <see cref="DataEntry"/> whose bytes to retrieve.</param>
    public ReadOnlySpan<byte> GetDataEntry(DataEntry entry)
    {
        return GetSection<ModuleDataSection>().GetEntry(entry);
    }

    /// <summary>
    /// Adds a new routine to the module's code section.
    /// </summary>
    /// <param name="name">The name of the routine. This value is added to the data section if it is not already present.</param>
    /// <returns>A <see cref="CodeBuilder"/> which can be used to create a body for the new routine.</returns>
    public CodeBuilder EmitRoutine(string name)
    {
        var nameEntry = AddDataEntry(name);
        return EmitRoutine(nameEntry);
    }

    /// <summary>
    /// Adds a new routine to the module's code section.
    /// </summary>
    /// <param name="nameEntry">An entry in this module's data section which contains the name of the routine.</param>
    /// <returns>A <see cref="CodeBuilder"/> which can be used to create a body for the new routine.</returns>
    /// <returns></returns>
    public CodeBuilder EmitRoutine(DataEntry nameEntry)
    {
        // simply pass the name entry on to the code section
        var code = GetSection<ModuleCodeSection>();
        return code.AddRoutine(nameEntry);
    }
    
    /// <summary>
    /// Writes a multi-line summary of this module to a <see cref="TextWriter"/>.
    /// </summary>
    public void Dump(TextWriter writer)
    {
        writer.WriteLine("Module: ");
        foreach (var section in sections)
        {
            writer.WriteLine();
            section.Dump(writer);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Binary;

public abstract class ModuleSection
{
    public abstract SectionKind Kind { get; }
    public Module Module { get; internal init; }

    protected abstract void LoadData(Stream stream);
    protected abstract void SaveData(Stream stream);

    public virtual void Dump(TextWriter writer)
    {
        writer.WriteLine(ToString());
    }

    internal void Save(Stream stream)
    {
        var ms = new MemoryStream();
        SaveData(ms);

        var data = ms.GetBuffer().AsSpan(0..(int)ms.Position);

        stream.Write(Kind);
        stream.Write((uint)data.Length);
        stream.Write(data);
    }

    internal void Load(Stream stream)
    {
        var kind = stream.Read<SectionKind>();

        if (kind != Kind)
            throw new Exception($"Unexpected {kind} chunk");

        var size = stream.Read<uint>();

        var data = new byte[size];
        stream.Read(data);

        using var ms = new MemoryStream(data);
        LoadData(ms);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Binary;

public sealed class ModuleHeader : ModuleSection
{
    public uint MajorVersion { get; private set; }
    public uint MinorVersion { get; private set; }

    public override SectionKind Kind => SectionKind.Header;

    public ModuleHeader()
    {
        MajorVersion = MinorVersion = 0;
    }

    protected override void LoadData(Stream stream)
    {
        MajorVersion = stream.Read<uint>();
        MinorVersion = stream.Read<uint>();
    }

    protected override void SaveData(Stream stream)
    {
        stream.Write(MajorVersion);
        stream.Write(MinorVersion);
    }

    public override void Dump(TextWriter writer)
    {
        writer.WriteLine("Header: ");
        writer.WriteLine("Major Version: " + MajorVersion);
        writer.WriteLine("Minor Version: " + MinorVersion);
    }
}
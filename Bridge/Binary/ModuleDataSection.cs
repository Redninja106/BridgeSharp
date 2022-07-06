using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Binary;

public sealed class ModuleDataSection : ModuleSection
{
    private Range[] entries;
    private byte[] data;

    public override SectionKind Kind => SectionKind.Data;

    public ModuleDataSection()
    {
        entries = Array.Empty<Range>();
        data = Array.Empty<byte>();
    }

    public ReadOnlySpan<byte> GetEntry(DataEntry entry)
    {
        var range = entries[entry.value];
        return data.AsSpan(range);
    }

    public DataEntry AddEntry(Span<byte> bytes)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            var (_, length) = entry.GetOffsetAndLength(data.Length);
            if (length == bytes.Length)
            {
                var span = data.AsSpan(entry);
                if (span.SequenceEqual(bytes))
                {
                    return new(i);
                }
            }
        }

        int begin = data.Length;
        Array.Resize(ref data, data.Length + bytes.Length);
        int end = data.Length;
        bytes.CopyTo(data.AsSpan(begin..end));

        int newEntry = entries.Length;
        Array.Resize(ref entries, entries.Length + 1);
        entries[newEntry] = begin..end;
        return new(newEntry);
    }

    protected override void LoadData(Stream stream)
    {
        uint entryCount = stream.Read<uint>();
        entries = new Range[entryCount];
        stream.Read<Range>(entries);

        uint dataSize = stream.Read<uint>();
        data = new byte[dataSize];
        stream.Read(data);
    }

    protected override void SaveData(Stream stream)
    {
        stream.Write((uint)entries.Length);
        stream.Write(entries.AsSpan());

        stream.Write((uint)data.Length);
        stream.Write(data.AsSpan());
    }

    public override void Dump(TextWriter writer)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            writer.WriteLine($"{i} ({entry.Start}..{entry.End}): {Encoding.UTF8.GetString(GetEntry(i))}");
        }
    }
}

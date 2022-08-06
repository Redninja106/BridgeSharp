using System;
using System.Collections.Generic;
using System.Text;

namespace Bridge;

public sealed class ResourceTable
{
    internal List<ResourceTableEntry> entries = new();

    public int EntryCount => entries.Count;
    public IEnumerable<ResourceTableEntry> Entries => entries;

    internal ResourceTable(IEnumerable<ResourceTableEntry> resources)
    {
        this.entries = new(resources);
    }

    public ResourceKind GetKind(Index resource)
    {
        return entries[resource].Kind;
    }

    public Index Find(string resource)
    {
        return Find(resource, Encoding.Default);
    }
    
    public Index Find(string resource, Encoding encoding)
    {
        Span<byte> bytes = stackalloc byte[encoding.GetByteCount(resource)];
        encoding.GetBytes(resource, bytes);
        return Find(bytes);
    }
    
    public Index Find(Span<byte> bytes)
    {
        return TryFind(bytes, out Index index) ? index : throw new Exception("Resource Not Found!");
    }

    public bool TryFind(string resource, out Index index)
    {
        return TryFind(resource, Encoding.Default, out index);
    }

    public bool TryFind(string resource, Encoding encoding, out Index index)
    {
        Span<byte> bytes = stackalloc byte[encoding.GetByteCount(resource)];
        encoding.GetBytes(resource, bytes);
        return TryFind(bytes, out index);
    }

    public bool TryFind(Span<byte> bytes, out Index index)
    {
        index = default;
        foreach (var value in entries)
        {
            if (bytes.SequenceEqual(value.Data))
            { 
                index = entries.IndexOf(value);
                return true;
            } 
        }

        return false;
    }
    
    public ResourceTableEntry GetResource(Index index)
    {
        return entries[index];
    }

    public ReadOnlySpan<byte> GetResourceBytes(Index index)
    {
        return GetResourceBytes(index, out _);
    }

    public ReadOnlySpan<byte> GetResourceBytes(Index index, out ResourceKind kind)
    {
        byte[] bytes;
        (kind, bytes) = GetResource(index);
        return bytes;
    }

    public string GetResourceString(Index index)
    {
        return GetResourceString(index, Encoding.Default);
    }

    public string GetResourceString(Index index, Encoding encoding)
    {
        return encoding.GetString(GetResourceBytes(index));
    }
}
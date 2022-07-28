using System.Text;

namespace Bridge;

public sealed class ResourceTable
{
    List<byte[]> resources = new();

    internal ResourceTable(IEnumerable<byte[]> resources)
    {
        this.resources = new(resources);
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
        foreach (var value in resources)
        {
            if (bytes.SequenceEqual(value))
            { 
                index = resources.IndexOf(value);
                return true;
            } 
        }

        return false;
    }

    public ReadOnlySpan<byte> GetResource(Index index)
    {
        return resources[index];
    }

    public string GetResourceString(Index index)
    {
        return GetResourceString(index, Encoding.Default);
    }

    public string GetResourceString(Index index, Encoding encoding)
    {
        return encoding.GetString(GetResource(index));
    }
}
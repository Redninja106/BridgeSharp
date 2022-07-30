using System.Text;

namespace Bridge;

public sealed class ResourceTable
{
    List<(ResourceKind, byte[])> resources = new();

    internal ResourceTable(IEnumerable<(ResourceKind, byte[])> resources)
    {
        this.resources = new(resources);
    }

    public ResourceKind GetKind(Index resource)
    {
        return resources[resource].Item1;
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
        foreach (var (kind, value) in resources)
        {
            if (bytes.SequenceEqual(value))
            { 
                index = resources.IndexOf((kind,value));
                return true;
            } 
        }

        return false;
    }
    
    public ReadOnlySpan<byte> GetResource(Index index)
    {
        return GetResource(index, out _);
    }

    public ReadOnlySpan<byte> GetResource(Index index, out ResourceKind kind)
    {
        byte[] result; 
        (kind, result) = resources[index];
        return result;
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
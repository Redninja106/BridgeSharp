namespace Bridge;

public record struct ResourceTableEntry(ResourceKind Kind, byte[] Data)
{
    //public void Deconstruct(out ResourceKind kind, out byte[] data)
    //{
    //    kind = Kind;
    //    data = Data;
    //}
}
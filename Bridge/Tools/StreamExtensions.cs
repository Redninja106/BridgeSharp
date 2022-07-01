namespace Bridge;

internal static class StreamExtensions
{
    public static unsafe void Write<T>(this Stream stream, T value) where T : unmanaged
    {
        Span<byte> bytes = new Span<byte>(&value, sizeof(T));
        stream.Write(bytes);
    }

    public static void Pad(this Stream stream, int buffer)
    {
        while (stream.Length % buffer != 0)
            stream.WriteByte(0);
    }
}
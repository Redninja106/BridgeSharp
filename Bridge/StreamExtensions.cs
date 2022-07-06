using System.Runtime.CompilerServices;

namespace Bridge;

internal unsafe static class StreamExtensions
{
    public static void Write<T>(this Stream stream, T value) where T : unmanaged
    {
        stream.Write(ref value);
    }

    public static void Write<T>(this Stream stream, ref T value) where T : unmanaged
    {
        Span<byte> bytes = new(Unsafe.AsPointer(ref value), sizeof(T));
        stream.Write(bytes);
    }

    public static void Write<T>(this Stream stream, Span<T> values) where T : unmanaged
    {
        for (int i = 0; i < values.Length; i++)
        {
            Write(stream, ref values[i]);
        }
    }

    public static T Read<T>(this Stream stream) where T : unmanaged
    {
        T value;
        Span<byte> bytes = new(&value, sizeof(T));
        stream.Read(bytes);
        return value;
    }

    public static void Read<T>(this Stream stream, Span<T> values) where T : unmanaged
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = Read<T>(stream);
        }
    }

    public static void Pad(this Stream stream, int buffer)
    {
        while (stream.Length % buffer != 0)
            stream.WriteByte(0);
    }
}
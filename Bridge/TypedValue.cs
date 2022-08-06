using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Bridge;

/// <summary>
/// Represents a type-value pair, with utilities for reading a proper value.
/// </summary>
/// <param name="Bits">The raw data of the value, 0-padded to 64 bits.</param>
/// <param name="Type">The type of the value.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct TypedValue(ulong Bits, DataType Type)
{
    /// <summary>
    /// Returns the size of the value in bytes.
    /// This property does not return the size of <see cref="TypedValue"/>.
    /// </summary>
    public int Size => GetDataTypeSize(Type);

    public unsafe readonly Span<byte> GetBytes()
    {
        var ptr = (byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
        return new Span<byte>(ptr, Size);
    }
    
    /// <summary>
    /// Interprets this value's raw bytes as the provided primitive.
    /// </summary>
    public readonly T As<T>() where T : unmanaged
    {
        ulong bits = Bits;
        return Unsafe.As<ulong, T>(ref bits);
    }

    /// <summary>
    /// Returns <see langword="true"/> if this object's type matches the provided primitive, otherwise <see langword="false"/>.
    /// </summary>
    public readonly bool Is<T>() where T : unmanaged
    {
        return GetDataType<T>() == this.Type;
    }

    /// <summary>
    /// Creates a new <see cref="TypedValue"/> from the provided primitive.
    /// <para>
    /// Valid primitives are:
    /// <see langword="long"/>,
    /// <see langword="int"/>,
    /// <see langword="short"/>,
    /// <see langword="sbyte"/>,
    /// <see langword="ulong"/>,
    /// <see langword="uint"/>,
    /// <see langword="ushort"/>,
    /// <see langword="byte"/>,
    /// <see langword="double"/>,
    /// <see langword="float"/>,
    /// and <see langword="nuint"/>
    /// </para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static TypedValue Create<T>(T value) where T : unmanaged
    {
        if (GetDataType<T>() is DataType.Void)
        {
            throw new ArgumentException("Invalid Primitive!");
        }

        ulong bits = 0;
        var size = Unsafe.SizeOf<T>();
        Unsafe.CopyBlock(ref Unsafe.As<ulong, byte>(ref bits), ref Unsafe.As<T, byte>(ref value), (uint)size);
        return new(bits, GetDataType<T>());
    }

    /// <summary>
    /// Creates a new <see cref="TypedValue"/> with the default value of the provided type.
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static TypedValue CreateDefault(DataType dataType)
    {
        return dataType switch
        {
            DataType.I64 => Create<long>(default),
            DataType.I32 => Create<int>(default),
            DataType.I16 => Create<short>(default),
            DataType.I8 => Create<sbyte>(default),
            DataType.U64 => Create<ulong>(default),
            DataType.U32 => Create<uint>(default),
            DataType.U16 => Create<ushort>(default),
            DataType.U8 => Create<byte>(default),
            DataType.F64 => Create<double>(default),
            DataType.F32 => Create<float>(default),
            DataType.Pointer => Create<nuint>(default),
            DataType.Void => Create<ulong>(default),
            _ => throw new ArgumentException("Invalid DataType!", nameof(dataType)),
        };
    }

    /// <summary>
    /// Returns the dataType which most closely matches the given .NET primitive, or <see cref="DataType.Void"/> if no match is found.
    /// </summary>
    public static DataType GetDataType<T>() where T : unmanaged
    {
        return default(T) switch
        {
            long => DataType.I64,
            int => DataType.I32,
            short => DataType.I16,
            sbyte => DataType.I8,
            ulong => DataType.U64,
            uint => DataType.U32,
            ushort => DataType.U16,
            byte => DataType.U8,
            double => DataType.F64,
            float => DataType.F32,
            nuint => DataType.Pointer,
            _ => DataType.Void,
        };
    }

    /// <summary>
    /// Returns the size of a value of the given data type, or 0 if the type is invalid or <see cref="DataType.Void"/>.
    /// </summary>
    public static int GetDataTypeSize(DataType type)
    {
        return type switch
        {
            DataType.I64 or DataType.U64 or DataType.F64 => 8,
            DataType.I32 or DataType.U32 or DataType.F32 => 4,
            DataType.I16 or DataType.U16 => 2,
            DataType.I8 or DataType.U8 => 1,
            DataType.Pointer => Unsafe.SizeOf<nuint>(),
            _ => 0
        };
    }

    /// <summary>
    /// Returns the type of the .NET primitive which most closely matches the given data type, or null if no match is found or the type is <see cref="DataType.Void"/>.
    /// </summary>
    public static Type GetDataTypePrimitive(DataType type)
    {
        return type switch
        {
            DataType.I64 => typeof(long),
            DataType.I32 => typeof(int),
            DataType.I16 => typeof(short),
            DataType.I8 => typeof(sbyte),
            DataType.U64 => typeof(ulong),
            DataType.U32 => typeof(uint),
            DataType.U16 => typeof(ushort),
            DataType.U8 => typeof(byte),
            DataType.F64 => typeof(double),
            DataType.F32 => typeof(float),
            DataType.Pointer => typeof(nuint),
            _ => null
        };
    }

    public override string ToString()
    {
        switch (this.Type)
        {
            case DataType.I64: return As<long>().ToString();
            case DataType.I32: return As<int>().ToString();
            case DataType.I16: return As<short>().ToString();
            case DataType.I8: return As<sbyte>().ToString();
            case DataType.U64: return As<ulong>().ToString();
            case DataType.U32: return As<uint>().ToString();
            case DataType.U16: return As<short>().ToString();
            case DataType.U8: return As<byte>().ToString();
            case DataType.F64: return As<double>().ToString();
            case DataType.F32: return As<float>().ToString();
            case DataType.Pointer: return As<nuint>().ToString();
            case DataType.Void: return As<ulong>().ToString();
            default: throw new Exception();
        }
    }
} 

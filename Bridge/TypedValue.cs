using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct TypedValue(ulong Bits, DataType Type)
{
    public int Size => GetDataTypeSize(Type);

    public unsafe readonly Span<byte> GetBytes()
    {
        var ptr = (byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
        return new Span<byte>(ptr, Size);
    }
    
    public readonly T As<T>()
    {
        ulong bits = Bits;
        return Unsafe.As<ulong, T>(ref bits);
    }

    public readonly bool Is<T>()
    {
        return GetDataType<T>() is not DataType.Void;
    }

    public static TypedValue Create<T>(T value) where T : unmanaged
    {
        ulong bits = 0;
        var size = Unsafe.SizeOf<T>();
        Unsafe.CopyBlock(ref Unsafe.As<ulong, byte>(ref bits), ref Unsafe.As<T, byte>(ref value), (uint)size);
        return new(bits, GetDataType<T>());
    }
    
    public static TypedValue CreateDefault<T>() where T : unmanaged
    {
        return Create<T>(default);
    }

    public static TypedValue CreateDefault(DataType dataType)
    {
        switch (dataType)
        {
            case DataType.I64:      return CreateDefault<long>();
            case DataType.I32:      return CreateDefault<int>();
            case DataType.I16:      return CreateDefault<short>();
            case DataType.I8:       return CreateDefault<sbyte>();
            case DataType.U64:      return CreateDefault<ulong>();
            case DataType.U32:      return CreateDefault<uint>();
            case DataType.U16:      return CreateDefault<ushort>();
            case DataType.U8:       return CreateDefault<byte>();
            case DataType.F64:      return CreateDefault<double>();
            case DataType.F32:      return CreateDefault<float>();
            case DataType.Pointer:  return CreateDefault<nuint>();
            case DataType.Void:     return CreateDefault<ulong>();
            default: throw new Exception();
        }
    }

    public static DataType GetDataType<T>()
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
    
    public static int GetDataTypeSize<T>()
    {
        return GetDataTypeSize(GetDataType<T>());
    }

    public static int GetDataTypeSize(DataType type)
    {
        return type switch
        {
            DataType.I64 or DataType.U64 or DataType.F64 => 8,
            DataType.I32 or DataType.U32 or DataType.F32 => 4,
            DataType.I16 or DataType.U16 => 2,
            DataType.I8 or DataType.U8 => 1,
            DataType.Pointer => Unsafe.SizeOf<nuint>(),
            _ => throw new Exception(),
        };
    }

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
            _ => throw new Exception(),
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

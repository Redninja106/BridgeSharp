namespace Bridge;

/// <summary>
/// Specifies bridge data types.
/// </summary>
public enum DataType : byte
{
    /// <summary>
    /// A native-sized, unsigned integer
    /// </summary>
    Pointer,
    /// <summary>
    /// A 64 bit, signed integer.
    /// </summary>
    I64,
    /// <summary>
    /// A 32 bit, signed integer.
    /// </summary>
    I32,
    /// <summary>
    /// A 16 bit, signed integer.
    /// </summary>
    I16,
    /// <summary>
    /// A 8 bit, signed integer.
    /// </summary>
    I8,
    /// <summary>
    /// A 64 bit, unsigned integer.
    /// </summary>
    U64,
    /// <summary>
    /// A 32 bit, unsigned integer.
    /// </summary>
    U32,
    /// <summary>
    /// A 16 bit, unsigned integer.
    /// </summary>
    U16,
    /// <summary>
    /// A 8 bit, unsigned integer.
    /// </summary>
    U8,
    /// <summary>
    /// A 64 bit, floating point number.
    /// </summary>
    F64,
    /// <summary>
    /// A 32 bit, floating point number.
    /// </summary>
    F32,
    /// <summary>
    /// Signifies the absence of a type.
    /// </summary>
    Void = 255,
}

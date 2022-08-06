namespace Bridge;

/// <summary>
/// Identifies a calling convention.
/// </summary>
public enum CallingConvention
{
    /// <summary>
    /// The default bridge calling convention. Use this when calling bridge routines.
    /// </summary>
    Bridge,
    /// <summary>
    /// Maps to the C/C++ StdCall calling convention.
    /// </summary>
    StdCall,
    /// <summary>
    /// Maps to the C/C++ Cdecl calling convention.
    /// </summary>
    Cdecl,
}
namespace Bridge;

/// <summary>
/// Provides function sigature information for an indirect call.
/// </summary>
/// <param name="ReturnType"></param>
/// <param name="Parameters"></param>
/// <param name="CallingConvention"></param>
public record struct CallInfo(DataType ReturnType, DataType[] Parameters, CallingConvention CallingConvention);
namespace Bridge;

/// <summary>
/// Specifies different kind of stack operations -- used as the first argument in an <see cref="Instruction{T}"/>
/// or <see cref="Instruction{T1, T2}"/> for <see cref="OpCode.Push"/> or <see cref="OpCode.Pop"/> instructions.
/// </summary>
internal enum StackOpKind : byte
{
    Const,
    Local,
    Arg,
    LocalAddress,
    ArgAddress,
    Resource,
    Routine
}
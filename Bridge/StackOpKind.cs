namespace Bridge;

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
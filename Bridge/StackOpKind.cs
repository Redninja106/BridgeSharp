namespace Bridge;

internal enum StackOpKind : byte
{
    Const,
    Local,
    Arg,
    Fp,
    Sp,
    LocalAddress,
    ArgAddress,
    Resource,
}
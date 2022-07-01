namespace Bridge;

internal record Call(long Address, params long[] Parameters) : Instruction;
using System;

namespace Bridge;

public record Instruction(OpCode OpCode)
{
    public virtual int GetArgCount() => 0;

    public static Instruction Return() => new Instruction(OpCode.Return);
    public static Instruction CallDirect(DefinitionBuilder builder) => CallDirect(builder.ID);
    public static Instruction CallDirect(Definition definition) => CallDirect(definition.ID);
    public static Instruction CallDirect(int definitionID) => new Instruction<CallMode, int>(OpCode.Call, CallMode.Direct, definitionID);
    public static Instruction CallIndirect(CallInfo info) => new Instruction<CallMode, CallInfo>(OpCode.Call, CallMode.Indirect, info);
    public static Instruction Jump(Label label) => new Instruction<Label>(OpCode.Jump, label);
    public static Instruction If(ComparisonKind comparison, DataType type) => new Instruction<ComparisonKind, DataType>(OpCode.If, comparison, type);

    public static Instruction PushConst<T>(T value) where T : unmanaged => PushConst(TypedValue.Create(value));
    public static Instruction PushConst(TypedValue value) => new Instruction<StackOpKind, TypedValue>(OpCode.Push, StackOpKind.Const, value);
    public static Instruction PushLocal(Local local) => new Instruction<StackOpKind, Local>(OpCode.Push, StackOpKind.Local, local);
    public static Instruction PushArg(byte arg) => new Instruction<StackOpKind, byte>(OpCode.Push, StackOpKind.Arg, arg);
    public static Instruction Pop(DataType type) => new Instruction<StackOpKind, DataType>(OpCode.Pop, StackOpKind.Const, type);
    public static Instruction PopLocal(Local local) => new Instruction<StackOpKind, Local>(OpCode.Pop, StackOpKind.Local, local);
    public static Instruction PopArg(byte arg) => new Instruction<StackOpKind, byte>(OpCode.Pop, StackOpKind.Arg, arg);
    public static Instruction PushArgAddress(byte arg) => new Instruction<StackOpKind, byte>(OpCode.Push, StackOpKind.ArgAddress, arg);
    public static Instruction PushLocalAddress(Local local) => new Instruction<StackOpKind, Local>(OpCode.Push, StackOpKind.LocalAddress, local);
    public static Instruction PushResource(Index resourceIndex) => new Instruction<StackOpKind, Index>(OpCode.Push, StackOpKind.Resource, resourceIndex);
    public static Instruction PushRoutine(int routineID) => new Instruction<StackOpKind, int>(OpCode.Push, StackOpKind.Routine, routineID);

    public static Instruction Load(DataType type) => new Instruction<DataType>(OpCode.Load, type);
    public static Instruction Store(DataType type) => new Instruction<DataType>(OpCode.Load, type);

    public static Instruction Cast(DataType from, DataType to) => new Instruction<DataType, DataType>(OpCode.Cast, from, to);

    public static Instruction Add(DataType type) => new Instruction<DataType>(OpCode.Add, type);
    public static Instruction Subtract(DataType type) => new Instruction<DataType>(OpCode.Subtract, type);
    public static Instruction Multiply(DataType type) => new Instruction<DataType>(OpCode.Multiply, type);
    public static Instruction Divide(DataType type) => new Instruction<DataType>(OpCode.Divide, type);
    public static Instruction Modulo(DataType type) => new Instruction<DataType>(OpCode.Modulo, type);
    public static Instruction Negate(DataType type) => new Instruction<DataType>(OpCode.Negate, type);

    public static Instruction Increment(DataType type) => new Instruction<DataType>(OpCode.Increment, type);
    public static Instruction Decrement(DataType type) => new Instruction<DataType>(OpCode.Decrement, type);
    public static Instruction And(DataType type) => new Instruction<DataType>(OpCode.And, type);
    public static Instruction Or(DataType type) => new Instruction<DataType>(OpCode.Or, type);
    public static Instruction Xor(DataType type) => new Instruction<DataType>(OpCode.Xor, type);
    public static Instruction Not(DataType type) => new Instruction<DataType>(OpCode.Not, type);

    public static Instruction Print(DataType type) => new Instruction<DataType>(OpCode.Print, type);
    public static Instruction PrintChar(DataType type) => new Instruction<DataType>(OpCode.PrintChar, type);
    public static Instruction ReadChar(DataType type) => new Instruction<DataType>(OpCode.ReadChar, type);
    public static Instruction Compare(ComparisonKind kind, DataType type) => new Instruction<ComparisonKind, DataType>(OpCode.Compare, kind, type);
}

public record Instruction<T>(OpCode OpCode, T Arg1) : Instruction(OpCode)
{
    public override int GetArgCount() => 1;
}

public record Instruction<T1, T2>(OpCode OpCode, T1 Arg1, T2 Arg2) : Instruction<T1>(OpCode, Arg1)
{
    public override int GetArgCount() => 2;
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;

public record Instruction(OpCode OpCode)
{
    public virtual int GetArgCount() => 0;

    public static Instruction Return() => new Instruction(OpCode.Return);
    public static Instruction Call(DefinitionBuilder builder) => Call(builder.ID);
    public static Instruction Call(Definition definition) => Call(definition.ID);
    public static Instruction Call(int definitionID) => new Instruction<int>(OpCode.Call, definitionID);
    public static Instruction Jump(Label label) => new Instruction<Label>(OpCode.Jump, label);
    public static Instruction If(ComparisonKind comparison, DataType type) => new Instruction<ComparisonKind, DataType>(OpCode.If, comparison, type);
    
    public static Instruction Push(TypedValue value) => new Instruction<StackOpKind, TypedValue>(OpCode.Push, StackOpKind.Const, value);
    public static Instruction Push(Local local) => new Instruction<StackOpKind, Local>(OpCode.Push, StackOpKind.Local, local);
    public static Instruction PushArg(byte arg) => new Instruction<StackOpKind, byte>(OpCode.Push, StackOpKind.Arg, arg);
    public static Instruction Pop(DataType type) => new Instruction<StackOpKind, DataType>(OpCode.Pop, StackOpKind.Const, type);
    public static Instruction Pop(Local local) => new Instruction<StackOpKind, Local>(OpCode.Pop, StackOpKind.Local, local);
    public static Instruction PopArg(byte arg) => new Instruction<StackOpKind, byte>(OpCode.Pop, StackOpKind.Arg, arg);

    public static Instruction Loada(byte arg) => new Instruction<byte>(OpCode.Loada, arg);
    public static Instruction Loada(Local local) => new Instruction<Local>(OpCode.Loada, local);

    public static Instruction Load(DataType type) => new Instruction<DataType>(OpCode.Load, type);
    public static Instruction Store(DataType type) => new Instruction<DataType>(OpCode.Load, type);

    public static Instruction Cast(DataType from, DataType to) => new Instruction<DataType, DataType>(OpCode.Cast, from, to);


    public static Instruction Add(DataType type) => new Instruction<DataType>(OpCode.Add, type);
    public static Instruction Subtract(DataType type) => new Instruction<DataType>(OpCode.Subtract, type);
    public static Instruction Multiply(DataType type) => new Instruction<DataType>(OpCode.Multiply, type);
    public static Instruction Divide(DataType type) => new Instruction<DataType>(OpCode.Divide, type);
    public static Instruction Modulo(DataType type) => new Instruction<DataType>(OpCode.Modulo, type);
    public static Instruction Negate(DataType type) => new Instruction<DataType>(OpCode.Negate, type);

    public static Instruction Print(DataType type) => new Instruction<DataType>(OpCode.Print, type);
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
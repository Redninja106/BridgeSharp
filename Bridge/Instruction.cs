using System;

namespace Bridge;

/* 
 * The instruction record has only an OpCode.
 * The generic versions allow instructions to have arguments/modifiers
 * this approach is beneficial since we don't have to declare a new type for every new instruction
 * on the other hand, systems like the compiler or parser then need to know the generic arguments for each specific opcode.
 */

internal record Instruction<T>(OpCode OpCode, T Arg1) : Instruction(OpCode);
internal record Instruction<T1, T2>(OpCode OpCode, T1 Arg1, T2 Arg2) : Instruction<T1>(OpCode, Arg1);

/// <summary>
/// Represents a bridge instruction.
/// </summary>
/// <param name="OpCode"></param>
public record Instruction(OpCode OpCode)
{
    /// <summary>
    /// Returns from the current routine, push the a value of the current return type to the stack of the caller (or nothing, if the return type is void).
    /// </summary>
    /// <returns>A <c>return</c> instruction.</returns>
    public static Instruction Return() => new Instruction(OpCode.Return);


    /// <summary>
    /// Calls the provided routine, extern, or inline.
    /// </summary>
    /// <returns>A <c>call.direct</c> instruction to the provided definition.</returns>
    public static Instruction CallDirect(DefinitionBuilder builder) => CallDirect(builder.ID);

    /// <summary>
    /// Calls the provided routine, extern, or inline.
    /// </summary>
    /// <returns>A <c>call.direct</c> instruction to the provided definition.</returns>
    public static Instruction CallDirect(Definition definition) => CallDirect(definition.ID);

    /// <summary>
    /// Calls the provided routine, extern, or inline by it's ID.
    /// </summary>
    /// <returns>A <c>call.direct</c> instruction to the provided definition.</returns>
    public static Instruction CallDirect(int definitionID) => new Instruction<CallMode, int>(OpCode.Call, CallMode.Direct, definitionID);

    /// <summary>
    /// Performs an indirect call: Pops a function pointer from the stack, then calls it using the provided <see cref="CallInfo"/>.
    /// </summary>
    /// <returns>A <c>call.indirect</c> instruction to the provided definition.</returns>
    public static Instruction CallIndirect(CallInfo info) => new Instruction<CallMode, CallInfo>(OpCode.Call, CallMode.Indirect, info);

    /// <summary>
    /// Jumps to the provided label.
    /// </summary>
    /// <param name="label">The label to jump to. To reference a label later in the routine, emit the before it is provided to <see cref="Jump(Label)"/> and call <see cref="CodeBuilder.MoveLabel(Label)"/> to move the label to it's intend location.</param>
    /// <returns>A <c>jump</c> instruction.</returns>
    public static Instruction Jump(Label label) => new Instruction<Label>(OpCode.Jump, label);

    /// <summary>
    /// Executes the next instruction only if the provided comparison is true.
    /// </summary>
    /// <param name="comparison">The kind of comparison to perform.</param>
    /// <param name="type">The type of value to perform the comparison on.</param>
    /// <returns>An <c>if</c> instruction.</returns>
    public static Instruction If(ComparisonKind comparison, DataType type) => new Instruction<ComparisonKind, DataType>(OpCode.If, comparison, type);


    /// <summary>
    /// Pushes a constant value of the provided primitive type.
    /// </summary>
    /// <typeparam name="T">The primitive type to push to the stack.
    /// <para>
    /// Valid primitives are:
    /// <see langword="long"/>,
    /// <see langword="int"/>,
    /// <see langword="short"/>,
    /// <see langword="sbyte"/>,
    /// <see langword="ulong"/>,
    /// <see langword="uint"/>,
    /// <see langword="ushort"/>,
    /// <see langword="byte"/>,
    /// <see langword="double"/>,
    /// <see langword="float"/>,
    /// and <see langword="nuint"/>.
    /// </para>
    /// </typeparam>
    /// <param name="value">The value to push to the stack.</param>
    /// <returns>A <c>push.const</c> instruction.</returns>
    public static Instruction Push<T>(T value) where T : unmanaged => Push(TypedValue.Create(value));

    /// <summary>
    /// Pushes a constant value to the stack.
    /// </summary>
    /// <param name="value">The value to push to the stack.</param>
    /// <returns>A <c>push.const</c> instruction.</returns>
    public static Instruction Push(TypedValue value) => new Instruction<StackOpKind, TypedValue>(OpCode.Push, StackOpKind.Const, value);
    
    /// <summary>
    /// Pushes the value of a local to the stack.
    /// </summary>
    /// <param name="local">The local whose value to push to the stack.</param>
    /// <returns>A <c>push.local</c> instruction.</returns>
    public static Instruction Push(Local local) => new Instruction<StackOpKind, Local>(OpCode.Push, StackOpKind.Local, local);

    /// <summary>
    /// Pushes the value of an argument to the stack.
    /// </summary>
    /// <param name="argument">The argument whose value to push to the stack.</param>
    /// <returns>A <c>push.arg</c> instruction.</returns>
    public static Instruction Push(Argument argument) => new Instruction<StackOpKind, Argument>(OpCode.Push, StackOpKind.Arg, argument);

    /// <summary>
    /// Pushes the address of an argument to the stack. 
    /// </summary>
    /// <param name="argument">The argument of which to push the address of.</param>
    /// <returns>A <c>push.arg&amp;</c></returns>
    public static Instruction PushAddress(Argument argument) => new Instruction<StackOpKind, Argument>(OpCode.Push, StackOpKind.ArgAddress, argument);

    /// <summary>
    /// Pushes the address of a local to the stack. 
    /// </summary>
    /// <param name="argument">The local of which to push the address of.</param>
    /// <returns>A <c>push.local&amp;</c> instruction.</returns>
    public static Instruction PushAddress(Local local) => new Instruction<StackOpKind, Local>(OpCode.Push, StackOpKind.LocalAddress, local);

    /// <summary>
    /// Pushes the address of a resource to the stack
    /// </summary>
    /// <param name="resourceIndex">The index of the resource to push, typically returned from <see cref="ModuleBuilder.AddResource(string)"/>.</param>
    /// <returns>A <c>push.resource</c> instruction.</returns>
    public static Instruction PushResource(Index resourceIndex) => new Instruction<StackOpKind, Index>(OpCode.Push, StackOpKind.Resource, resourceIndex);

    /// <summary>
    /// Pushes the size of a resource, in bytes, to the stack as native-sized int (pointer).
    /// </summary>
    /// <param name="resourceIndex">The index of the resource to push, typically returned from <see cref="ModuleBuilder.AddResource(string)"/>.</param>
    /// <returns>A <c>push.resource</c> instruction.</returns>
    public static Instruction PushResourceSize(Index resourceIndex) => new Instruction<StackOpKind, Index>(OpCode.Push, StackOpKind.ResourceSize, resourceIndex);

    /// <summary>
    /// Pushes the address of a routine or extern to the stack.
    /// <para>
    /// The pushed address can then be called using <see cref="CallIndirect(CallInfo)"/>.
    /// </para>
    /// </summary>
    /// <param name="builder">The definition whose address to push to the stack. This may only a <see cref="RoutineBuilder"/> or <see cref="ExternBuilder"/>.</param>
    /// <returns>A <c>push.routine</c> instruction.</returns>
    public static Instruction PushRoutine(DefinitionBuilder builder) => PushRoutine(builder.ID);

    /// <summary>
    /// Pushes the address of a routine or extern to the stack.
    /// <para>
    /// The pushed address can then be called using <see cref="CallIndirect(CallInfo)"/>.
    /// </para>
    /// </summary>
    /// <param name="definition">The definition whose address to push to the stack. This may only a <see cref="RoutineDefinition"/> or <see cref="ExternDefinition"/>.</param>
    /// <returns>A <c>push.routine</c> instruction.</returns>
    public static Instruction PushRoutine(Definition definition) => PushRoutine(definition.ID);

    /// <summary>
    /// Pushes the address of a routine to the stack.
    /// <para>
    /// The pushed address can then be called using <see cref="CallIndirect(CallInfo)"/>.
    /// </para>
    /// </summary>
    /// <param name="routineID">The routine to be pushed to the stack.</param>
    /// <returns>A <c>push.routine</c> instruction.</returns>
    public static Instruction PushRoutine(int routineID) => new Instruction<StackOpKind, int>(OpCode.Push, StackOpKind.Routine, routineID);

    
    /// <summary>
    /// Pops a value of the provided type from the stack.
    /// </summary>
    /// <param name="type">The type of value to pop from the stack.</param>
    /// <returns>A <c>pop</c> instruction.</returns>
    public static Instruction Pop(DataType type) => new Instruction<StackOpKind, DataType>(OpCode.Pop, StackOpKind.Const, type);

    /// <summary>
    /// Pops a value from the stack and stores it in the provided local.
    /// </summary>
    /// <param name="local">The local into which to pop from the stack.</param>
    /// <returns>A <c>pop.local</c> instruction.</returns>
    public static Instruction Pop(Local local) => new Instruction<StackOpKind, Local>(OpCode.Pop, StackOpKind.Local, local);

    /// <summary>
    /// Pops a value from the stack and stores it in the provided argument.
    /// </summary>
    /// <param name="argument">The argument into which to pop from the stack.</param>
    /// <returns>A <c>pop.arg</c> instruction.</returns>
    public static Instruction Pop(Argument argument) => new Instruction<StackOpKind, Argument>(OpCode.Pop, StackOpKind.Arg, argument);


    /// <summary>
    /// Pops a address from the stack and reads a value of the provided type from that address, and pushes the value read.
    /// </summary>
    /// <param name="type">The type of value to read.</param>
    /// <returns>A <c>load</c> instruction.</returns>
    public static Instruction Load(DataType type) => new Instruction<DataType>(OpCode.Load, type);

    /// <summary>
    /// Pops an address from the stack, then a value of the provided type, then writes that value to the address.
    /// </summary>
    /// <param name="type">The type of value to write.</param>
    /// <returns>A <c>store</c> instruction.</returns>
    public static Instruction Store(DataType type) => new Instruction<DataType>(OpCode.Store, type);

    /// <summary>
    /// Pops a value of one type, converts to another type, and pushes the result.
    /// </summary>
    /// <param name="from">The type to convert from.</param>
    /// <param name="to">The type to convert to.</param>
    /// <returns>A <c>cast</c> instruction.</returns>
    public static Instruction Cast(DataType from, DataType to) => new Instruction<DataType, DataType>(OpCode.Cast, from, to);

    /// <summary>
    /// Pops a value of the provided type, performs the comparison of the provided kind, and pushes the i8 value 0 in the comparison failed, or 1 if it succeeded.
    /// </summary>
    /// <param name="kind">The kind of comparison to perform.</param>
    /// <param name="type">The type of values to perform the comparison on.</param>
    /// <returns>A <c>compare</c> instruction.</returns>
    public static Instruction Compare(ComparisonKind kind, DataType type) => new Instruction<ComparisonKind, DataType>(OpCode.Compare, kind, type);

    /// <summary>
    /// Pops two values and adds them, then pushes the result.
    /// </summary>
    /// <param name="type">The type of value to perform the addition on.</param>
    /// <returns>An <c>add</c> instruction.</returns>
    public static Instruction Add(DataType type) => new Instruction<DataType>(OpCode.Add, type);

    /// <summary>
    /// Pops two values and subtracts them, then pushes the result.
    /// </summary>
    /// <param name="type">The type of value to perform the subtraction on.</param>
    /// <returns>A <c>subtract</c> instruction.</returns>
    public static Instruction Subtract(DataType type) => new Instruction<DataType>(OpCode.Subtract, type);

    /// <summary>
    /// Pops two values and multiplies them, then pushes the result.
    /// </summary>
    /// <param name="type">The type of value to perform the multiplication on.</param>
    /// <returns>A <c>multiply</c> instruction.</returns>
    public static Instruction Multiply(DataType type) => new Instruction<DataType>(OpCode.Multiply, type);

    /// <summary>
    /// Pops two values and divides them, then pushes the result.
    /// </summary>
    /// <param name="type">The type of value to perform the division on.</param>
    /// <returns>A <c>divide</c> instruction.</returns>
    public static Instruction Divide(DataType type) => new Instruction<DataType>(OpCode.Divide, type);

    /// <summary>
    /// Pops two values and pushes the remainder when the first is divied by the second.
    /// </summary>
    /// <param name="type">The type of value to perform the modulo on.</param>
    /// <returns>A <c>modulo</c> instruction.</returns>
    public static Instruction Modulo(DataType type) => new Instruction<DataType>(OpCode.Modulo, type);

    /// <summary>
    /// Pops a value of the stack, negates it, then pushes the result.
    /// </summary>
    /// <param name="type">The type of value to negate.</param>
    /// <returns>A <c>negate</c> instruction.</returns>
    public static Instruction Negate(DataType type) => new Instruction<DataType>(OpCode.Negate, type);


    /// <summary>
    /// Pops a value from the stack, adds one to it, then pushes the result.
    /// </summary>
    /// <param name="type">The type of value to increment.</param>
    /// <returns>An <c>increment</c> instruction.</returns>
    public static Instruction Increment(DataType type) => new Instruction<DataType>(OpCode.Increment, type);

    /// <summary>
    /// Pops a value from the stack, subtracts one from it, then pushes the result.
    /// </summary>
    /// <param name="type">The type of value to decrement.</param>
    /// <returns>A <c>decrement</c> instruction.</returns>
    public static Instruction Decrement(DataType type) => new Instruction<DataType>(OpCode.Decrement, type);


    /// <summary>
    /// Pops two values from the stack, performs a bitwise AND, then pushes the result.
    /// </summary>
    /// <param name="type">The type of value with which to perform the bitwise AND. This must not be a floating point type (i.e. it cannot have an "f" prefix).</param>
    /// <returns>An <c>and</c> instruction.</returns>
    public static Instruction And(DataType type) => new Instruction<DataType>(OpCode.And, type);

    /// <summary>
    /// Pops two values from the stack, performs a bitwise OR, then pushes the result.
    /// </summary>
    /// <param name="type">The type of value with which to perform the bitwise OR. This must not be a floating point type (i.e. it cannot have an "f" prefix).</param>
    /// <returns>An <c>or</c> instruction.</returns>
    public static Instruction Or(DataType type) => new Instruction<DataType>(OpCode.Or, type);

    /// <summary>
    /// Pops two values from the stack, performs a bitwise XOR, then pushes the result.
    /// </summary>
    /// <param name="type">The type of value with which to perform the bitwise XOR. This must not be a floating point type (i.e. it cannot have an "f" prefix).</param>
    /// <returns>An <c>xor</c> instruction.</returns>
    public static Instruction Xor(DataType type) => new Instruction<DataType>(OpCode.Xor, type);

    /// <summary>
    /// Pops two values from the stack, performs a bitwise NOT (ones complement), then pushes the result.
    /// </summary>
    /// <param name="type">The type of value with which to perform the bitwise NOT. This must not be a floating point type (i.e. it cannot have an "f" prefix).</param>
    /// <returns>An <c>not</c> instruction.</returns>
    public static Instruction Not(DataType type) => new Instruction<DataType>(OpCode.Not, type);

    // TODO: remove these functions now the externs are supported better

    public static Instruction Print(DataType type) => new Instruction<DataType>(OpCode.Print, type);
    public static Instruction PrintChar(DataType type) => new Instruction<DataType>(OpCode.PrintChar, type);
    public static Instruction ReadChar(DataType type) => new Instruction<DataType>(OpCode.ReadChar, type);
}
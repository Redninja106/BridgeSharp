using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Bridge;
public unsafe class Interpreter : IDisposable
{
    const int STACK_SIZE = 1024 * 1024; // 1MB

    NativeLibraryHandler libraryHandler = new();
    Module module;
    Dictionary<Index, nuint> resources = new();
    byte* stack;
    nuint sp, fp;
    int ip;

    Span<byte> StackSpan => new(stack, STACK_SIZE);

    Stack<RoutineDefinition> callStack = new();
    RoutineDefinition currentRoutine => callStack.Peek();

    public Interpreter()
    {
        stack = (byte*)NativeMemory.AllocZeroed(STACK_SIZE);
        sp = fp = (nuint)stack;
        ip = -1;
    }

    public void Run(Module module)
    {
        //if (!module.IsExecutable)
        //    throw new ArgumentException("Module must be executable!");

        this.module = module;

        Call(module.Routines.Single(x => x.Name is "main").ID);

        while (callStack.Any())
        {
            //Console.WriteLine(new string('=', 100));
            //Console.WriteLine($"IP: {ip}, SP: {sp}, FP: {fp}");
            //DumpStack();
            ip++;
            EvalInstruction(currentRoutine.Instructions[ip]);
        }
    }

    private void EvalInstruction(Instruction instruction)
    {
        if (instruction.OpCode is OpCode.Nop)
            return;

        switch (instruction)
        {
            case Instruction<Label> labelInstruction when instruction.OpCode is OpCode.Jump:
                if (instruction.OpCode is not OpCode.Jump)
                    throw new Exception();
                ip = currentRoutine.LabelLocations[labelInstruction.Arg1.Value] - 1;
                break;
            case Instruction<int> ptrInstruction:
                EvalPtrInstruction(ptrInstruction);
                break;
            case Instruction<CallMode> callInstruction:
                EvalCallInstruction(callInstruction);
                break;
            case Instruction<DataType> typedInstruction:
                EvalTypedInstruction(typedInstruction);
                break;
            case Instruction<StackOpKind> stackInstruction:
                EvalStackInstruction(stackInstruction);
                break;
            case Instruction<ComparisonKind, DataType> compInstruction:
                EvalCompInstruction(compInstruction);
                break;
            case Instruction:
                EvalBaseInstruction(instruction);
                break;
            default:
                break;
        }
    }

    private void EvalCallInstruction(Instruction<CallMode> instruction)
    {
        switch (instruction.Arg1)
        {
            case CallMode.Direct when instruction is Instruction<CallMode, int> directCallInstruction:
                Call(directCallInstruction.Arg2);
                break;
            case CallMode.Indirect:
            case CallMode.Tail:
            case CallMode.TailIndirect:
            default:
                throw new NotImplementedException();
        }
    }

    private void EvalCompInstruction(Instruction<ComparisonKind, DataType> instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.If:
                if (!Compare(instruction.Arg1, instruction.Arg2))
                {
                    ip++;
                }
                break;
            case OpCode.Compare:
                Push(Compare(instruction.Arg1, instruction.Arg2) ? (byte)1 : (byte)0);
                break;
            default:
                throw new Exception();
        }
    }

    private void EvalPtrInstruction(Instruction<int> instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.Jump:
                ip = instruction.Arg1;
                break;
            default:
                break;
        }
    }

    private void EvalBaseInstruction(Instruction instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.Return:
                Return();
                break;
            default:
                break;
        }
    }

    private void EvalStackInstruction(Instruction<StackOpKind> instruction)
    {
        switch (instruction)
        {
            case Instruction<StackOpKind, Argument> idInstruction:
                EvalArgInstruction(idInstruction);
                break;
            case Instruction<StackOpKind, Local> localInstruction:
                EvalLocalInstruction(localInstruction);
                break;
            case Instruction<StackOpKind, TypedValue> constInstruction when instruction.OpCode is OpCode.Push:
                if (constInstruction.Arg1 is not StackOpKind.Const)
                    throw new Exception();
                Push(constInstruction.Arg2);
                break;
            case Instruction<StackOpKind, DataType> typedInstruction when instruction.OpCode is OpCode.Pop:
                Pop(typedInstruction.Arg2);
                break;
            case Instruction<StackOpKind, Index> resourceInstruction:
                Push(LoadResource(resourceInstruction.Arg2));
                break;
            default:
                throw new Exception();
        }
    }

    private nuint LoadResource(Index index)
    {
        if (resources.TryGetValue(index, out var loadedResource))
        {
            return loadedResource;
        }

        var bytes = module.ResourceTable.GetResourceBytes(index);
        
        var resource = NativeMemory.Alloc((nuint)bytes.Length);
        
        var resourceSpan = new Span<byte>(resource, bytes.Length);
        bytes.CopyTo(resourceSpan);

        this.resources.Add(index, (nuint)resource);
        return (nuint)resource;
    }

    //private nuint IndexToPointer(int index)
    //{
    //    return (nuint)stack + (nuint)index;
    //}

    private void EvalTypedInstruction(Instruction<DataType> instruction)
    {
        TypedValue left, right;
        
        switch (instruction.OpCode)
        {
            case OpCode.Load:
                var address = Pop<nuint>();
                var val = Read(instruction.Arg1, address);
                Push(val);
                break;
            case OpCode.Store:
                Write(Pop(instruction.Arg1), Pop<nuint>());
                break;
            case OpCode.Print:
                Console.WriteLine(Pop(instruction.Arg1));
                break;
            case OpCode.Add:
                right = Pop(instruction.Arg1);
                left = Pop(instruction.Arg1);

                Push(BinaryOp(left, right, (a, b) =>
                {
                    if (a is nuint c && b is nuint d)
                        return c + d;

                    return a + b;
                }));
                break;
            case OpCode.Subtract:
                right = Pop(instruction.Arg1);
                left = Pop(instruction.Arg1);
                Push(BinaryOp(left, right, (a, b) => a - b));
                break;
            case OpCode.Multiply:
                right = Pop(instruction.Arg1);
                left = Pop(instruction.Arg1);
                Push(BinaryOp(left, right, (a, b) => a * b));
                break;
            case OpCode.Divide:
                right = Pop(instruction.Arg1);
                left = Pop(instruction.Arg1);
                Push(BinaryOp(left, right, (a, b) => a / b));
                break;
            case OpCode.Modulo:
                right = Pop(instruction.Arg1);
                left = Pop(instruction.Arg1);
                Push(BinaryOp(left, right, (a, b) => a % b));
                break;
            case OpCode.Negate:
                Push(UnaryOp(Pop(instruction.Arg1), x => -x));
                break;
            case OpCode.Cast:
                if (instruction is not Instruction<DataType, DataType> castInst)
                    throw new Exception();
                Push(Cast(Pop(castInst.Arg1), castInst.Arg2));
                break;
            case OpCode.PrintChar:
                var type = instruction.Arg1;

                if (type is DataType.F32 or DataType.F64)
                    throw new Exception();
                
                TypedValue value = Pop(type);
                Console.Write((char)ToDynamic(value));
                break;
            case OpCode.ReadChar:
                type = instruction.Arg1;

                if (type is DataType.F32 or DataType.F64)
                    throw new Exception();

                Push(Cast(TypedValue.Create(Console.Read()), type));
                break;
            case OpCode.Increment:
                var operand = Pop(instruction.Arg1);
                Push(UnaryOp(operand, x => x + 1));
                break;
            case OpCode.Decrement:
                operand = Pop(instruction.Arg1);
                Push(UnaryOp(operand, x => x - 1));
                break;
            case OpCode.And:
                right = Pop(instruction.Arg1);
                left = Pop(instruction.Arg1);
                Push(BinaryOp(left, right, (a, b) => a & b));
                break;
            case OpCode.Or:
                right = Pop(instruction.Arg1);
                left = Pop(instruction.Arg1);
                Push(BinaryOp(left, right, (a, b) => a | b));
                break;
            case OpCode.Xor:
                right = Pop(instruction.Arg1);
                left = Pop(instruction.Arg1);
                Push(BinaryOp(left, right, (a, b) => a ^ b));
                break;
            case OpCode.Not:
                Push(UnaryOp(Pop(instruction.Arg1), x => ~x));
                break;
            default:
                throw new Exception();
        }
    }

    private void EvalArgInstruction(Instruction<StackOpKind, Argument> instruction)
    {
        DataType type = currentRoutine.Parameters[instruction.Arg2.Value];
        nuint location = GetArgumentLocation(instruction.Arg2.Value);

        switch (instruction.OpCode)
        {
            case OpCode.Push: Push(Read(type, location)); break;
            case OpCode.Pop: Write(Pop(type), location); break;
            default: throw new Exception();
        }
    }

    private void EvalLocalInstruction(Instruction<StackOpKind, Local> instruction)
    {
        DataType type = currentRoutine.Locals[instruction.Arg2.Value];
        nuint location = GetLocalLocation(instruction.Arg2);

        switch (instruction.OpCode)
        {
            case OpCode.Push: Push(Read(type, location)); break;
            case OpCode.Pop: Write(Pop(type), location); break;
            default: throw new Exception();
        }
    }

    private bool Compare(ComparisonKind kind, DataType type)
    {
        if (kind is ComparisonKind.Zero)
            return UnaryCompOp(Pop(type), x => x == 0);
        if (kind is ComparisonKind.NotZero)
            return UnaryCompOp(Pop(type), x => x != 0);

        var right = Pop(type);
        var left = Pop(type);

        switch (kind)
        {
            case ComparisonKind.LessThan:           return BinaryCompOp(left, right, (a, b) => a < b);
            case ComparisonKind.LessThanEqual:      return BinaryCompOp(left, right, (a, b) => a <= b);
            case ComparisonKind.GreaterThan:        return BinaryCompOp(left, right, (a, b) => a > b);
            case ComparisonKind.GreaterThanEqual:   return BinaryCompOp(left, right, (a, b) => a >= b);
            case ComparisonKind.Equal:              return BinaryCompOp(left, right, (a, b) => a == b);
            case ComparisonKind.NotEqual:           return BinaryCompOp(left, right, (a, b) => a != b);
            default:                                throw new Exception();
        }
    }

    private void Call(int routineID)
    {
        var callee = module.Definitions.Single(x => x.ID == routineID);

        switch (callee)
        {
            case ExternDefinition ex:
                libraryHandler.Call(ex);
                
                break;
            case RoutineDefinition routine:
                callStack.Push(routine);

                Push(TypedValue.Create(fp));
                Push(TypedValue.Create(ip));
                fp = sp;
                ip = -1;

                foreach (var local in currentRoutine.Locals)
                {
                    Push(TypedValue.CreateDefault(local));
                }
                break;
            default:
                break;
        }
    }

    private void Return()
    {
        var returnType = currentRoutine.ReturnType;
        TypedValue? retval = null;

        if (currentRoutine.ReturnType is not DataType.Void)
            retval = Pop(returnType);

        sp = fp;
        ip = Pop(DataType.I32).As<int>();
        fp = Pop(DataType.Pointer).As<nuint>();
        
        foreach (var param in currentRoutine.Parameters)
        {
            Pop(param);
        }
        
        callStack.Pop();

        if (retval is not null)
            Push(retval.Value);
    }
    
    private void Push(TypedValue value)
    {
        Write(value, sp);
        sp += (nuint)value.Size;
    }

    private void Push<T>(T value) where T : unmanaged
    {
        Push(TypedValue.Create(value));
    }

    private TypedValue Read(DataType type, nuint ptr)
    {
        // copy bits from stack
        var size = TypedValue.GetDataTypeSize(type);
        Span<byte> bytes = stackalloc byte[size];
        var targetBytes = new Span<byte>((void*)ptr, bytes.Length);
        targetBytes.CopyTo(bytes);

        // copy bytes into a new TypedValue
        TypedValue result = TypedValue.CreateDefault(type);
        bytes.CopyTo(result.GetBytes());
        return result;
    }

    private void Write(TypedValue value, nuint ptr)
    {
        var bytes = value.GetBytes();

        if (sp + (nuint)bytes.Length >= sp + STACK_SIZE)
            throw new Exception("Stack overflow!");

        var targetBytes = new Span<byte>((void*)ptr, bytes.Length);
        bytes.CopyTo(targetBytes);
    }

    private TypedValue Pop(DataType type)
    {
        sp -= (nuint)TypedValue.GetDataTypeSize(type);
        return Read(type, sp);
    }
    
    private T Pop<T>() where T : unmanaged
    {
        return Pop(TypedValue.GetDataType<T>()).As<T>();
    }
    
    private nuint GetArgumentLocation(byte id)
    {
        nuint location = fp - 12;
        for (int i = currentRoutine.Parameters.Length - 1; i >= id; i--)
        {
            location -= (nuint)TypedValue.GetDataTypeSize(currentRoutine.Parameters[i]);
        }
        return location;
    }
    
    private nuint GetLocalLocation(Local local)
    {
        nuint location = fp;
        for (int i = 0; i < local.Value; i++)
        {
            location += (nuint)TypedValue.GetDataTypeSize(currentRoutine.Locals[i]);
        }
        return location;
    }

    private static TypedValue BinaryOp(TypedValue a, TypedValue b, Func<dynamic, dynamic, dynamic> op)
    {
        if (a.Type != b.Type)
            throw new Exception("must be the same type");

        var dynamicA = ToDynamic(a);
        var dynamicB = ToDynamic(b);
        var result = op(dynamicA, dynamicB);
        return FromDynamic(result);
    }

    private static bool BinaryCompOp(TypedValue a, TypedValue b, Func<dynamic, dynamic, dynamic> op)
    {
        if (a.Type != b.Type)
            throw new Exception("must be the same type");

        var dynamicA = ToDynamic(a);
        var dynamicB = ToDynamic(b);
        var result = op(dynamicA, dynamicB);
        return (bool)result;
    }

    private static bool UnaryCompOp(TypedValue operand, Func<dynamic, dynamic> op)
    {
        var dynamicOperand = ToDynamic(operand);
        var result = op(dynamicOperand);
        return (bool)result;
    }

    private static TypedValue UnaryOp(TypedValue operand, Func<dynamic, dynamic> op)
    {
        var dynamicOperand = ToDynamic(operand);
        var result = op(dynamicOperand);
        return FromDynamic(result);
    }


    private static dynamic ToDynamic(TypedValue value)
    {
        switch (value.Type)
        {
            case DataType.I64:      return value.As<long>();
            case DataType.I32:      return value.As<int>();
            case DataType.I16:      return value.As<short>();
            case DataType.I8:       return value.As<sbyte>();
            case DataType.U64:      return value.As<ulong>();
            case DataType.U32:      return value.As<uint>();
            case DataType.U16:      return value.As<ushort>();
            case DataType.U8:       return value.As<byte>();
            case DataType.F64:      return value.As<double>();
            case DataType.F32:      return value.As<float>();
            case DataType.Pointer:  return value.As<nuint>();
            default:
                throw new Exception("unsupported type");
        };
    }

    private static TypedValue FromDynamic(dynamic dynamic)
    {
        if (dynamic is long)    return TypedValue.Create<long>(dynamic);
        if (dynamic is int)     return TypedValue.Create<int>(dynamic);
        if (dynamic is short)   return TypedValue.Create<short>(dynamic);
        if (dynamic is sbyte)   return TypedValue.Create<sbyte>(dynamic);
        if (dynamic is ulong)   return TypedValue.Create<ulong>(dynamic);
        if (dynamic is uint)    return TypedValue.Create<uint>(dynamic);
        if (dynamic is ushort)  return TypedValue.Create<ushort>(dynamic);
        if (dynamic is byte)    return TypedValue.Create<byte>(dynamic);
        if (dynamic is double)  return TypedValue.Create<double>(dynamic);
        if (dynamic is float)   return TypedValue.Create<float>(dynamic);
        if (dynamic is nuint)   return TypedValue.Create<nuint>(dynamic);
        throw new Exception();
    }

    private static TypedValue Cast(TypedValue value, DataType target)
    {
        dynamic dynamic = ToDynamic(value);

        dynamic result = null;
        if (target is DataType.I64)     result = (long)dynamic;
        if (target is DataType.I32)     result = (int)dynamic;
        if (target is DataType.I16)     result = (short)dynamic;
        if (target is DataType.I8)      result = (sbyte)dynamic;
        if (target is DataType.U64)     result = (ulong)dynamic;
        if (target is DataType.U32)     result = (uint)dynamic;
        if (target is DataType.U16)     result = (ushort)dynamic;
        if (target is DataType.U8)      result = (byte)dynamic;
        if (target is DataType.F64)     result = (double)dynamic;
        if (target is DataType.F32)     result = (float)dynamic;
        if (target is DataType.Pointer) result = (nuint)dynamic;

        return FromDynamic(result);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        NativeMemory.Free(this.stack);

        libraryHandler.Dispose();

        foreach (var (index, resource) in resources)
        {
            NativeMemory.Free((void*)resource);
        }
    }

    class NativeLibraryHandler : IDisposable
    {
        Dictionary<string, (IntPtr, int)> libraries = new();
        
        public void Call(ExternDefinition definition)
        {
            var assembly = typeof(Action).Assembly;
            int genericArgCount = definition.Parameters.Length;
            bool isFunc = definition.ReturnType is DataType.Void;

            string delegateName = "System.";
            
            if (isFunc)
            {
                delegateName += "Action";
            }
            else
            {
                genericArgCount += 1;
                delegateName += "Func";
            }
            
            if (genericArgCount > 0)
            {
                delegateName += "`" + genericArgCount;
            }

            var genericArgs = new Type[genericArgCount];

            for (int i = 0; i < genericArgCount; i++)
            {
                genericArgs[i] = TypedValue.GetDataTypePrimitive(definition.Parameters[i]);
            }
            
            if (isFunc)
            {
                genericArgs[genericArgCount - 1] = TypedValue.GetDataTypePrimitive(definition.ReturnType);
            }

            var delegateType = assembly.GetType(delegateName);
            var genericType = delegateType!.MakeGenericType(genericArgs);
        }

        public void Dispose()
        {
            //foreach (var lib in libraries.Values)
            //{
            //    NativeLibrary.Free(lib.Item1);
            //}
        }

        private TypedValue Call(IntPtr fn, params TypedValue[] args)
        {
            return default;
        }
    }

    ~Interpreter()
    {
        Dispose();
    }
}
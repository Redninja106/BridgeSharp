using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CIL = System.Reflection.Emit;
using CILModuleBuilder = System.Reflection.Emit.ModuleBuilder;
using CILOpCode = System.Reflection.Emit.OpCode;

namespace Bridge;

/// <summary>
/// Experimental bridge to .NET cil compiler
/// </summary>
internal class CILCompiler
{
    private readonly Module module;
    private readonly List<(RoutineDefinition Routine, MethodBuilder Method)> methodBuilders = new();
    private FieldInfo dumpField;
    Stack<CIL.Label> ifs = new();
    
    public CILCompiler(Module module)
    {
        this.module = module;
    }

    public MethodInfo Compile(AssemblyBuilder builder)
    {
        var moduleBuilder = builder.DefineDynamicModule("module");
        var typeBuilder = moduleBuilder.DefineType("Program");

        dumpField = typeBuilder.DefineField("dump", typeof(long), FieldAttributes.Static);

        foreach (var definition in module.Definitions)
        {
            CompileDefinition(definition, typeBuilder);
        }

        foreach (var definition in module.Definitions)
        {
            CompileDefinitionBody(definition, typeBuilder);
        }

        var compiledType = typeBuilder.CreateType();
        return compiledType.GetMethod("main");
    }

    private void CompileDefinitionBody(Definition definition, TypeBuilder typeBuilder)
    {
        switch (definition)
        {
            case RoutineDefinition routineDefinition:
                CompileRoutineBody(routineDefinition, typeBuilder);
                break;
            default:
                break;
        }
    }

    private void CompileDefinition(Definition definition, TypeBuilder typeBuilder)
    {
        switch (definition)
        {
            case RoutineDefinition routineDefinition:
                CompileRoutine(routineDefinition, typeBuilder);
                break;
            default:
                break;
        }
    }

    private void CompileRoutine(RoutineDefinition routine, TypeBuilder typeBuilder)
    {
        var returnType = routine.ReturnType is DataType.Void ? typeof(void) : TypedValue.GetDataTypePrimitive(routine.ReturnType);
        var parameters = routine.Parameters.Select(p=>TypedValue.GetDataTypePrimitive(p)).ToArray();
        var method = typeBuilder.DefineMethod(routine.Name, MethodAttributes.Static | MethodAttributes.Public, returnType, parameters);
        
        methodBuilders.Add((routine, method));
    }

    private void CompileRoutineBody(RoutineDefinition routine, TypeBuilder typeBuilder)
    {
        var (_, method) = methodBuilders.First(m => m.Routine == routine);

        var il = method.GetILGenerator();

        var labels = new CIL.Label[routine.LabelLocations.Length];

        for (int i = 0; i < labels.Length; i++)
        {
            labels[i] = il.DefineLabel();
        }

        foreach (var local in routine.Locals)
        {
            il.DeclareLocal(TypedValue.GetDataTypePrimitive(local));
        }

        for (int i = 0; i < routine.Instructions.Length; i++)
        {
            for (int j = 0; j < routine.LabelLocations.Length; j++)
            {
                var location = routine.LabelLocations[j];
                if (i==location)
                {
                    il.MarkLabel(labels[j]);
                }
            }

            var instruction = routine.Instructions[i];
            CompileInstruction(il, routine, labels, instruction);
        }

    }

    private void CompileInstruction(ILGenerator il, RoutineDefinition routine, CIL.Label[] labels, Instruction instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.Nop:
                il.Emit(OpCodes.Nop);
                break;
            case OpCode.Push when instruction is Instruction<StackOpKind> stackInstruction:
                EmitPush(il, stackInstruction);
                break;
            case OpCode.Pop when instruction is Instruction<StackOpKind> stackInstruction:
                EmitPop(il, stackInstruction);
                break;
            case OpCode.Load when instruction is Instruction<DataType> loadInstruction:
                EmitLoad(il, loadInstruction);
                break;
            case OpCode.Store when instruction is Instruction<DataType> storeInstruction:
                EmitStore(il, storeInstruction);
                break;
            case OpCode.Call when instruction is Instruction<int> callInstruction:
                il.EmitCall(OpCodes.Call, methodBuilders.Single(pair => pair.Routine.ID == callInstruction.Arg1).Method, null);
                break;
            case OpCode.Return:
                il.Emit(OpCodes.Ret);
                break;
            case OpCode.Jump:
                if (instruction is not Instruction<Label> labelInst)
                    throw new Exception();
                il.Emit(OpCodes.Br, labels[labelInst.Arg1.Value]);
                break;
            case OpCode.If when instruction is Instruction<ComparisonKind, DataType> ifInstruction:
                ifs.Push(BeginIf(il, ifInstruction));
                break;
            case OpCode.Cast when instruction is Instruction<DataType, DataType> castInstruction:
                EmitCast(il, castInstruction);
                break;
            case OpCode.Add:
                il.Emit(OpCodes.Add);
                break;
            case OpCode.Subtract:
                il.Emit(OpCodes.Sub);
                break;
            case OpCode.Multiply:
                il.Emit(OpCodes.Mul);
                break;
            case OpCode.Divide:
                il.Emit(OpCodes.Div);
                break;
            case OpCode.Modulo:
                if (instruction is not Instruction<DataType> typedInstruction)
                    throw new Exception();

                if (typedInstruction.Arg1 is DataType.U8 or DataType.U16 or DataType.U32 or DataType.U64)
                {
                    il.Emit(OpCodes.Rem_Un);
                }
                else
                {
                    il.Emit(OpCodes.Rem);
                }
                break;
            case OpCode.Negate:
                il.Emit(OpCodes.Neg);
                break;
            case OpCode.Increment:
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                break;
            case OpCode.Decrement:
                il.Emit(OpCodes.Ldc_I4_M1);
                il.Emit(OpCodes.Add);
                break;
            case OpCode.And:
                il.Emit(OpCodes.And);
                break;
            case OpCode.Or:
                il.Emit(OpCodes.Or);
                break;
            case OpCode.Xor:
                il.Emit(OpCodes.Xor);
                break;
            case OpCode.Not:
                il.Emit(OpCodes.Not);
                break;
            case OpCode.Compare when instruction is Instruction<ComparisonKind, DataType> compInstruction:
                EmitCompare(il, compInstruction);
                break;
            case OpCode.Print:
                il.EmitCall(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) }), null);
                break;
            case OpCode.PrintChar:
                il.EmitCall(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(char) }), null);
                break;
            default:
                break;
        }

        if (instruction.OpCode is not OpCode.If)
        {
            while (ifs.Any()) 
            {
                EndIf(il, ifs.Pop());
            }
        }
    }

    private CIL.Label BeginIf(ILGenerator il, Instruction<ComparisonKind, DataType> ifInstruction)
    {
        var falseCase = il.DefineLabel();

        EmitCompare(il, ifInstruction);
        il.Emit(OpCodes.Brfalse, falseCase);

        return falseCase;
    }
    
    private void EndIf(ILGenerator il, CIL.Label label)
    {
        il.MarkLabel(label);
    }

    private void EmitCompare(ILGenerator il, Instruction<ComparisonKind, DataType> compareInstruction)
    {
        bool unsigned = compareInstruction.Arg2 is DataType.U8 or DataType.U16 or DataType.U32 or DataType.U64 or DataType.F64 or DataType.F32;
        var comparison = compareInstruction.Arg1;

        switch (comparison)
        {
            case ComparisonKind.Equal:
                il.Emit(OpCodes.Ceq);
                break;
            case ComparisonKind.NotEqual:
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Not);
                break;
            case ComparisonKind.GreaterThan:
                il.Emit(unsigned ? OpCodes.Cgt_Un : OpCodes.Cgt);
                break;
            case ComparisonKind.LessThanEqual:
                il.Emit(unsigned ? OpCodes.Cgt_Un : OpCodes.Cgt);
                il.Emit(OpCodes.Not);
                break;
            case ComparisonKind.LessThan:
                il.Emit(unsigned ? OpCodes.Clt_Un : OpCodes.Clt);
                break;
            case ComparisonKind.GreaterThanEqual:
                il.Emit(unsigned ? OpCodes.Clt_Un : OpCodes.Clt);
                il.Emit(OpCodes.Not);
                break;
            case ComparisonKind.Zero:
                il.Emit(OpCodes.Ldc_I4_0);
                goto case ComparisonKind.Equal;
            case ComparisonKind.NotZero:
                il.Emit(OpCodes.Ldc_I4_0);
                goto case ComparisonKind.NotEqual;
            default:
                throw new Exception();
        }
    }

    private ComparisonKind InvertComparison(ComparisonKind comparison)
    {
        return comparison switch
        {
            ComparisonKind.LessThan => ComparisonKind.GreaterThanEqual,
            ComparisonKind.LessThanEqual => ComparisonKind.GreaterThan,
            ComparisonKind.GreaterThan => ComparisonKind.LessThanEqual,
            ComparisonKind.GreaterThanEqual => ComparisonKind.LessThan,
            ComparisonKind.Equal => ComparisonKind.NotEqual,
            ComparisonKind.NotEqual => ComparisonKind.Equal,
            ComparisonKind.Zero => ComparisonKind.NotZero,
            ComparisonKind.NotZero => ComparisonKind.Zero,
            _ => throw new Exception(),
        };
    }

    private static void EmitLoad(ILGenerator il, Instruction<DataType> loadInstruction)
    {
        il.Emit(loadInstruction.Arg1 switch
        {
            DataType.Pointer => OpCodes.Ldind_I,
            DataType.I64 or DataType.U64 => OpCodes.Ldind_I8,
            DataType.I32 or DataType.U32 => OpCodes.Ldind_I4,
            DataType.I16 or DataType.U16 => OpCodes.Ldind_I2,
            DataType.I8 or DataType.U8 => OpCodes.Ldind_I1,
            DataType.F64 => OpCodes.Ldind_R8,
            DataType.F32 => OpCodes.Ldind_R4,
            _ => throw new Exception()
        });
    }

    private static void EmitStore(ILGenerator il, Instruction<DataType> storeInstruction)
    {
        il.Emit(storeInstruction.Arg1 switch
        {
            DataType.Pointer => OpCodes.Stind_I,
            DataType.I64 or DataType.U64 => OpCodes.Stind_I8,
            DataType.I32 or DataType.U32 => OpCodes.Stind_I4,
            DataType.I16 or DataType.U16 => OpCodes.Stind_I2,
            DataType.I8 or DataType.U8 => OpCodes.Stind_I1,
            DataType.F64 => OpCodes.Stind_R8,
            DataType.F32 => OpCodes.Stind_R4,
            _ => throw new Exception()
        });
    }

    private static void EmitCast(ILGenerator il, Instruction<DataType, DataType> castInstruction)
    {
        var targetDataType = castInstruction.Arg2;
        il.Emit(targetDataType switch
        {
            DataType.Pointer => OpCodes.Conv_U,
            DataType.I64 => OpCodes.Conv_U8,
            DataType.I32 => OpCodes.Conv_U4,
            DataType.I16 => OpCodes.Conv_U2,
            DataType.I8 => OpCodes.Conv_U1,
            DataType.U64 => OpCodes.Conv_U8,
            DataType.U32 => OpCodes.Conv_U4,
            DataType.U16 => OpCodes.Conv_U2,
            DataType.U8 => OpCodes.Conv_U1,
            DataType.F64 => OpCodes.Conv_R8,
            DataType.F32 => OpCodes.Conv_R4,
            _ => throw new Exception()
        });
    }

    private void EmitPop(ILGenerator il, Instruction<StackOpKind> instruction)
    {
        switch (instruction.Arg1)
        {
            case StackOpKind.Const when instruction is Instruction<StackOpKind, DataType> typedInstruction:
                il.Emit(OpCodes.Stsfld, dumpField);
                break;
            case StackOpKind.Local when instruction is Instruction<StackOpKind, Local> localInstruction:
                var local = localInstruction.Arg2;
                il.Emit(OpCodes.Stloc, Unsafe.As<Local, short>(ref local));
                break;
            case StackOpKind.Arg when instruction is Instruction<StackOpKind, byte> argInstruction:
                il.Emit(OpCodes.Starg_S, argInstruction.Arg2);
                break;
            case StackOpKind.ArgAddress:
            case StackOpKind.LocalAddress:
                throw new InvalidOperationException("cannot pop into the address of a local or arg");
            case StackOpKind.Resource:
                throw new InvalidOperationException("cannot pop into a resource");
            case StackOpKind.Fp:
            case StackOpKind.Sp:
                throw new NotSupportedException("fp and sp ops are not supported!");
            default:
                throw new Exception();
        }
    }

    private static void EmitPush(ILGenerator il, Instruction<StackOpKind> instruction)
    {
        switch (instruction.Arg1)
        {
            case StackOpKind.Const when instruction is Instruction<StackOpKind, TypedValue> constInstruction:
                var value = constInstruction.Arg2;
                switch (value.Type)
                {
                    case DataType.Pointer:
                        il.Emit(OpCodes.Ldc_I8, value.As<long>());
                        il.Emit(OpCodes.Conv_U);
                        break;
                    case DataType.I64:
                    case DataType.U64:
                        il.Emit(OpCodes.Ldc_I4, value.As<long>());
                        break;
                    case DataType.I32:
                    case DataType.I16:
                    case DataType.I8:
                    case DataType.U32:
                    case DataType.U16:
                    case DataType.U8:
                        il.Emit(OpCodes.Ldc_I4, value.As<int>());
                        break;
                    case DataType.F64:
                        il.Emit(OpCodes.Ldc_R8, value.As<double>());
                        break;
                    case DataType.F32:
                        il.Emit(OpCodes.Ldc_R4, value.As<float>());
                        break;
                    case DataType.Void:
                    default:
                        throw new Exception();
                }
                break;
            case StackOpKind.Local when instruction is Instruction<StackOpKind, Local> localInstruction:
                var local = localInstruction.Arg2;
                il.Emit(OpCodes.Ldloc, Unsafe.As<Local, short>(ref local));
                break;
            case StackOpKind.LocalAddress when instruction is Instruction<StackOpKind, Local> localInstruction:
                local = localInstruction.Arg2;
                il.Emit(OpCodes.Ldloca, Unsafe.As<Local, short>(ref local));
                break;
            case StackOpKind.Arg when instruction is Instruction<StackOpKind, byte> argInstruction:
                il.Emit(OpCodes.Ldarg_S, argInstruction.Arg2);
                break;
            case StackOpKind.ArgAddress when instruction is Instruction<StackOpKind, byte> argInstruction:
                il.Emit(OpCodes.Ldarga_S, argInstruction.Arg2);
                break;
            case StackOpKind.Resource:
                throw new NotImplementedException();
            case StackOpKind.Fp:
            case StackOpKind.Sp:
                throw new NotSupportedException("fp and sp ops are not supported!");
            default:
                throw new Exception();
        }
    }

    private static void InitResources()
    {

    }
}
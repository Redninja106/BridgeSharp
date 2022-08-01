using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mono.Reflection;
using CIL = System.Reflection.Emit;
using CILModuleBuilder = System.Reflection.Emit.ModuleBuilder;
using NativeCallingConvention = System.Runtime.InteropServices.CallingConvention;

namespace Bridge;

/// <summary>
/// Experimental bridge to .NET cil compiler
/// </summary>
public class CILCompiler
{
    private readonly Module module;
    private readonly List<(Definition Definition, MethodBuilder Method)> methodBuilders = new();
    private Stack<CIL.Label> ifs = new();
    private FieldInfo resources;
    private int[] resourceOffsets;

    public static void DumpModuleIL(TextWriter writer, Module module, MethodInfo entryPoint)
    {
        if (entryPoint.Name is not "__entrypoint")
            throw new Exception();

        var type = entryPoint.DeclaringType;

        foreach (var routine in module.Routines)
        {
            var method = type.GetMethod(routine.Name);

            if (method is null)
                return;
            
            var instructions = method.GetInstructions();

            writer.WriteLine($"emitted code for routine '{routine.Name}'");

            writer.WriteLine("{");

            foreach (var instruction in instructions)
            {
                writer.WriteLine("    " + instruction.ToString());
            }

            writer.WriteLine("}");
        }
    }

    internal CILCompiler(Module module)
    {
        this.module = module;
    }

    internal MethodInfo Compile(AssemblyBuilder builder)
    {
        var moduleBuilder = builder.DefineDynamicModule("module");
        var typeBuilder = moduleBuilder.DefineType("__Program");
        
        var entrypoint = typeBuilder.DefineMethod("__entrypoint", MethodAttributes.Static | MethodAttributes.Public);

        var dataType = typeof(nuint);//.MakePointerType();
        resources = typeBuilder.DefineField("__resources", dataType, FieldAttributes.Private | FieldAttributes.Static);
        
        var offset = 0;
        List<int> resourceOffsets = new List<int>();
        foreach (var e in module.ResourceTable.Entries)
        {
            resourceOffsets.Add(offset);
            offset += e.Data.Length;
        }
        this.resourceOffsets = resourceOffsets.ToArray();
        
        foreach (var definition in module.Definitions)
        {
            CompileDefinition(definition, typeBuilder);
        }

        foreach (var definition in module.Definitions)
        {
            CompileDefinitionBody(definition, typeBuilder);
        }
        
        var il = entrypoint.GetILGenerator();
        EmitResources(il, moduleBuilder, typeBuilder);

        il.EmitCall(OpCodes.Call, methodBuilders.Single(pair => pair.Definition.Name is "main").Method, null);

        EmitFreeResources(il);
        il.Emit(OpCodes.Ret);

        var compiledType = typeBuilder.CreateType();
        moduleBuilder.CreateGlobalFunctions();
        
        var entry = compiledType.GetMethod("__entrypoint");
        return entry;
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
            case ExternDefinition externDefinition:
                CompileExtern(externDefinition, typeBuilder);
                break;
            default:
                break;
        }
    }

    private void CompileExtern(ExternDefinition externDefinition, TypeBuilder typeBuilder)
    {
        var name = externDefinition.Name;
        var library = externDefinition.Library;
        var returnType = externDefinition.ReturnType is DataType.Void ? typeof(void) : TypedValue.GetDataTypePrimitive(externDefinition.ReturnType);
        var parameterTypes = externDefinition.Parameters.Select(TypedValue.GetDataTypePrimitive).ToArray();
        var callingConvention = externDefinition.CallingConvention switch
        {
            CallingConvention.Cdecl => NativeCallingConvention.Cdecl,
            CallingConvention.StdCall => NativeCallingConvention.StdCall,
            _ => throw new Exception(),
        };

        var method = typeBuilder.DefinePInvokeMethod(name, library, MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, returnType, parameterTypes, callingConvention, CharSet.None);
        method.SetCustomAttribute(typeof(PreserveSigAttribute).GetConstructor(Array.Empty<Type>()), new byte[0]);
        methodBuilders.Add((externDefinition, method));
    }

    private void CompileRoutine(RoutineDefinition routine, TypeBuilder typeBuilder)
    {
        var returnType = routine.ReturnType is DataType.Void ? typeof(void) : TypedValue.GetDataTypePrimitive(routine.ReturnType);
        var parameters = routine.Parameters.Select(p => TypedValue.GetDataTypePrimitive(p)).ToArray();
        var method = typeBuilder.DefineMethod(routine.Name, MethodAttributes.Static | MethodAttributes.Public, returnType, parameters);
        
        methodBuilders.Add((routine, method));
    }

    private void CompileRoutineBody(RoutineDefinition routine, TypeBuilder typeBuilder)
    {
        var (_, method) = methodBuilders.First(m => m.Definition == routine);

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
            case OpCode.Call when instruction is Instruction<CallMode> callInstruction:
                EmitCall(il, callInstruction);
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
            case OpCode.Print when instruction is Instruction<DataType> printInstruction:
                Type type = printInstruction.Arg1 switch
                {
                    DataType.I8 or DataType.I16 or DataType.I32 => typeof(int),
                    DataType.U8 or DataType.U16 or DataType.U32 => typeof(uint),
                    DataType.I64 => typeof(long),
                    DataType.U64 => typeof(ulong),
                    DataType.Pointer => typeof(ulong)
                };

                if (printInstruction.Arg1 is DataType.Pointer)
                {
                    il.Emit(OpCodes.Conv_U8);
                }

                il.EmitCall(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { type }), null);
                break;
            case OpCode.PrintChar when instruction is Instruction<DataType> printCharInstruction:
                il.EmitCall(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(char) }), null);
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

    private void EmitCall(ILGenerator il, Instruction<CallMode> instruction)
    {
        switch (instruction.Arg1)
        {
            case CallMode.Direct:
            case CallMode.Tail:
                if (instruction.Arg1 is CallMode.Tail)
                    il.Emit(OpCodes.Tailcall);
                
                var directCallInstruction = (Instruction<CallMode, int>)instruction;
                il.EmitCall(OpCodes.Call, methodBuilders.Single(pair => pair.Definition.ID == directCallInstruction.Arg2).Method, null);
                break;
            case CallMode.Indirect:
            case CallMode.TailIndirect:
                if (instruction.Arg1 is CallMode.TailIndirect)
                    il.Emit(OpCodes.Tailcall);

                var indirectCallInstruction = (Instruction<CallMode, CallInfo>)instruction;
                var ret = TypedValue.GetDataTypePrimitive(indirectCallInstruction.Arg2.ReturnType);
                var parameters = indirectCallInstruction.Arg2.Parameters.Select(TypedValue.GetDataTypePrimitive).ToArray();

                if (indirectCallInstruction.Arg2.CallingConvention is CallingConvention.Bridge)
                {
                    il.EmitCalli(OpCodes.Call, CallingConventions.Standard, ret, parameters, null);
                }

                var conv = ConvertCallingConvention(indirectCallInstruction.Arg2.CallingConvention);
                il.EmitCalli(OpCodes.Calli, conv, ret, parameters);
                break;
            default:
                throw new Exception();
        }
    }

    private NativeCallingConvention ConvertCallingConvention(CallingConvention conv) => conv switch
    {
        CallingConvention.Cdecl => NativeCallingConvention.Cdecl,
        CallingConvention.StdCall => NativeCallingConvention.StdCall,
        _ => throw new Exception()
    };

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
            DataType.I32 => OpCodes.Ldind_I4,
            DataType.I16 => OpCodes.Ldind_I2,
            DataType.I8 => OpCodes.Ldind_I1,
            DataType.U32 => OpCodes.Ldind_U4,
            DataType.U16 => OpCodes.Ldind_U2,
            DataType.U8 => OpCodes.Ldind_U1,
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
                il.Emit(OpCodes.Pop);
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
            default:
                throw new Exception();
        }
    }

    private void EmitPush(ILGenerator il, Instruction<StackOpKind> instruction)
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
            case StackOpKind.Resource when instruction is Instruction<StackOpKind, Index> resourceInstruction:
                var offset = resourceOffsets[resourceInstruction.Arg2];
                il.Emit(OpCodes.Ldsfld, this.resources);
                il.Emit(OpCodes.Ldc_I4, offset);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Conv_U);
                break;
            case StackOpKind.Routine when instruction is Instruction<StackOpKind, int> routineInstruction:
                var method = this.methodBuilders[routineInstruction.Arg2].Method;
                il.Emit(OpCodes.Ldftn, method);
                break;
            default:
                throw new Exception();
        }
    }

    private void EmitResources(ILGenerator il, CILModuleBuilder moduleBuilder, TypeBuilder typeBuilder)
    {
        var length = 0;
        foreach (var entry in module.ResourceTable.Entries)
        {
            length += entry.Data.Length;
        }

        il.Emit(OpCodes.Ldc_I4, length);
        il.Emit(OpCodes.Conv_U);
        il.EmitCall(OpCodes.Call, typeof(NativeMemory).GetMethod(nameof(NativeMemory.Alloc), new Type[] { typeof(nuint) }), null);
        il.Emit(OpCodes.Stsfld, resources);

        for (int i = 0; i < module.ResourceTable.EntryCount; i++)
        {
            var entry = module.ResourceTable.GetResource(i);
            var offset = resourceOffsets[i];

            var fld = moduleBuilder.DefineInitializedData("resource" + i, entry.Data, FieldAttributes.Static);

            il.Emit(OpCodes.Ldsfld, resources);
            il.Emit(OpCodes.Ldc_I4, offset);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Ldsflda, fld);
            il.Emit(OpCodes.Ldc_I4, entry.Data.Length);
            il.Emit(OpCodes.Cpblk);
        }
    }

    private void EmitFreeResources(ILGenerator il)
    {
        il.Emit(OpCodes.Ldsfld, resources);
        il.EmitCall(OpCodes.Call, typeof(NativeMemory).GetMethod(nameof(NativeMemory.Free)), null);
        il.Emit(OpCodes.Ret);
    }
}

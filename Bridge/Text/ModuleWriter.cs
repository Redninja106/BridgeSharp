using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Bridge.Text;

internal class ModuleWriter
{
    private readonly Module module;

    public ModuleWriter(Module module)
    {
        this.module = module;
    }

    public void WriteModule(TextWriter writer)
    {
        foreach (var def in module.Definitions)
        {
            WriteDefinition(writer, def);
        }
    }

    private void WriteDefinition(TextWriter writer, Definition definition)
    {
        switch (definition)
        {
            case RoutineDefinition routine:
                WriteRoutine(writer, routine);
                break;
            case ExternDefinition ex:
                WriteExtern(writer, ex);
                break;
            default:
                break;
        }
    }
    
    private void WriteExtern(TextWriter writer, ExternDefinition definition)
    {
        writer.Write("extern ");

        if (definition.ReturnType != DataType.Void)
        {
            WriteDataType(writer, definition.ReturnType, false);
            writer.Write(" ");
        }

        writer.Write(definition.Name);

        if (definition.Parameters.Any())
        {
            writer.Write("(");
            WriteDataType(writer, definition.Parameters.First(), false);
            foreach (var param in definition.Parameters.Skip(1))
            {
                writer.Write(",");
                WriteDataType(writer, param, false);
            }
            writer.Write(")");
        }

        writer.WriteLine();
        writer.WriteLine("{");
        const string tab = "    ";
        writer.WriteLine($"{tab}library \"{definition.Library}\"");
        writer.WriteLine("}");
    }

    private void WriteRoutine(TextWriter writer, RoutineDefinition routine)
    {
        writer.Write("routine ");

        if (routine.ReturnType != DataType.Void)
        {
            WriteDataType(writer, routine.ReturnType, false);
            writer.Write(" ");
        }

        writer.Write(routine.Name);

        if (routine.Parameters.Any())
        {
            writer.Write("(");
            WriteDataType(writer, routine.Parameters.First(), false);
            foreach (var param in routine.Parameters.Skip(1))
            {
                writer.Write(",");
                WriteDataType(writer, param, false);
            }
            writer.Write(")");
        }

        writer.WriteLine();
        writer.WriteLine("{");

        const string defaultTab = "    ";
        const string IfTab = "        ";
        
        foreach (var local in routine.Locals)
        {
            writer.Write(defaultTab);
            writer.Write("local");
            WriteDataType(writer, local);
            writer.WriteLine();
        }


        string currentTab = defaultTab;
        for (int i = 0; i < routine.Instructions.Length; i++)
        {
            for (int j = 0; j < routine.LabelLocations.Length; j++)
            {
                var location = routine.LabelLocations[j];
                if (location == i)
                {
                    WriteLabel(writer, location, true);
                    writer.WriteLine();
                }
            }

            var instruction = routine.Instructions[i];

            writer.Write(currentTab);

            if (currentTab == IfTab)
                currentTab = defaultTab;

            WriteOpCode(writer, instruction);
            switch (instruction)
            {
                case Instruction<DataType, DataType> castInstruction:
                    WriteDataType(writer, castInstruction.Arg1);
                    WriteDataType(writer, castInstruction.Arg2);
                    break;
                case Instruction<CallMode, int> callInstruction:
                    WriteModifier(writer, callInstruction);
                    writer.Write(" ");
                    writer.Write(module.FindDefinition(callInstruction.Arg2).Name);
                    break;
                case Instruction<StackOpKind, byte> argInstruction:
                    WriteModifier(writer, argInstruction);
                    writer.Write(" ");
                    writer.Write(argInstruction.Arg2);
                    break;
                case Instruction<StackOpKind, TypedValue> constInstruction:
                    WriteModifier(writer, constInstruction);
                    TypedValue value = constInstruction.Arg2;
                    WriteDataType(writer, value.Type);
                    writer.Write(" ");
                    switch (value.Type)
                    {
                        case DataType.Pointer:
                            writer.Write("0x");
                            writer.Write(value.As<nuint>().ToString("x"));
                            break;
                        case DataType.I64:
                            writer.Write(value.As<long>());
                            break;
                        case DataType.I32:
                            writer.Write(value.As<int>());
                            break;
                        case DataType.I16:
                            writer.Write(value.As<short>());
                            break;
                        case DataType.I8:
                            writer.Write(value.As<sbyte>());
                            break;
                        case DataType.U64:
                            writer.Write(value.As<ulong>());
                            break;
                        case DataType.U32:
                            writer.Write(value.As<uint>());
                            break;
                        case DataType.U16:
                            writer.Write(value.As<ushort>());
                            break;
                        case DataType.U8:
                            writer.Write(value.As<byte>());
                            break;
                        case DataType.F64:
                            writer.Write(value.As<double>());
                            break;
                        case DataType.F32:
                            writer.Write(value.As<float>());
                            break;
                        case DataType.Void:
                        default:
                            writer.Write("?");
                            break;
                    }
                    break;
                case Instruction<StackOpKind, Local> localInstruction:
                    WriteModifier(writer, localInstruction);
                    writer.Write(" ");
                    writer.Write(localInstruction.Arg2.Value);
                    break;
                case Instruction<StackOpKind, DataType> typedInstruction:
                    WriteModifier(writer, typedInstruction);
                    WriteDataType(writer, typedInstruction.Arg2);
                    break;
                case Instruction<StackOpKind, Index> resourceInstruction:
                    WriteModifier(writer, resourceInstruction);
                    writer.Write(" ");
                    switch (module.ResourceTable.GetKind(resourceInstruction.Arg2))
                    {
                        case ResourceKind.String8:
                            WriteString(writer, EscapeString(module.ResourceTable.GetResourceString(resourceInstruction.Arg2, Encoding.UTF8)));
                            break;
                        case ResourceKind.String16:
                            WriteString(writer, EscapeString(module.ResourceTable.GetResourceString(resourceInstruction.Arg2, Encoding.Unicode)));
                            break;
                        case ResourceKind.Unknown:
                        default:
                            writer.Write(resourceInstruction.Arg2);
                            break;
                    }
                    break;
                case Instruction<StackOpKind, int> routineInstruction:
                    WriteModifier(writer, routineInstruction);
                    writer.Write(" ");
                    writer.Write(module.FindDefinition(routineInstruction.Arg2).Name);
                    break;
                case Instruction<StackOpKind> stackInstruction:
                    writer.Write(stackInstruction.Arg1.ToString().ToLower());
                    break;
                case Instruction<ComparisonKind, DataType> conditionalInstruction:
                    writer.Write(".");
                    WriteComparison(writer, conditionalInstruction.Arg1);
                    WriteDataType(writer, conditionalInstruction.Arg2);
                    if (instruction.OpCode is OpCode.If)
                        currentTab = IfTab;
                    break;
                case Instruction<DataType> typedInstruction:
                    WriteDataType(writer, typedInstruction.Arg1);
                    break;
                case Instruction<Label> jumpInstruction:
                    writer.Write(" ");
                    WriteLabel(writer, routine.LabelLocations[jumpInstruction.Arg1.Value], false);
                    break;
                default:
                    break;
            }

            writer.WriteLine();
        }

        writer.WriteLine("}");
    }

    private static void WriteString(TextWriter writer, string value)
    {
        writer.Write("\"");
        writer.Write(value.Replace("\"", "\\\""));
        writer.Write("\"");
    }

    private static void WriteOpCode(TextWriter writer, Instruction instruction)
    {
        writer.Write(instruction.OpCode.ToString().ToLower());
    }
    
    private static void WriteModifier<T>(TextWriter writer, Instruction<T> instruction)
    {
        writer.Write(".");
        writer.Write(instruction.Arg1.ToString()?.ToLower());
    }

    private static void WriteLabel(TextWriter writer, int location, bool colon)
    {
        writer.Write("inst");
        writer.Write(location.ToString());
        if (colon)
            writer.Write(":");
    }

    private static void WriteDataType(TextWriter writer, DataType type, bool dotPrefix = true)
    {
        if (type is DataType.Void)
        {
            return;
        }

        if (type is DataType.Pointer)
        {
            writer.Write("*");
            return;
        }

        if (dotPrefix)
            writer.Write(".");

        writer.Write(type.ToString().ToLower());
    }

    private void WriteComparison(TextWriter writer, ComparisonKind kind)
    {
        writer.Write(kind switch
        {
            ComparisonKind.Equal => "eq",
            ComparisonKind.NotEqual => "neq",
            ComparisonKind.LessThan => "lt",
            ComparisonKind.LessThanEqual => "lte",
            ComparisonKind.GreaterThan => "gt",
            ComparisonKind.GreaterThanEqual => "gte",
            ComparisonKind.Zero => "zero",
            ComparisonKind.NotZero => "notzero",
            _ => throw new Exception()
        });
    }

    private string EscapeString(string s)
    {
        return s.Replace("\"", "\\\"")
            .Replace("\\", "\\\\")
            .Replace("\t", "\\t")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\0", "\\0");
    }
}

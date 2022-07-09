using Bridge.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;
public sealed class Interpreter
{
    private Module module;
    private Stack stack;

    public void RunModule(Module module)
    {
        if (!module.IsExecutable)
            throw new Exception("This module is not executable (has no 'main' routine)");

        this.module = module;
        this.stack = new();

        var code = module.GetSection<ModuleCodeSection>();
        var main = code.Routines.FirstOrDefault(d => module.GetDataEntryString(d.Name) == "main");

        RunRoutine(main);
    }

    private void RunRoutine(Routine routine)
    {
        stack.BeginFrame(routine.Locals.Count);

        foreach (var instruction in routine.Instructions)
        {
            EvalInstruction(instruction);
        }

        stack.EndFrame(routine.Locals.Count);
    }

    private void EvalInstruction(Instruction instruction)
    {
        switch (instruction)
        {
            case ConstInstruction constInstruction:
                EvalConstInstruction(constInstruction);
                break;
            case LocalInstruction localInstruction:
                EvalLocalInstruction(localInstruction);
                break;
            case DataEntryInstruction dataEntryInstruction:
                EvalDataEntryInstruction(dataEntryInstruction);
                break;
            default:
                EvalStandardInstruction(instruction);
                break;
        }
    }

    private void EvalStandardInstruction(Instruction instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.Print: Console.WriteLine(stack.Pop()); break;
            case OpCode.Add: stack.Push(stack.Pop() + stack.Pop()); break;
            case OpCode.Sub: stack.Push(stack.Pop() - stack.Pop()); break;
            case OpCode.Mul: stack.Push(stack.Pop() * stack.Pop()); break;
            case OpCode.Div: stack.Push(stack.Pop() / stack.Pop()); break;
            case OpCode.Ret: break;
            default:
                throw new Exception("Evaluation Error: Unknown instruction or bad instruction state");
        }
    }

    private void EvalConstInstruction(ConstInstruction instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.PushConst:
                stack.Push(instruction.Value);
                break;
            default:
                throw new Exception("Evaluation Error: Unknown instruction or bad instruction state");
        }
    }

    private void EvalLocalInstruction(LocalInstruction instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.Push:
                stack.Push(instruction.Mode, instruction.Local);
                break;
            case OpCode.Pop:
                stack.Pop(instruction.Mode, instruction.Local);
                break;
            default:
                throw new Exception("Evaluation Error: Unknown instruction or bad instruction state");
        }
    }

    private void EvalDataEntryInstruction(DataEntryInstruction instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.Call:
                var codeSection = module.GetSection<ModuleCodeSection>();
                var routine = codeSection.Routines.First(d => d.Name == instruction.DataEntry);
                
                if (routine is null)
                    throw new Exception("Evaluation Error: Define not found");

                RunRoutine(routine);
                break;
            case OpCode.CallIf:
                if (stack.Pop() != 0)
                    goto case OpCode.Call;
                break;
            default:
                throw new Exception("Evaluation Error: Unknown instruction or bad instruction state");
        }
    }
    
    class Stack
    {
        InspectableStack<long> values = new();
        InspectableStack<long> locals = new();

        public void Push(long value)
        {
            values.Push(value);
        }
        
        public void BeginFrame(int localCount)
        {
            for (int i = 0; i < localCount; i++)
            {
                locals.Push(0);
            }
        }

        public void EndFrame(int localCount)
        {
            for (int i = 0; i < localCount; i++)
            {
                locals.Pop();
            }
        }

        public void Push(LocalAccessMode mode, byte local)
        {
            switch (mode)
            {
                case LocalAccessMode.Normal:
                    values.Push(locals.Peek(-local)); 
                    break;
                case LocalAccessMode.Dereference:
                    values.Push(locals.Peek(-(int)locals.GetValue(local))); 
                    break;
                case LocalAccessMode.Address:
                    values.Push(locals.Location + local); 
                    break;
                default:
                    throw new Exception("Unknown LocalAccessMode");
            }
        }

        public void Pop(LocalAccessMode mode, byte local)
        {
            switch (mode)
            {
                case LocalAccessMode.Normal:
                    locals.Peek(-local) = values.Pop();
                    break;
                case LocalAccessMode.Dereference:
                    locals.Peek(-(int)locals.GetValue(local)) = values.Pop();
                    break;
                case LocalAccessMode.Address:
                    throw new Exception("Invalid instruction");
                default:
                    throw new Exception("Unknown LocalAccessMode");
            }
        }
        
        public long Pop()
        {
            return values.Pop();
        }
    }

    class InspectableStack<T>
    {
        T[] values = new T[4];
        
        public int Location { get; set; }

        public void Push(T value)
        {
            GetValue(++Location) = value;
        }
        
        public ref T Peek(int offset)
        {
            return ref GetValue(Location + offset);
        }

        public T Pop()
        {
            return GetValue(Location--);
        }

        public ref T GetValue(int index)
        {
            if (index >= values.Length)
                Array.Resize(ref values, values.Length << 1);

            return ref values[index];
        }
    }
}

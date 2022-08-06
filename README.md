# Bridge
A library for compiling assembly-like instructions to a variety of platforms

## About
The Bridge project was created to simplify the process of generating
code using an appropriate syntax. The bridge syntax is not too complex,
and has features such as locals and routines, while still giving the
same control that assembly-like languages provide.

## IL Example
```
routine main
{
    push.const.i32 5
    push.const.i32 10
    add.i32
    print.i32
    return
}
```

## API Example

```csharp
using Bridge;

// Create a module
ModuleBuilder module = Module.CreateBuilder();

// Create a main routine
RoutineBuilder main = module.AddRoutine("main");
main.ReturnType = DataType.Void;

// Create a code section
CodeBuilder code = main.GetCodeBuilder();

// Add 5 and 10
code.Emit(Instruction.PushConst<int>(5));
code.Emit(Instruction.PushConst<int>(10));
code.Emit(Instruction.Add(DataType.I32));

// Print 15
code.Emit(Instruction.Print(DataType.I32));

// Return
code.Emit(Instruction.Return());

// Run the module using dotnet CIL
var result = Module.Compile(module.CreateModule());
result.Invoke(null, null);
```

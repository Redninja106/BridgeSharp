using Bridge;
using Bridge.Verification;
using Mono.Reflection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static Bridge.Instruction;
using Module = Bridge.Module;

var mod = Test();
Module.Dump(mod, Console.Out);

if (Module.Verify(mod, out var messages))
{
    messages.PrintToConsole();
    var entry = Module.Compile(mod, out var assembly);
    entry.Invoke(null, null);

    CILCompiler.DumpModuleIL(Console.Out, mod, entry);
}
else
{
    messages.PrintToConsole();
}
//var modBuilder = Module.Create();

////var main = modBuilder.AddRoutine("main");
////var mc = main.GetCodeBuilder();

////var print = modBuilder.AddRoutine("print");
////print.AddParameter(DataType.I32);
////var pc = print.GetCodeBuilder();

////mc.Emit(Push(TypedValue.Create(69)));
////mc.Emit(Call(print));
////mc.Emit(Return());

////pc.Emit(PushArg(0));
////pc.Emit(Print(DataType.I32));
////pc.Emit(Return());

//var main = modBuilder.AddRoutine("main");


//var maincode = main.GetCodeBuilder();
// maincode.Emit(PushResource(modBuilder.AddResource("Hello, world!\0", Encoding.UTF8)));
// maincode.Emit(Call(printstr));
//maincode.Emit(Return());



//var mod = modBuilder.CreateModule();

//Module.Dump(mod, Console.Out);

// using var i = new Interpreter();
// i.Run(mod);


//static class E
//{
//    [DllImport("ucrtbase.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
//    private static extern nuint malloc(nuint size);
//    [DllImport("ucrtbase.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
//    private static extern void free(nuint ptr);

//    public static void F()
//    {
//        var ptr = malloc(16);
//        free(ptr);
//    }
//}

//modBuilder.AddResource("hey!");

//var main = modBuilder.AddRoutine("main");
//var test = modBuilder.AddRoutine("test");
//test.AddParameter(DataType.I32);

//var maincode = main.GetCodeBuilder();
//maincode.Emit(Push(TypedValue.Create(10)));
//maincode.Emit(Call(test));
//maincode.Emit(Return());

//var testcode = test.GetCodeBuilder();
//var loopcond = testcode.AddLabel();
//var loopbody = testcode.AddLabel();

//var l0 = testcode.AddLocal(DataType.I32);

//testcode.Emit(Push(TypedValue.Create(0)));
//testcode.Emit(Pop(l0));
//testcode.Emit(Jump(loopcond));

//testcode.MoveLabel(loopbody);
//testcode.Emit(Push(l0));
//testcode.Emit(Push(TypedValue.Create(1)));
//testcode.Emit(Add(DataType.I32));
//testcode.Emit(Pop(l0));

//testcode.Emit(Push(l0));
//testcode.Emit(Print(DataType.I32));

//testcode.MoveLabel(loopcond);
//testcode.Emit(Push(l0)); 
//testcode.Emit(PushArg(0));
//testcode.Emit(If(ComparisonKind.LessThan, DataType.I32));
//testcode.Emit(Jump(loopbody));
//testcode.Emit(Return());

/*routine main 
{
  push.const.i32 10
  call test
  return
}

routine test(i32) 
{
  local.i32
  push.const.i32 0
  pop.local 0
  jump loopcond
loopbody:
  push.local 0
  push.const.i32 1
  add.i32
  pop.local 0

  push.local 0
  print.i32

loopcond:
  push.arg 0
  push.local 0
  if.lt.i32 
    jump loopcond
  return
}*/

//var main = modBuilder.AddRoutine("main");
//var code = main.GetCodeBuilder();
//var loc = code.AddLocal(DataType.Pointer);
//code.Emit(PushResource(modBuilder.AddResource("Hello, world!\0", Encoding.UTF8)));
//code.Emit(Pop(loc));
//var start = code.AddLabel();
//code.Emit(Push(loc));
//code.Emit(Load(DataType.U8));
//code.Emit(PrintChar(DataType.U8));

//code.Emit(Push(loc));
//code.Emit(Load(DataType.U8));
//code.Emit(If(ComparisonKind.Zero, DataType.U8));
//code.Emit(Return());

//code.Emit(Push(loc));
//code.Emit(Push(TypedValue.Create<nuint>(1)));
//code.Emit(Add(DataType.Pointer));
//code.Emit(Pop(loc));

//code.Emit(Jump(start));

//code.Emit(Return());


//pscode.Emit(Return());

Module Test()
{
    var builder = Module.CreateBuilder();
    var main = builder.AddRoutine("main");
    var pow = builder.AddRoutine("pow");
    pow.ReturnType = DataType.I32;
    pow.AddParameter(DataType.I32);
    pow.AddParameter(DataType.I32);

    var mainc = main.GetCodeBuilder();
    mainc.Emit(PushConst(32));
    mainc.Emit(PushConst(3));
    mainc.Emit(Call(pow));
    mainc.Emit(Print(DataType.I32));
    mainc.Emit(Return());

    var powc = pow.GetCodeBuilder();
    var inst15 = powc.AddLabel();
    var inst17 = powc.AddLabel();

    powc.Emit(PushArg(1));
    powc.Emit(PushConst(1));
    powc.Emit(Compare(ComparisonKind.Equal, DataType.I32));
    powc.Emit(Cast(DataType.I8, DataType.I32));
    powc.Emit(If(ComparisonKind.NotZero, DataType.I32));
    powc.Emit(Jump(inst15));
    powc.Emit(PushArg(0));
    powc.Emit(PushArg(0));
    powc.Emit(PushArg(1));
    powc.Emit(PushConst(1));
    powc.Emit(Subtract(DataType.I32));
    powc.Emit(Call(pow));
    powc.Emit(Multiply(DataType.I32));
    powc.Emit(Return());
    powc.Emit(Jump(inst17));

    powc.MoveLabel(inst15);
    powc.Emit(PushArg(0));
    powc.Emit(Return());

    powc.MoveLabel(inst17);
    powc.Emit(Pop(DataType.I32));
    powc.Emit(Return());

    var mod = builder.CreateModule();

    Module.Dump(mod, Console.Out);

    if (Module.Verify(mod, out var messages))
    {
        messages.PrintToConsole();
        var i = new Interpreter();
        i.Run(mod);
        
        //var entry = Module.Compile(mod);
        //entry.Invoke(null, null);
    }
    else
    {
        messages.PrintToConsole();
    }

    return mod;
}

Module MallocTest()
{
    var modBuilder = Module.CreateBuilder();

    var printstr = modBuilder.AddRoutine("printstr");

    var malloc = modBuilder.AddExtern("malloc");
    malloc.Library = "ucrtbase.dll";
    malloc.CallingConvention = Bridge.CallingConvention.Cdecl;
    malloc.ReturnType = DataType.Pointer;
    malloc.AddParameter(DataType.Pointer);


    var free = modBuilder.AddExtern("free");
    free.Library = "ucrtbase.dll";
    free.CallingConvention = Bridge.CallingConvention.Cdecl;
    free.ReturnType = DataType.Void;
    free.AddParameter(DataType.Pointer);

    var messagebox = modBuilder.AddExtern("MessageBoxA");
    messagebox.Library = "user32";
    messagebox.CallingConvention = Bridge.CallingConvention.StdCall;
    messagebox.ReturnType = DataType.I32;
    messagebox.AddParameter(DataType.Pointer);
    messagebox.AddParameter(DataType.Pointer);
    messagebox.AddParameter(DataType.Pointer);
    messagebox.AddParameter(DataType.U32);

    var failureMessage = modBuilder.AddResource("malloc failed!\0", Encoding.Unicode);
    var successMessage = modBuilder.AddResource("malloc'ed and freed 16 bytes!\0", Encoding.Unicode);

    var main = modBuilder.AddRoutine("main");
    var c = main.GetCodeBuilder();

    var end = c.AddLabel();
    var failure = c.AddLabel();

    var ptr = c.AddLocal(DataType.Pointer);

    c.Emit(PushConst<nuint>(16));
    c.Emit(PushRoutine(malloc.ID));
    c.Emit(CallIndirect(new(malloc.ReturnType, malloc.Parameters.ToArray(), Bridge.CallingConvention.Cdecl)));
    c.Emit(PopLocal(ptr));

    c.Emit(PushLocal(ptr));
    c.Emit(Cast(DataType.Pointer, DataType.U64));
    c.Emit(Print(DataType.Pointer));

    c.Emit(PushLocal(ptr));
    c.Emit(If(ComparisonKind.Zero, DataType.Pointer));
    c.Emit(Jump(failure));

    c.Emit(PushLocal(ptr));
    c.Emit(Call(free));

    c.Emit(PushResource(successMessage));
    c.Emit(Call(printstr));

    c.Emit(Jump(end));

    c.MoveLabel(failure);

    c.Emit(PushResource(failureMessage));
    c.Emit(Call(printstr));

    c.MoveLabel(end);
    c.Emit(Return());

    printstr.AddParameter(DataType.Pointer);
    var pscode = printstr.GetCodeBuilder();
    var start = pscode.AddLabel();
    pscode.Emit(PushArg(0));
    pscode.Emit(Load(DataType.U16));

    pscode.Emit(If(ComparisonKind.Zero, DataType.U16));
    pscode.Emit(Return());

    pscode.Emit(PushArg(0));
    pscode.Emit(Load(DataType.U16));
    pscode.Emit(PrintChar(DataType.U16));

    pscode.Emit(PushArg(0));
    pscode.Emit(PushConst<nuint>(2));
    pscode.Emit(Add(DataType.Pointer));
    pscode.Emit(PopArg(0));

    pscode.Emit(Jump(start));

    return modBuilder.CreateModule();
}

void main()
{
    Console.WriteLine(pow(53,3));
}

int pow(int x, int y)
{
    if (y == 1)
        return x;
    else
        return x * pow(x, y - 1);
}
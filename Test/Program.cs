using Bridge;
using Bridge.Verification;
using Mono.Reflection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static Bridge.Instruction;
using Module = Bridge.Module;


//var mod = Module.Parse("test5.br");

//Module.Dump(mod, Console.Out);
//Module.Verify(mod, out var messages);
//messages.PrintToConsole();

const bool compiled = true;
var mod = CallIndirectTest();


Module.Dump(mod, Console.Out);

if (Module.Verify(mod, out var messages))
{
    messages.PrintToConsole();
    if (compiled)
    {
        var entry = Module.Compile(mod, out var assembly);
        CILCompiler.DumpModuleIL(Console.Out, mod, entry);
        entry.Invoke(null, null);
    }
    else
    {
        var i = new Interpreter();
        i.Run(mod);
    }
}
else
{
    messages.PrintToConsole();
}

Module CallIndirectTest()
{
    var builder = Module.CreateBuilder();

    var main = builder.AddRoutine("main");
    var hey = builder.AddRoutine("hey");
    hey.AddParameter(DataType.I32);

    var mc = main.GetCodeBuilder();
    mc.Emit(Push(14));
    mc.Emit(PushRoutine(hey));
    mc.Emit(CallIndirect(new CallInfo(DataType.Void, new DataType[] { DataType.I32 }, Bridge.CallingConvention.Bridge)));
    mc.Emit(Return());

    var hc = hey.GetCodeBuilder();
    hc.Emit(Push(new Argument(0)));
    hc.Emit(Print(DataType.I32));
    hc.Emit(Return());

    return builder.CreateModule();
}

Module ReadCharTest()
{
    var builder = Module.CreateBuilder();

    var main = builder.AddRoutine("main");

    var mc = main.GetCodeBuilder();
    var start = mc.AddLabel();

    mc.Emit(ReadChar(DataType.I8));
    mc.Emit(PrintChar(DataType.I8));

    mc.Emit(Jump(start));

    return builder.CreateModule();
}

// var modBuilder = Module.Create();

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
    pow.SetReturn(DataType.I32);
    pow.AddParameter(DataType.I32);
    pow.AddParameter(DataType.I32);

    var mainc = main.GetCodeBuilder();
    mainc.Emit(Push(32));
    mainc.Emit(Push(3));
    mainc.Emit(CallDirect(pow));
    mainc.Emit(Print(DataType.I32));
    mainc.Emit(Return());

    var powc = pow.GetCodeBuilder();
    var inst15 = powc.AddLabel();
    var inst17 = powc.AddLabel();

    powc.Emit(Push((Argument)(new(1))));
    powc.Emit(Push(1));
    powc.Emit(Compare(ComparisonKind.Equal, DataType.I32));
    powc.Emit(Cast(DataType.I8, DataType.I32));
    powc.Emit(If(ComparisonKind.NotZero, DataType.I32));
    powc.Emit(Jump(inst15));
    powc.Emit(Push((Argument)(new(0))));
    powc.Emit(Push((Argument)(new(0))));
    powc.Emit(Push((Argument)(new(1))));
    powc.Emit(Push(1));
    powc.Emit(Subtract(DataType.I32));
    powc.Emit(CallDirect(pow));
    powc.Emit(Multiply(DataType.I32));
    powc.Emit(Return());
    powc.Emit(Jump(inst17));

    powc.MoveLabel(inst15);
    powc.Emit(Push((Argument)(new(0))));
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
    malloc.SetReturn(DataType.Pointer);
    malloc.AddParameter(DataType.Pointer);


    var free = modBuilder.AddExtern("free");
    free.Library = "ucrtbase.dll";
    free.CallingConvention = Bridge.CallingConvention.Cdecl;
    free.SetReturn(DataType.Void);
    free.AddParameter(DataType.Pointer);

    var messagebox = modBuilder.AddExtern("MessageBoxA");
    messagebox.Library = "user32";
    messagebox.CallingConvention = Bridge.CallingConvention.StdCall;
    messagebox.SetReturn(DataType.I32);
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

    c.Emit(Push<nuint>(16));
    c.Emit(PushRoutine(malloc.ID));
    c.Emit(CallIndirect(new(malloc.ReturnType, malloc.Parameters.ToArray(), Bridge.CallingConvention.Cdecl)));
    c.Emit(Pop(ptr));

    c.Emit(Push(ptr));
    c.Emit(Cast(DataType.Pointer, DataType.U64));
    c.Emit(Print(DataType.Pointer));

    c.Emit(Push(ptr));
    c.Emit(If(ComparisonKind.Zero, DataType.Pointer));
    c.Emit(Jump(failure));

    c.Emit(Push(ptr));
    c.Emit(CallDirect(free));

    c.Emit(PushResource(successMessage));
    c.Emit(CallDirect(printstr));

    c.Emit(Jump(end));

    c.MoveLabel(failure);

    c.Emit(PushResource(failureMessage));
    c.Emit(CallDirect(printstr));

    c.MoveLabel(end);
    c.Emit(Return());

    printstr.AddParameter(DataType.Pointer);
    var pscode = printstr.GetCodeBuilder();
    var start = pscode.AddLabel();
    pscode.Emit(Push((Argument)(new(0))));
    pscode.Emit(Load(DataType.U16));

    pscode.Emit(If(ComparisonKind.Zero, DataType.U16));
    pscode.Emit(Return());

    pscode.Emit(Push((Argument)(new(0))));
    pscode.Emit(Load(DataType.U16));
    pscode.Emit(PrintChar(DataType.U16));

    pscode.Emit(Push((Argument)(new(0))));
    pscode.Emit(Push<nuint>(2));
    pscode.Emit(Add(DataType.Pointer));
    pscode.Emit(Pop((Argument)(new(0))));

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
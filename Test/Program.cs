using Bridge;
using System.Runtime.CompilerServices;
using System.Text;
using static Bridge.Instruction;

var modBuilder = Module.Create();

var main = modBuilder.AddRoutine("main");
var test = modBuilder.AddRoutine("test");
test.AddParameter(DataType.I32);

var maincode = main.GetCodeBuilder();
maincode.Emit(Push(TypedValue.Create(10)));
maincode.Emit(Call(test));
maincode.Emit(Return());

var testcode = test.GetCodeBuilder();
var loopcond = testcode.AddLabel();
var loopbody = testcode.AddLabel();

var l0 = testcode.AddLocal(DataType.I32);

testcode.Emit(Push(TypedValue.Create(0)));
testcode.Emit(Pop(l0));
testcode.Emit(Jump(loopcond));

testcode.MoveLabel(loopbody);
testcode.Emit(Push(l0));
testcode.Emit(Push(TypedValue.Create(1)));
testcode.Emit(Add(DataType.I32));
testcode.Emit(Pop(l0));

testcode.Emit(Push(l0));
testcode.Emit(Print(DataType.I32));

testcode.MoveLabel(loopcond);
testcode.Emit(Push(l0)); 
testcode.Emit(PushArg(0));
testcode.Emit(If(ComparisonKind.LessThan, DataType.I32));
testcode.Emit(Jump(loopbody));
testcode.Emit(Return());

void E()
{
    byte[] b = new byte[] { 10, 11, 12, 13, 14, 96 };
}

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

//var maincode = routine.GetCodeBuilder();
//maincode.Emit(Instruction.PushResource(modBuilder.AddResource("Hello, world!\0", Encoding.UTF8)));
//maincode.Emit(Instruction.Call(printstr));
//maincode.Emit(Instruction.Return());

//var pscode = printstr.GetCodeBuilder();
//var start = pscode.AddLabel();
//pscode.Emit(Instruction.PushArg(0));
//pscode.Emit(Instruction.Load(DataType.U8));

//pscode.Emit(Instruction.If(ComparisonKind.Zero, DataType.U8));
//pscode.Emit(Instruction.Return());

//pscode.Emit(Instruction.PushArg(0));
//pscode.Emit(Instruction.Load(DataType.U8));
//pscode.Emit(Instruction.PrintChar(DataType.U8));

//pscode.Emit(Instruction.PushArg(0));
//pscode.Emit(Instruction.Push(TypedValue.Create<nuint>(1)));
//pscode.Emit(Instruction.Add(DataType.Pointer));
//pscode.Emit(Instruction.PopArg(0));

//pscode.Emit(Instruction.Jump(start));

var mod = modBuilder.CreateModule();

Module.Dump(mod, Console.Out);

using var i = new Interpreter();
i.Run(mod);

var entry = Module.Compile(mod);
entry.Invoke(null, null);
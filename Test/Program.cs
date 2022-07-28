using Bridge;
using System.Runtime.CompilerServices;
using System.Text;

var modBuilder = Module.Create();

var routine = modBuilder.AddRoutine("main");
var printstr = modBuilder.AddRoutine("printstr");
printstr.AddParameter(DataType.Pointer);

var maincode = routine.GetCodeBuilder();
maincode.Emit(Instruction.PushResource(modBuilder.AddResource("Hello, world!\0", Encoding.UTF8)));
maincode.Emit(Instruction.Call(printstr));
maincode.Emit(Instruction.Return());

var pscode = printstr.GetCodeBuilder();
var start = pscode.AddLabel();
pscode.Emit(Instruction.PushArg(0));
pscode.Emit(Instruction.Load(DataType.U8));

pscode.Emit(Instruction.If(ComparisonKind.Zero, DataType.U8));
pscode.Emit(Instruction.Return());

pscode.Emit(Instruction.PushArg(0));
pscode.Emit(Instruction.Load(DataType.U8));
pscode.Emit(Instruction.PrintChar(DataType.U8));

pscode.Emit(Instruction.PushArg(0));
pscode.Emit(Instruction.Push(TypedValue.Create<nuint>(1)));
pscode.Emit(Instruction.Add(DataType.Pointer));
pscode.Emit(Instruction.PopArg(0));

pscode.Emit(Instruction.Jump(start));

var mod = modBuilder.CreateModule();

var interpreter = new Interpreter();
interpreter.Run(mod);
using Bridge;

var modBuilder = Module.Create();

var ex = modBuilder.AddExtern("qayd1bs1");
ex.Library = "wgh";
ex.ReturnType = DataType.I32;
ex.AddParameter(DataType.I32);
ex.AddParameter(DataType.I32);

var main = modBuilder.AddRoutine("main");

var code = main.GetCodeBuilder();
code.Emit(Instruction.Call(ex));
code.Emit(Instruction.Return());

var mod = modBuilder.CreateModule();

var interpreter = new Interpreter();
interpreter.Run(mod);
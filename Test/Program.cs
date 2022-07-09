using Bridge;

var mod = Module.Parse("test.br");
mod.Dump(Console.Out);

var i = new Interpreter();
i.RunModule(mod);a
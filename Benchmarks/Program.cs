using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Bridge;
using System.Reflection;
using Module = Bridge.Module;
using static Bridge.Instruction;
using System.Text;

BenchmarkRunner.Run<B>();

public class B
{
    Module module;
    MethodInfo compiled;
    Interpreter interpreter;

    [Params(100, 1000000)]
    public int Count;

    [BenchmarkDotNet.Attributes.GlobalSetup]
    public void Setup()
    {
        var modBuilder = Module.CreateBuilder();

        var main = modBuilder.AddRoutine("main");
        var test = modBuilder.AddRoutine("test");
        test.AddParameter(DataType.I32);

        var maincode = main.GetCodeBuilder();
        maincode.Emit(PushConst(TypedValue.Create(Count)));
        maincode.Emit(Call(test));
        maincode.Emit(Return());

        var testcode = test.GetCodeBuilder();
        var loopcond = testcode.AddLabel();
        var loopbody = testcode.AddLabel();

        var l0 = testcode.AddLocal(DataType.I32);

        testcode.Emit(PushConst(TypedValue.Create(0)));
        testcode.Emit(PopLocal(l0));
        testcode.Emit(Jump(loopcond));

        testcode.MoveLabel(loopbody);
        testcode.Emit(PushLocal(l0));
        testcode.Emit(PushConst(TypedValue.Create(1)));
        testcode.Emit(Add(DataType.I32));
        testcode.Emit(PopLocal(l0));

        testcode.Emit(PushLocal(l0));
        testcode.Emit(Print(DataType.I32));

        testcode.MoveLabel(loopcond);
        testcode.Emit(PushLocal(l0));
        testcode.Emit(PushArg(0));
        testcode.Emit(If(ComparisonKind.LessThan, DataType.I32));
        testcode.Emit(Jump(loopbody));
        testcode.Emit(Return());

        module = modBuilder.CreateModule();

        interpreter = new();
        compiled = Module.Compile(module);

        Console.SetOut(new X());
    }

    class X : TextWriter
    {
        public override Encoding Encoding { get => Encoding.Default; }
    }

    [Benchmark]
    public void Interpreted()
    {
        interpreter.Run(module);
    }

    [Benchmark]
    public void Compiled()
    {
        compiled.Invoke(null, null);
    }
}
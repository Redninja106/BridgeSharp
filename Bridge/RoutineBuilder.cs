using System.Linq;

namespace Bridge;
public sealed class RoutineBuilder : HeaderBuilder
{
    private readonly RoutineCodeBuilder codeBuilder;

    internal RoutineBuilder(ModuleBuilder parent, int id, string name) : base(parent, id, name)
    {
        this.ReturnType = DataType.Void;
        this.codeBuilder = new(id);
    }

    public CodeBuilder GetCodeBuilder()
    {
        return codeBuilder;
    }

    public RoutineDefinition CreateRoutine()
    {
        codeBuilder.Close();

        var locals = codeBuilder.GetLocals();
        var labels = codeBuilder.GetLabels();
        var instructions = codeBuilder.GetInstructions();
        
        var result = new RoutineDefinition(this.ID, this.Name, this.ReturnType, this.Parameters.ToArray(), locals.ToArray(), labels.ToArray(), instructions.ToArray());
        return result;
    }

    protected override void OnClose(ModuleBuilder moduleBuilder)
    {
        moduleBuilder.AddDefinition(this.CreateRoutine());
        base.OnClose(moduleBuilder);
    }

    class RoutineCodeBuilder : CodeBuilder
    {
        internal RoutineCodeBuilder(int blockID) : base(blockID)
        {
        }

        public override void Close()
        {
            // make sure all code paths return   

            base.Close();
        }
    }
}
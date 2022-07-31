namespace Bridge;

public record struct Local(ushort Value);
public record struct Label(int Value);

public class CodeBuilder : IBuilder
{
    private protected readonly List<Instruction> instructions = new();
    private protected readonly List<DataType> locals = new();
    private protected readonly List<int> labelLocations = new();
    private readonly int blockID;
    
    internal CodeBuilder(int blockID)
    {
        this.blockID = blockID;
    }

    public virtual void Emit(Instruction instruction)
    {
        instructions.Add(instruction);
    }
    
    public virtual Local AddLocal(DataType type)
    {
        if (type is DataType.Void)
            throw new ArgumentException("locals cannot be void!", nameof(type));

        if (locals.Count >= ushort.MaxValue)
            throw new ArgumentException("cannot have >255 locals!", nameof(type));

        var id = (ushort)locals.Count;
        locals.Add(type);
        return new Local(id);
    }

    public virtual Label AddLabel()
    {
        var index = instructions.Count;
        var labelId = labelLocations.Count;
        labelLocations.Add(index);
        return new Label(labelId);
    }

    // moves a label to the current location
    public void MoveLabel(Label label)
    {
        labelLocations[label.Value] = instructions.Count;
    }
    
    public virtual void Close()
    {
    }

    internal IEnumerable<Instruction> GetInstructions()
    {
        return this.instructions;
    }

    internal IEnumerable<DataType> GetLocals()
    {
        return this.locals;
    }

    internal IEnumerable<int> GetLabels()
    {
        return this.labelLocations;
    }
}
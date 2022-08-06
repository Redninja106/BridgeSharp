namespace Bridge;

public abstract class DefinitionBuilder : IBuilder
{
    private readonly ModuleBuilder parent;
    public string Name { get; }
    public int ID { get; }

    public ModuleBuilder ModuleBuilder => parent;
    public bool Closed { get; private set; }

    public DefinitionBuilder(ModuleBuilder parent, int id, string name)
    {
        this.parent = parent;
        this.Name = name;
        this.ID = id;
    }

    void IBuilder.Close()
    {
        parent.OnBuilderCompleted(this);
        OnClose(parent);
    }
    
    protected virtual void OnClose(ModuleBuilder moduleBuilder)
    {
        Closed = true;
    }
}

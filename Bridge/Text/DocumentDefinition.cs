namespace Bridge.Text;

internal abstract record DocumentDefinition()
{
    public abstract void Build(DocumentBuildContext context, ModuleBuilder builder);
}
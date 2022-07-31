namespace Bridge.Text;
internal class Parser
{
    private readonly ModuleBuilder moduleBuilder;

    public Parser(ModuleBuilder moduleBuilder)
    {
        this.moduleBuilder = moduleBuilder;
    }

    public void Parse(string source)
    {
        var tokens = Scanner.Scan(source);
    }
    
}
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
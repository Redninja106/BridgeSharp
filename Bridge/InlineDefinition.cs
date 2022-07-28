using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;
public record InlineDefinition(int ID, string Name) : Definition(ID, Name)
{
    
}

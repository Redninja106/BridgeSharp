using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;
public record ExternDefinition(int ID, string Name, string Library, DataType ReturnType, DataType[] Parameters) : Definition(ID, Name)
{
}
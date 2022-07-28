using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;

public sealed record RoutineDefinition( int ID,
                                        string Name, 
                                        DataType ReturnType, 
                                        DataType[] Parameters, 
                                        DataType[] Locals,
                                        int[] LabelLocations,
                                        Instruction[] Instructions ) : Definition(ID, Name);
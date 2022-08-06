namespace Bridge;

public sealed record RoutineDefinition( int ID,
                                        string Name, 
                                        DataType ReturnType, 
                                        DataType[] Parameters, 
                                        DataType[] Locals,
                                        int[] LabelLocations,
                                        Instruction[] Instructions ) : Definition(ID, Name);
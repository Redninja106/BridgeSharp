using System.Collections.Generic;

namespace Bridge;

internal class ModuleLinker
{
    public static Module Link(IEnumerable<Module> modules)
    {
        ModuleBuilder builder = Module.CreateBuilder(); 

        foreach (var mod in modules)
        {
            foreach (var def in mod.Definitions)
            {
                LinkDefinition(builder, def);
            }
        }

        return builder.CreateModule();
    }

    private static void LinkDefinition(ModuleBuilder builder, Definition definition)
    {
        switch (definition)
        {
            case RoutineDefinition routine:
                LinkRoutine(builder, routine);
                break;
            case ExternDefinition externDef:
                LinkExtern(builder, externDef);
                break;
            default:
                break;
        }
    }

    private static void LinkExtern(ModuleBuilder builder, ExternDefinition externDef)
    {
    }

    private static void LinkRoutine(ModuleBuilder builder, RoutineDefinition routine)
    {
        
    }
}

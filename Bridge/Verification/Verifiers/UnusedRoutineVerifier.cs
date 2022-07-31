using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Verification.Verifiers;
internal class UnusedRoutineVerifier : Verifier
{
    public UnusedRoutineVerifier()
    {
        OnVerify<RoutineDefinition>((context, routine) =>
        {
            if (routine.Name is "main")
                return; 

            foreach (var r in context.Module.Definitions.OfType<RoutineDefinition>())
            {
                if (r == routine)
                    continue;

                foreach (var instruction in r.Instructions)
                {
                    if (instruction is Instruction<CallMode, int> callInstruction)
                    {
                        if (callInstruction.Arg2 == routine.ID)
                            return;
                    }
                }
            }

            context.AddWarning("Routine is unused!");
        });
    }
}

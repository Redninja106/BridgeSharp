using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Verification.Verifiers;
internal class TailCallVerifier : Verifier
{
    public TailCallVerifier()
    {
        OnVerify<Instruction<CallMode>>((context, instruction) =>
        {
            if (instruction.Arg1 is CallMode.Tail)
            {
                var directInstruction = (Instruction<CallMode, int>)instruction;
                var routine = context.Module.FindDefinition(directInstruction.Arg2) as RoutineDefinition;

                if (routine is null)
                {
                    context.AddError("Tail call to non-routine definition");
                    return;
                }
                
                if (routine.ReturnType != context.CurrentRoutine.ReturnType)
                {
                    context.AddError("Tail call to routine with different return type");
                    return;
                }
            }
        });
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Verification.Verifiers;

internal sealed class LocalVerifier : Verifier
{
    public LocalVerifier()
    {
        // verify local referencing instructions
        OnVerify<Instruction<StackOpKind, Local>>((context, instruction) =>
        {
            if (instruction.Arg2.Value < 0 || instruction.Arg2.Value >= context.CurrentRoutine.Locals.Length)
            {
                context.AddError(ErrorMessages.InvalidLocal(instruction.Arg2));
            }
        });

        OnVerify<RoutineDefinition>((context, routine) =>
        {
            foreach (var local in routine.Locals)
            {
                if (local is DataType.Void || !Enum.IsDefined(local))
                {
                    context.AddError("local has invalid type: '" + local.ToString().ToLower() + "'");
                }
            }
        });
    }
}

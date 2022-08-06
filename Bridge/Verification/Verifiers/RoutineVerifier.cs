using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Verification.Verifiers;
internal sealed class RoutineVerifier : Verifier
{
    public RoutineVerifier()
    {
        OnVerify<RoutineDefinition>((context, routine) =>
        {
            VerifyName(context, routine);
            VerifyLabels(context, routine);
            VerifyParameters(context, routine);
            VerifyReturnType(context, routine);
            VerifyLocals(context, routine);
        });
    }

    private void VerifyParameters(VerificationContext context, RoutineDefinition routine)
    {
    }

    private void VerifyReturnType(VerificationContext context, RoutineDefinition routine)
    {
    }

    private void VerifyLocals(VerificationContext context, RoutineDefinition routine)
    {
        foreach (var local in routine.Locals)
        {
            
        }
    }

    private void VerifyLabels(VerificationContext context, RoutineDefinition routine)
    {
        foreach (var labelLocation in routine.LabelLocations)
        {
            if (labelLocation < 0 || labelLocation >= routine.Instructions.Length)
            {
                context.AddError($"Label has invalid location!");
            }
        }
    }

    private void VerifyName(VerificationContext context, RoutineDefinition routine)
    {
        if (string.IsNullOrEmpty(routine.Name))
            context.AddError("Routine name is empty");

        if (routine.Name.Any(c => !IsValidNameChar(c)))
            context.AddError("Routine name contains invalid characters");
    }

    private bool IsValidNameChar(char c)
    {
        return c is '$' or '#' or '_' || char.IsLetterOrDigit(c);
    }
}

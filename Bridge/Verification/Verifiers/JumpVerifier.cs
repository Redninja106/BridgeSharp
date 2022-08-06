using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Verification.Verifiers;
internal sealed class JumpVerifier : Verifier
{
    public JumpVerifier()
    {
        OnVerify<Instruction<Label>>((context, instruction) =>
        {
            if (context.CurrentRoutine is null)
                return;

            var label = instruction.Arg1;

            if (label.Value < 0 || label.Value >= context.CurrentRoutine.LabelLocations.Length)
            {
                context.AddError(ErrorMessages.InvalidLabel());
            }
        });
    }
}

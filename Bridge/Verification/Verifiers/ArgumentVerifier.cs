using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Verification.Verifiers;

/// <summary>
/// Verifies that a routine doesn't load arguments that don't exist
/// </summary>
internal sealed class ArgumentVerifier : Verifier
{
    public ArgumentVerifier()
    {
        OnVerify<Instruction<StackOpKind, byte>>((context, instruction) =>
        {
            if (context.CurrentRoutine is null)
                return;

            var argId = instruction.Arg2;

            if (argId < 0 || argId >= context.CurrentRoutine.Parameters.Length)
                context.AddError(ErrorMessages.InvalidArgID(argId));
        });
    }
}

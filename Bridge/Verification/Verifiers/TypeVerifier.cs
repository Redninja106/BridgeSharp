using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Verification.Verifiers;
internal class TypeVerifier : Verifier
{
    public TypeVerifier()
    {
        OnVerify<Instruction<DataType>>((context, instruction) =>
        {
            if (instruction.Arg1 is DataType.Void || !Enum.IsDefined(instruction.Arg1))
            {
                context.AddError(ErrorMessages.InvalidType(instruction.Arg1));
            }
        });
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Verification;

public sealed record VerificationMessage(MessageSeverity Severity, string Message, Definition Definition, int Instruction = -1)
{
    public override string ToString()
    {
        return $"{Severity.ToString().ToUpper()} ('{Definition.Name}'{(Instruction is -1 ? "" : ", instruction " + Instruction )}): {Message}";
    }
}
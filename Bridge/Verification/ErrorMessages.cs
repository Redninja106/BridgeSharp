using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Verification;
internal static class ErrorMessages
{
    public static string InvalidArgID(byte argId) => $"{nameof(InvalidArgID)}: An argument with index '{argId}' does not exist on this routine.";
    public static string InvalidLocal(Local local) => $"{nameof(InvalidLocal)}: A local with index '{local.Value}' does not exist on this routine.";
    public static string InvalidType(DataType type) => $"{nameof(InvalidType)}: Invalid type modifier '{type.ToString().ToLower()}' on instruction.";
    public static string InvalidLabel() => $"{nameof(InvalidLabel)}: The provided label does not exist.";
}

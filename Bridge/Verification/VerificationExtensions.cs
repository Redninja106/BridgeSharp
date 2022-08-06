using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Verification;

public static class VerificationExtensions
{
    public static bool HasErrors(this IEnumerable<VerificationMessage> messages)
    {
        return messages.Any(m => m.Severity is MessageSeverity.Error);
    }

    public static void PrintToConsole(this IEnumerable<VerificationMessage> messages)
    {
        var oldColor = Console.ForegroundColor;
        if (messages.Any())
        {
            foreach (var message in messages)
            {
                Console.ForegroundColor = message.Severity switch
                {
                    MessageSeverity.Error => ConsoleColor.Red,
                    MessageSeverity.Warning => ConsoleColor.Yellow,
                    _ => ConsoleColor.Gray,
                };

                Console.WriteLine(message.ToString());
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SUCCESS");
        }
        Console.ForegroundColor = oldColor;
    }
}

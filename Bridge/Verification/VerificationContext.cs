using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Verification;

internal sealed class VerificationContext
{
    public Module Module { get; private set; }
    public Definition CurrentDefinition { get; private set; }
    public RoutineDefinition CurrentRoutine => CurrentDefinition as RoutineDefinition;

    private int currentInstructionIndex;
    private Verifier currentVerifier;
    private readonly List<VerificationMessage> messages = new();
    
    public VerificationContext(Module module)
    {
        this.Module = module;
    }

    public void RunVerifier(Verifier verifier)
    {
        currentVerifier = verifier;

        verifier.Verify(this, Module);

        foreach (var def in Module.Definitions)
        {
            CurrentDefinition = def;

            verifier.Verify(this, def);
            
            if (def is RoutineDefinition routine)
            {
                for (int i = 0; i < routine.Instructions.Length; i++)
                {
                    var instruction = routine.Instructions[i];
                    currentInstructionIndex = i;
                    verifier.Verify(this, instruction);
                }
                currentInstructionIndex = -1;
            }
        }
    }

    public void AddError(string message)
    {
        messages.Add(new VerificationMessage(MessageSeverity.Error, message, CurrentDefinition, currentInstructionIndex));
    }

    public void AddWarning(string message)
    {
        messages.Add(new VerificationMessage(MessageSeverity.Warning, message, CurrentDefinition, currentInstructionIndex));
    }

    public VerificationMessage[] GetMessages()
    {
        return messages.ToArray();
    }
}

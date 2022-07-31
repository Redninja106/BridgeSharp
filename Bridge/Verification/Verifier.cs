using Bridge.Verification.Verifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Verification;

internal abstract class Verifier
{
    private readonly List<(Type, Delegate)> listeners = new();

    private static readonly Verifier[] verifiers = new Verifier[]
    {
        new ArgumentVerifier(),
        new LocalVerifier(),
        new TypeVerifier(),
        new JumpVerifier(),
        new RoutineVerifier(),
        new UnusedRoutineVerifier(),
        new TailCallVerifier(),
    };

    public static VerificationMessage[] Verify(Module module)
    {
        var context = new VerificationContext(module);

        foreach (var verifier in verifiers)
        {
            context.RunVerifier(verifier);
        }

        return context.GetMessages();
    }

    public void Verify(VerificationContext context, object obj)
    {
        var objType = obj.GetType();
        foreach (var (type, listener) in listeners)
        {
            if (objType.IsSubclassOf(type) || objType == type)
            {
                listener.DynamicInvoke(context, obj);
            }
        }
    }

    protected void OnVerify<T>(Action<VerificationContext, T> action)
    {
        listeners.Add((typeof(T), action));
    }
}

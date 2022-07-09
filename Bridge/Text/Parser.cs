using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Text;

internal sealed class ModuleParser
{
    Module result = Module.Create();

    public void AddSource(TokenReader reader)
    {
        if (result is null)
            throw new InvalidOperationException("Parser has already generated a module!");

        while (!reader.IsAtEnd)
        {
            ParseRoutine(reader);
        }
    }

    public Module GetResult()
    {
        if (result is null)
            throw new InvalidOperationException("Parser has already generated a module!");
        var r = result;
        result = null;
        return r;
    }
    
    private void ParseRoutine(TokenReader reader)
    {
        var defineKeyword = reader.Read(TokenKind.Define);
        var name = reader.Read(TokenKind.Identifier);
        var openBracket = reader.Read(TokenKind.OpenBracket);
        
        var routine = result.EmitRoutine(name.Value);

        while (reader.GetCurrent().Kind != TokenKind.CloseBracket)
        {
            var opIdentifier = reader.Read();
            var opCode = Enum.Parse<OpCode>(opIdentifier.Value, true);

            switch (opCode)
            {
                case OpCode.PushConst:
                    var constValueToken = reader.Read(TokenKind.NumericLiteral);
                    var constValue = long.Parse(constValueToken.Value);
                    routine.EmitPushConst(constValue);
                    break;
                case OpCode.Push:
                    ReadLocalInstructionArgs(routine, reader, out string local, out var accessMode);
                    routine.EmitPush(local, accessMode);
                    break;
                case OpCode.Pop:
                    ReadLocalInstructionArgs(routine, reader, out local, out accessMode);
                    routine.EmitPush(local, accessMode);
                    break;
                case OpCode.Call:
                    var callTarget = reader.Read(TokenKind.Identifier);
                    routine.EmitCall(callTarget.Value);
                    break;
                case OpCode.CallIf:
                    var callTargetIf = reader.Read(TokenKind.Identifier);
                    routine.EmitCallIf(callTargetIf.Value);
                    break;
                default:
                    routine.Emit(opCode);
                    break;
            }

        }

        var closeBracket = reader.Read(TokenKind.CloseBracket);
    }

    private static void ReadLocalInstructionArgs(CodeBuilder routine, TokenReader reader, out string local, out LocalAccessMode accessMode)
    {
        Token? accessModifier = reader.Read();
        if (accessModifier?.Kind is not (TokenKind.At or TokenKind.Dot))
        {
            reader.PutBack(accessModifier.Value);
            accessModifier = null;
        }

        var localIdentifier = reader.Read(TokenKind.Identifier);
        
        local = localIdentifier.Value;

        if (!routine.HasLocal(local))
            routine.EmitLocal(local);

        accessMode = AccessModeFromToken(accessModifier);
    }

    private static LocalAccessMode AccessModeFromToken(Token? token)
    {
        if (token?.Kind is TokenKind.At)
            return LocalAccessMode.Address;

        if (token?.Kind is TokenKind.Dot)
            return LocalAccessMode.Dereference;
        
        return LocalAccessMode.Normal;
    }
}

using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Text;

internal sealed record DocumentInstruction(Token<TokenKind> Opcode, Token<TokenKind>? Modifier, Token<TokenKind>? Type, Token<TokenKind>? Argument)
{
    public void Build(DocumentBuildContext context, CodeBuilder codeBuilder)
    {
        switch (Opcode.ToEnum<OpCode>())
        {
            case OpCode.Return:
            case OpCode.Nop:
                break;
            case OpCode.Push:
                break;
            case OpCode.Pop:
                break;
            case OpCode.Load:
                break;
            case OpCode.Store:
                break;
            case OpCode.Call:
                break;
                break;
            case OpCode.Jump:
                break;
            case OpCode.If:
                break;
            case OpCode.Cast:
                break;
            case OpCode.Add:
                break;
            case OpCode.Subtract:
                break;
            case OpCode.Multiply:
                break;
            case OpCode.Divide:
                break;
            case OpCode.Modulo:
                break;
            case OpCode.Negate:
                break;
            case OpCode.Increment:
                break;
            case OpCode.Decrement:
                break;
            case OpCode.And:
                break;
            case OpCode.Or:
                break;
            case OpCode.Xor:
                break;
            case OpCode.Not:
                break;
            case OpCode.Compare:
                break;
            case OpCode.Print:
                break;
            case OpCode.PrintChar:
                break;
            default:
                break;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Text;

internal sealed class TokenReader
{
    public bool IsAtEnd { get; private set; }
    
    private readonly IEnumerable<Token> source;
    private readonly IEnumerator<Token> enumerator;
    private Token? returned;

    public TokenReader(IEnumerable<Token> source)
    {
        this.source = source;
        enumerator = source.GetEnumerator();
        enumerator.MoveNext();
    }

    public Token GetCurrent()
    {
        if (returned is not null)
            return returned.Value;

        return enumerator.Current;
    }

    public Token Read()
    {
        if (IsAtEnd)
            return default;

        var result = GetCurrent();

        if (returned is not null)
        {
            returned = null;
        }
        else
        {
            IsAtEnd = !enumerator.MoveNext();
        }

        if (result.Kind is TokenKind.Comment)
            return Read();

        return result;
    }

    public Token Read(TokenKind kind)
    {
        var result = Read();

        if (result.Kind != kind)
            throw new Exception($"Expected token of kind {kind}, but got {result.Kind}");

        return result;
    }

    public void PutBack(Token token)
    {
        if (returned != null)
            throw new InvalidOperationException("Cannot return more than one token");

        returned = token;
    }
}

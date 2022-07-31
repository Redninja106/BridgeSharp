namespace Bridge.Text;

internal struct Token
{
    public string Value { get; private set; }
    public TokenKind Kind { get; private set; }

    public Token(string value, TokenKind kind)
    {
        Value = value;
        Kind = kind;
    }

    public Token(string value) : this(value, GetKindFromValue(value))
    {
    }

    private static TokenKind GetKindFromValue(string value)
    {
        TokenKind? kind = value switch
        {
            "define" => TokenKind.Define,
            "{" => TokenKind.OpenBracket,
            "}" => TokenKind.CloseBracket,
            "." => TokenKind.Dot,
            "@" => TokenKind.At,
            _ => null,
        };

        if (kind is not null)
            return (TokenKind)kind;

        if (Scanner.IsIdentifier(value))
            return TokenKind.Identifier;
        if (Scanner.IsNumericLiteral(value))
            return TokenKind.NumericLiteral;
        if (Scanner.IsComment(value))
            return TokenKind.Comment;

        return TokenKind.Unknown;
    }

    public override string ToString()
    {
        return $"{Value} (kind: {Kind})";
    }
}
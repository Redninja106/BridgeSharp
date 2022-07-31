namespace Bridge.Text;

internal enum TokenKind
{
    Unknown,
    Identifier,
    NumericLiteral,
    StringLiteral,

    Define,

    Comment,
    OpenBracket,
    CloseBracket,
    At,
    Dot
}
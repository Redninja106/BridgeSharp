namespace Bridge.Text;

internal enum TokenKind
{
    Unknown,
    Identifier,
    NumericLiteral,
    StringLiteral,
    DataType,
    ExternKeyword,
    RoutineKeyword,
    InlineKeyword,
    OpenBracket,
    CloseBracket,
    OpenParen,
    CloseParen,
    Comma,
    Dot,
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

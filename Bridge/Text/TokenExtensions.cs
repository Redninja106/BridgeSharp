using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Text;
internal static class TokenExtensions
{
    public static DataType? ToDataType(this Token<TokenKind> token)
    {
        if (token.Kind is not TokenKind.DataType)
            return null;

        if (token.ToStringValue() is "*")
            return DataType.Pointer;

        return Enum.Parse<DataType>(token.ToStringValue());
    }

    public static T? ToEnum<T>(this Token<TokenKind> token) where T : struct, Enum
    {
        return Enum.TryParse<T>(token.ToStringValue(), out var result) ? result : null;
    }
}
namespace Bridge.Text;
internal static class Scanner
{
    public static IEnumerable<Token> Scan(string source)
    {
        var reader = new StringReader(source);
        return Scan(reader);
    }

    public static IEnumerable<Token> Scan(TextReader source)
    {
        string token = string.Empty;
        int current = source.Read();
        while (current != -1)
        {
            char c = (char)current;

            if (token == "" && !char.IsWhiteSpace(c))
            {
                token += c;
            }
            else if (ContinuesToken(token, c))
            {
                token += c;
            }
            else if (!string.IsNullOrWhiteSpace(token))
            {
                yield return new Token(token);
                token = char.IsWhiteSpace(c) ? "" : c.ToString();
            }

            current = source.Read();
        }

        if (!string.IsNullOrWhiteSpace(token) && !IsComment(token))
            yield return new Token(token);
    }

    private static bool ContinuesToken(string token, char continuation)
    {
        if (IsIdentifier(token))
            return IsIdentifier(continuation.ToString()) || char.IsNumber(continuation);

        if (IsNumericLiteral(token))
            return char.IsDigit(continuation);

        // single line comment
        if (token == "/" && continuation == '/')
            return true;
        if (token.StartsWith("//"))
            return !token.EndsWith('\n');

        // multi line comment
        if (token == "/" && continuation == '*')
            return true;
        if (token.StartsWith("/*"))
            return !token.EndsWith("*/");

        return false;
    }

    public static bool IsIdentifier(string token)
    {
        if (token.Length is 0)
            return false;

        if (!char.IsLetter(token[0]) && token[0] != '_')
            return false;

        for (int i = 1; i < token.Length; i++)
        {
            if (!char.IsLetterOrDigit(token[i]) && token[0] != '_')
                return false;
        }

        return true;
    }

    public static bool IsNumericLiteral(string token)
    {
        if (token.Length is 0)
            return false;

        for (int i = 0; i < token.Length; i++)
        {
            if (!char.IsDigit(token[i]))
                return false;
        }

        return true;
    }

    public static bool IsComment(string token)
    {
        if (token.StartsWith("//"))
            return token.EndsWith('\n');

        if (token.StartsWith("/*"))
            return token.EndsWith("*/");

        return false;
    }

    public static bool IsKeyword(string token, out TokenKind keyword)
    {
        keyword = token switch
        {
            "define" => TokenKind.Define,
            _ => TokenKind.Unknown,
        };

        return keyword != TokenKind.Unknown;
    }
}

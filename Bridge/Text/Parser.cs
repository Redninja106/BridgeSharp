using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Bridge.Text;

internal class Parser
{
    private readonly ModuleBuilder moduleBuilder;

    private Tokenizer<TokenKind> tokenizer;

    private string[] keywords = new[]
    {
        "routine",
        "extern",
        "inline",
    };

    private string[] dataTypes = new[]
    {
        "i64",
        "i32",
        "i16",
        "i8",
        "u64",
        "u32",
        "u16",
        "u8",
        "f64",
        "f32",
    };

    private List<string> errors = new();

    public Parser(ModuleBuilder moduleBuilder)
    {
        TextParser<string> ParseString(string s)
        {
            return Character.Letter.Many().Where(c => c.AsSpan().SequenceEqual(s)).Select(c => s);
        }

        this.moduleBuilder = moduleBuilder;

        var parseKeyword = Character.Letter.Many().Where(s => keywords.Contains(new string(s))).Named("keyword");
        var parseDataType = Character.Letter.Many().Where(s => dataTypes.Contains(new string(s))).Named("data type");

        var tokenizerBuilder = new TokenizerBuilder<TokenKind>()
            .Ignore(Span.WhiteSpace)
            .Match(Character.EqualTo('.'), TokenKind.Dot)
            .Match(Character.EqualTo(','), TokenKind.Comma)
            .Match(Character.EqualTo('('), TokenKind.OpenParen)
            .Match(Character.EqualTo(')'), TokenKind.CloseParen)
            .Match(Character.EqualTo('{'), TokenKind.OpenBracket)
            .Match(Character.EqualTo('}'), TokenKind.CloseBracket)
            .Match(Character.EqualTo('*'), TokenKind.DataType)
            .Match(ParseString("routine"), TokenKind.RoutineKeyword, true)
            .Match(ParseString("extern"), TokenKind.ExternKeyword, true)
            .Match(ParseString("inline"), TokenKind.InlineKeyword, true)
            .Match(parseDataType, TokenKind.DataType, true)
            .Match(parseDataType, TokenKind.DataType, true)
            .Match(Span.Regex(@"""(?:[^""\\]|\\.)*"""), TokenKind.StringLiteral, true)
            .Match(Numerics.Natural, TokenKind.NumericLiteral, true)
            .Match(Span.Regex(@"[a-zA-Z_][\w\d_]*"), TokenKind.Identifier, true);

        tokenizer = tokenizerBuilder.Build();
    }

    private TokenList<TokenKind> GetTokens(Tokenizer<TokenKind> tokenizer, string source)
    {
        List<Token<TokenKind>> tokens = new();
        var result = tokenizer.TryTokenize(source);

        if (result.ErrorMessage is not null)
        {
            errors.Add(result.ErrorMessage);
        }

        if (result.HasValue)
        {
            tokens.AddRange(result.Value);
        }
        else
        {
            errors.Add(result.ToString());
        }

        return new(tokens.ToArray());
    }

    public bool TryParse(string source, out Document document, out string[] errors)
    {
        List<DocumentDefinition> definitions = new();

        var tokens = GetTokens(tokenizer, source);

        var result = parseDocument.TryParse(tokens);
        if (result.HasValue)
        {
            document = result.Value;
        }
        else
        {
            document = null;
            this.errors.Add(result.ToString());
        }

        errors = this.errors.ToArray();
        return document != null;
    }

    private T[] ParseWithErrors<T>(TokenListParser<TokenKind, T> parser, TokenList<TokenKind> tokens)
    {
        List<T> results = new();

        TokenListParserResult<TokenKind, T> result;
        do
        {
            result = parser.TryParse(tokens);

            if (result.ErrorMessage is not null)
            {
                errors.Add(result.ErrorMessage);
            }

            if (result.HasValue)
            {
                results.Add(result.Value);
                tokens = result.Remainder;
            }
            else
            {
                errors.Add(result.ToString());
                tokens = result.Remainder.ConsumeToken().Remainder;
            }
        }
        while (tokens.Any());

        return results.ToArray();
    }

    private TokenListParser<TokenKind, Document> parseDocument => parseDefinition.Many().AtEnd().Select(defs => new Document(defs));
    private TokenListParser<TokenKind, DocumentDefinition> parseDefinition => parseRoutine.Cast<TokenKind, DocumentRoutine, DocumentDefinition>();
    private TokenListParser<TokenKind, DocumentRoutine> parseRoutine =>
        from routineKeyword in Token.EqualTo(TokenKind.RoutineKeyword)
        from header in parseDefinitionHeader
        from instructions in parseInstruction.Many().Between(Token.EqualTo(TokenKind.OpenBracket), Token.EqualTo(TokenKind.CloseBracket))
        select new DocumentRoutine(header, instructions);

    private TokenListParser<TokenKind, DocumentDefinitionHeader> parseDefinitionHeader =>
        from returnType in Token.EqualTo(TokenKind.DataType).Optional()
        from name in Token.EqualTo(TokenKind.Identifier)
        from parameters in Token.EqualTo(TokenKind.DataType)
                            .ManyDelimitedBy(Token.EqualTo(TokenKind.Comma))
                            .Between(Token.EqualTo(TokenKind.OpenParen), Token.EqualTo(TokenKind.CloseParen))
        select new DocumentDefinitionHeader(returnType, name, parameters);

    private TokenListParser<TokenKind, DocumentInstruction> parseInstruction =>
        from opcode in Token.EqualTo(TokenKind.Identifier)
        from modifier in (
            from dot in Token.EqualTo(TokenKind.Dot)
            from modifier in Token.EqualTo(TokenKind.Identifier)
            select modifier
            ).Optional()
        from type in parseDataTypeModifier.Optional()
        from argument in Parse.OneOf(
            Token.EqualTo(TokenKind.NumericLiteral),
            Token.EqualTo(TokenKind.Identifier),
            Token.EqualTo(TokenKind.StringLiteral)
            ).Optional()
        select new DocumentInstruction(opcode, modifier, type, argument);

    private TokenListParser<TokenKind, Token<TokenKind>> parseDataTypeModifier => Token.Sequence(TokenKind.Dot, TokenKind.DataType).Select(k => k.First()).Or(Token.EqualToValue(TokenKind.DataType, "*"));
}
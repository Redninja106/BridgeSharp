using Superpower.Model;

namespace Bridge.Text;

internal sealed record DocumentDefinitionHeader(Token<TokenKind>? ReturnType, Token<TokenKind> Name, Token<TokenKind>[] Parameters)
{
    public void Build(DocumentBuildContext context, HeaderBuilder builder)
    {
        builder.SetReturn(this.ReturnType?.ToDataType() ?? DataType.Void);
        
        foreach (var parameter in this.Parameters)
        {
            builder.AddParameter(parameter.ToDataType().GetValueOrDefault());
        }
    }
}

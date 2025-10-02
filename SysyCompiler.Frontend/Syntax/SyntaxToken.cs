using System.Text.Json.Serialization;
using SysyCompiler.Frontend.Tokenization;

namespace SysyCompiler.Frontend.Syntax;

public class SyntaxToken : SyntaxNode
{
    [JsonIgnore]
    private readonly IToken token;

    [JsonIgnore]
    public TextSpan Span => token.Span;

    [SyntaxMember]
    public TokenKind TokenKind => token.Kind;

    [SyntaxMember]
    public string Text => token.Text;

    [SyntaxMember]
    public string? RawText => token.RawText;

    public SyntaxToken(IToken token)
    {
        this.token = token;
    }

    public override SyntaxKind Kind => SyntaxKind.SyntaxToken;
    public override IEnumerable<SyntaxToken> GetTokens() => new[] { this };
    public override SyntaxMember[] GetMembers() => new SyntaxMember[] {
        new(nameof(TokenKind), TokenKind),
        new(nameof(Text), Text),
    };
}
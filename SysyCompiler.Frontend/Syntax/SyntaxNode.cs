using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SysyCompiler.Frontend.Syntax;

public abstract class SyntaxNode
{
    public abstract SyntaxKind Kind { get; }

    public abstract SyntaxMember[] GetMembers();

    public virtual IEnumerable<SyntaxToken> GetTokens()
    {
        foreach (var member in GetMembers())
        {
            if (member.Value is SyntaxNode node)
            {
                foreach (var token in node.GetTokens())
                    yield return token;
            }
        }
    }

    public virtual T? Accept<T>(SyntaxVisitor<T> visitor, T? val = default)
    {
        return visitor.Visit(this, val);
    }

    private static readonly SyntaxJsonConverter jsonConverter = new();
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public override string ToString()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        jsonConverter.Write(writer, this, jsonOptions);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
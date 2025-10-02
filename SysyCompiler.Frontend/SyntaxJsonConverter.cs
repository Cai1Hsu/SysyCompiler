using System.Text.Json;
using System.Text.Json.Serialization;
using SysyCompiler.Frontend.Syntax;

namespace SysyCompiler.Frontend;

public class SyntaxJsonConverter : JsonConverter<SyntaxNode>
{
    public override SyntaxNode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    private static string GetNodeName(object value)
    {
        string typeName = value.GetType().Name;

        if (typeName.EndsWith("Syntax"))
            typeName = typeName[..^"Syntax".Length];

        return typeName;
    }

    public override void Write(Utf8JsonWriter writer, SyntaxNode value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("type", GetNodeName(value));

        foreach (var member in value.GetMembers())
        {
            if (member.Value is null)
                continue;

            writer.WritePropertyName(member.Name);

            switch (member.Value)
            {
                case SyntaxNode syntaxNode:
                    Write(writer, syntaxNode, options);
                    break;

                case IEnumerable<SyntaxNode> syntaxArray:
                    writer.WriteStartArray();

                    foreach (var node in syntaxArray)
                    {
                        Write(writer, node, options);
                    }

                    writer.WriteEndArray();

                    break;

                case string str:
                    // Escape special characters in the string
                    writer.WriteStringValue(str.Replace("\"", "\\\""));
                    break;

                case object obj when obj.GetType().IsEnum:
                    writer.WriteStringValue(obj.ToString());
                    break;

                default:
                    throw new NotSupportedException($"Unsupported member type: {member.Value.GetType().Name}");
            }
        }

        writer.WriteEndObject();
    }
}
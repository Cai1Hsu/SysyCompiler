using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;

namespace SysyCompiler.Frontend;

public interface ISyntaxParser
{
    ITokenSource Source { get; }

    SyntaxNode ParseDocument();
}

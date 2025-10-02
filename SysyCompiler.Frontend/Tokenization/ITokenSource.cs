namespace SysyCompiler.Frontend.Tokenization;

public interface ITokenSource : IEnumerable<IToken>
{
    public IToken? NextToken();
}

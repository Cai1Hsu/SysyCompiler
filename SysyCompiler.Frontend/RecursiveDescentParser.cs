using System.Collections.Immutable;
using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;

namespace SysyCompiler.Frontend;

public partial class RecursiveDescentParser : ISyntaxParser
{
    public IBufferedTokenSource Source { get; }

    ITokenSource ISyntaxParser.Source => Source;

    public RecursiveDescentParser(ITokenSource tokenSource)
    {
        if (tokenSource is not IBufferedTokenSource bufferedTokenSource)
        {
            Source = new QueuedTokenSource(tokenSource);
        }
        else
        {
            Source = bufferedTokenSource;
        }
    }

    private bool IsTokenKind(IToken token, params ReadOnlySpan<TokenKind> kind)
    {
        for (int i = 0; i < kind.Length; i++)
        {
            if (token.Kind == kind[i])
                return true;
        }

        return false;
    }

    public SyntaxToken? ParseNullableToken(params ReadOnlySpan<TokenKind> kind)
    {
        if (!kind.IsEmpty && !Source.IsMatch(0, kind))
            return null;

        IToken? token = Source.NextToken();

        if (token is null)
            return null;

        if (!kind.IsEmpty && !IsTokenKind(token, kind))
            return null;

        return new SyntaxToken(token);
    }

    public SyntaxToken ParseToken(params ReadOnlySpan<TokenKind> kind)
    {
        IToken? token = Source.NextToken();

        if (token is null ||
            (!kind.IsEmpty && !IsTokenKind(token, kind)))
        {
            // TODO: Report Error
        }

        return new SyntaxToken(token!);
    }

    public SyntaxNode ParseDocument()
    {
        return ParseCompilationUnit();
    }

    public CompilationUnitSyntax ParseCompilationUnit()
    {
        var members = ImmutableArray.CreateBuilder<MemberDeclarationSyntax>();

        while (Source.HasToken() && !Source.IsMatch(0, TokenKind.EndOfFile))
            members.Add(ParseMemberDeclaration());

        SyntaxToken endOfFile = ParseToken(TokenKind.EndOfFile);

        return new CompilationUnitSyntax(
            new SyntaxList<MemberDeclarationSyntax>(members.ToImmutable()),
            endOfFile
        );
    }

    public MemberDeclarationSyntax ParseMemberDeclaration()
    {
        // Try match Function Declaration
        // <type> <identifier> (
        if (Source.IsMatch(0, new[] {
            new[] { TokenKind.Identifier, TokenKind.Int, TokenKind.Char, TokenKind.Void },
            new[] { TokenKind.Identifier },
            new[] { TokenKind.LeftParen }
        }))
        {
            return ParseFunctionDeclaration();
        }

        return ParseVariableDeclaration();
    }

    public FunctionDeclarationSyntax ParseFunctionDeclaration()
    {
        TypeSyntax returnType = ParseType();
        SyntaxToken identifier = ParseToken(TokenKind.Identifier);
        SyntaxToken openParenToken = ParseToken(TokenKind.LeftParen);
        ParameterListSyntax? parameterList = ParseParameterList();
        SyntaxToken closeParenToken = ParseToken(TokenKind.RightParen);
        BlockSyntax body = ParseBlock();

        return new FunctionDeclarationSyntax(
            returnType,
            identifier,
            openParenToken,
            parameterList,
            closeParenToken,
            body
        );
    }

    public TypeSyntax ParseType()
    {
        SyntaxToken? typeToken = ParseNullableToken(
            TokenKind.Identifier, TokenKind.Int, TokenKind.Char, TokenKind.Void);

        return new TypeSyntax(typeToken!);
    }

    public ParameterListSyntax ParseParameterList()
    {
        var parameters = ImmutableArray.CreateBuilder<ParameterItemSyntax>();

        if (Source.HasToken() && !Source.IsMatch(0, TokenKind.RightParen))
        {
            do
            {
                parameters.Add(ParseParameterItem());
            }
            while (Source.HasToken() && parameters.Last().CommaToken is not null);
        }

        return new ParameterListSyntax(
            new SyntaxList<ParameterItemSyntax>(parameters.ToImmutable()));
    }

    public ParameterItemSyntax ParseParameterItem()
    {
        TypeSyntax type = ParseType();
        SyntaxToken identifier = ParseToken(TokenKind.Identifier);

        var arrayDimensions = ImmutableArray.CreateBuilder<ArrayDimensionSyntax>();

        while (Source.IsMatch(0, TokenKind.LeftBracket))
        {
            arrayDimensions.Add(ParseArrayDimension());
        }

        SyntaxToken? comma = ParseNullableToken(TokenKind.Comma);

        return new ParameterItemSyntax(
            type,
            identifier,
            new SyntaxList<ArrayDimensionSyntax>(arrayDimensions.ToImmutable()),
            comma
        );
    }

    public ArrayDimensionSyntax ParseArrayDimension()
    {
        SyntaxToken leftBracketToken = ParseToken(TokenKind.LeftBracket);

        ExpressionSyntax? expression = null;
        if (!Source.IsMatch(0, TokenKind.RightBracket))
            expression = ParseExpression();

        SyntaxToken rightBracketToken = ParseToken(TokenKind.RightBracket);

        return new ArrayDimensionSyntax(
            leftBracketToken,
            rightBracketToken,
            expression
        );
    }

    public VariableDeclarationSyntax ParseVariableDeclaration()
    {
        SyntaxToken? constToken = ParseNullableToken(TokenKind.Const);
        TypeSyntax type = ParseType();

        var defines = ImmutableArray.CreateBuilder<VariableDefineSyntax>();
        defines.Add(ParseVariableDefine());

        if (!Source.IsMatch(0, TokenKind.Semicolon))
        {
            do
            {
                defines.Add(ParseVariableDefine());
            }
            while (Source.HasToken() && defines.Last().CommaToken is not null);
        }

        SyntaxToken semicolonToken = ParseToken(TokenKind.Semicolon);

        var modifier = constToken is not null
            ? new ModifierSyntax(constToken)
            : null;

        if (defines.Count == 1)
        {
            return new VariableDeclarationSyntax(
                type,
                defines.Single(),
                semicolonToken,
                modifier
            );
        }

        return new VariableDeclarationSyntax(
            type,
            new SyntaxList<VariableDefineSyntax>(defines.ToImmutable()),
            semicolonToken,
            modifier
        );
    }

    public VariableDefineSyntax ParseVariableDefine()
    {
        SyntaxToken identifier = ParseToken(TokenKind.Identifier);

        var dimensions = ImmutableArray.CreateBuilder<ArrayDimensionSyntax>();
        while (Source.IsMatch(0, TokenKind.LeftBracket))
        {
            dimensions.Add(ParseArrayDimension());
        }

        SyntaxList<ArrayDimensionSyntax>? arrayDimensions = dimensions.Count == 0 ? null
            : new SyntaxList<ArrayDimensionSyntax>(dimensions.ToImmutable());

        SyntaxToken? assignToken = ParseNullableToken(TokenKind.AssignEqual);
        InitializerSyntax? initializer = null;

        if (assignToken is not null)
            initializer = ParseInitializer();

        SyntaxToken? commaToken = ParseNullableToken(TokenKind.Comma);

        return new VariableDefineSyntax(
            identifier,
            initializer,
            assignToken,
            arrayDimensions,
            commaToken
        );
    }

    public InitializerSyntax ParseInitializer()
    {
        if (Source.IsMatch(0, TokenKind.LeftBrace))
            return ParseArrayInitializer();

        return ParseExpressionInitializer();
    }

    public ArrayInitializerSyntax ParseArrayInitializer()
    {
        SyntaxToken openBraceToken = ParseToken(TokenKind.LeftBrace);

        var items = ImmutableArray.CreateBuilder<ArrayInitializerItemSyntax>();

        if (Source.HasToken() && !Source.IsMatch(0, TokenKind.RightBrace))
        {
            do
            {
                items.Add(ParseArrayInitializerItem());
            }
            while (Source.HasToken() && items.Last().CommaToken is not null);
        }

        SyntaxToken closeBraceToken = ParseToken(TokenKind.RightBrace);

        return new ArrayInitializerSyntax(
            openBraceToken,
            new SyntaxList<ArrayInitializerItemSyntax>(items.ToImmutable()),
            closeBraceToken
        );
    }

    public ArrayInitializerItemSyntax ParseArrayInitializerItem()
    {
        InitializerSyntax initializer = ParseInitializer();
        SyntaxToken? commaToken = ParseNullableToken(TokenKind.Comma);

        return new ArrayInitializerItemSyntax(initializer, commaToken);
    }

    public ExpressionInitializerSyntax ParseExpressionInitializer()
    {
        ExpressionSyntax expression = ParseExpression();
        return new ExpressionInitializerSyntax(expression);
    }

    public ArgumentListSyntax ParseArgumentList()
    {
        var arguments = ImmutableArray.CreateBuilder<ArgumentItemSyntax>();

        if (Source.HasToken() && !Source.IsMatch(0, TokenKind.RightParen))
        {
            do
            {
                arguments.Add(ParseArgumentItem());
            }
            while (Source.HasToken() && arguments.Last().CommaToken is not null);
        }

        return new ArgumentListSyntax(new SyntaxList<ArgumentItemSyntax>(arguments.ToImmutable()));
    }

    public ArgumentItemSyntax ParseArgumentItem()
    {
        ExpressionSyntax expression = ParseExpression();
        SyntaxToken? commaToken = ParseNullableToken(TokenKind.Comma);

        return new ArgumentItemSyntax(expression, commaToken);
    }

    public BlockSyntax ParseBlock()
    {
        SyntaxToken openBraceToken = ParseToken(TokenKind.LeftBrace);

        var statements = ImmutableArray.CreateBuilder<StatementSyntax>();

        while (Source.HasToken() && !Source.IsMatch(0, TokenKind.RightBrace))
            statements.Add(ParseStatement());

        SyntaxList<StatementSyntax> statementList = new SyntaxList<StatementSyntax>(statements.ToImmutable());

        SyntaxToken closeBraceToken = ParseToken(TokenKind.RightBrace);

        return new BlockSyntax(openBraceToken, statementList, closeBraceToken);
    }

    public BlockStatementSyntax ParseBlockStatement()
    {
        BlockSyntax block = ParseBlock();
        return new BlockStatementSyntax(block);
    }

    public IfStatementSyntax ParseIfStatement()
    {
        SyntaxToken ifToken = ParseToken(TokenKind.If);
        SyntaxToken openParenToken = ParseToken(TokenKind.LeftParen);
        ExpressionSyntax condition = ParseExpression();
        SyntaxToken closeParenToken = ParseToken(TokenKind.RightParen);
        StatementSyntax thenStatement = ParseStatement();

        SyntaxToken? elseToken = ParseNullableToken(TokenKind.Else);
        StatementSyntax? elseStatement = null;

        if (elseToken is not null)
            elseStatement = ParseStatement();

        return new IfStatementSyntax(
            ifToken,
            openParenToken,
            condition,
            closeParenToken,
            thenStatement,
            elseToken,
            elseStatement
        );
    }

    public ReturnStatementSyntax ParseReturnStatement()
    {
        SyntaxToken returnToken = ParseToken(TokenKind.Return);

        ExpressionSyntax? expression = null;
        if (!Source.IsMatch(0, TokenKind.Semicolon))
            expression = ParseExpression();

        SyntaxToken? semicolonToken = ParseNullableToken(TokenKind.Semicolon);

        return new ReturnStatementSyntax(
            returnToken,
            expression,
            semicolonToken
        );
    }

    public WhileStatementSyntax ParseWhileStatement()
    {
        SyntaxToken whileToken = ParseToken(TokenKind.While);
        SyntaxToken openParenToken = ParseToken(TokenKind.LeftParen);
        ExpressionSyntax condition = ParseExpression();
        SyntaxToken closeParenToken = ParseToken(TokenKind.RightParen);
        StatementSyntax body = ParseStatement();

        return new WhileStatementSyntax(
            whileToken,
            openParenToken,
            condition,
            closeParenToken,
            body
        );
    }

    public EmptyStatementSyntax ParseEmptyStatement()
    {
        SyntaxToken semicolonToken = ParseToken(TokenKind.Semicolon);
        return new EmptyStatementSyntax(semicolonToken);
    }

    public LocalDeclarationStatementSyntax ParseLocalDeclarationStatement()
    {
        VariableDeclarationSyntax declaration = ParseVariableDeclaration();
        return new LocalDeclarationStatementSyntax(declaration);
    }

    public ExpressionStatementSyntax ParseExpressionStatement()
    {
        ExpressionSyntax expression = ParseExpression();
        SyntaxToken? semicolonToken = ParseNullableToken(TokenKind.Semicolon);

        return new ExpressionStatementSyntax(expression, semicolonToken);
    }

    public StatementSyntax ParseStatement()
    {
        bool IsStartOfLocalDeclaration()
        {
            int lookahead = 0;

            if (Source.IsMatch(lookahead, TokenKind.Const))
                lookahead++;

            if (!Source.IsMatch(lookahead, TokenKind.Int, TokenKind.Char, TokenKind.Void, TokenKind.Identifier))
                return false;

            return Source.IsMatch(lookahead + 1, TokenKind.Identifier);
        }

        if (Source.IsMatch(0, TokenKind.Semicolon))
            return ParseEmptyStatement();

        if (Source.IsMatch(0, TokenKind.LeftBrace))
            return ParseBlockStatement();

        if (Source.IsMatch(0, TokenKind.If))
            return ParseIfStatement();

        if (Source.IsMatch(0, TokenKind.Return))
            return ParseReturnStatement();

        if (Source.IsMatch(0, TokenKind.Break, TokenKind.Continue))
            return ParseControlFlowStatement();

        if (Source.IsMatch(0, TokenKind.While))
            return ParseWhileStatement();

        if (IsStartOfLocalDeclaration())
            return ParseLocalDeclarationStatement();

        return ParseExpressionStatement();
    }

    public ControlFlowStatementSyntax ParseControlFlowStatement()
    {
        SyntaxToken controlFlowToken = ParseToken(TokenKind.Break, TokenKind.Continue);
        SyntaxToken? semicolonToken = ParseNullableToken(TokenKind.Semicolon);

        return new ControlFlowStatementSyntax(controlFlowToken, semicolonToken);
    }
}

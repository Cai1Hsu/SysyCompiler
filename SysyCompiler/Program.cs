using System.Text;
using SysyCompiler.Frontend;
using SysyCompiler.Frontend.Syntax;
using SysyCompiler.Frontend.Tokenization;
using SysyCompiler.Analyzer;

string sourceCode = File.ReadAllText("testfile.txt");

Lexer lexer = new Lexer(sourceCode);
ISyntaxParser parser = new RecursiveDescentParser(lexer);

SyntaxNode root = parser.ParseDocument();

OnlineJudgeAdapter adapter = new OnlineJudgeAdapter();

OnlineJudgeOutput? result = root.Accept(adapter, new());

string output = string.Join("\n", result?.StringLines() ?? []);
File.WriteAllText("output.txt", output);

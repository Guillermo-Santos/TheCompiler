using SparkCore;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.IO;
using SparkCore.IO.Text;

namespace spi.Replies;

internal sealed class SparkRepl : Repl
{
    private Compilation? _previous;
    private bool _showTree;
    private bool _showProgram;
    private readonly Dictionary<VariableSymbol, object> _variables = new();


    protected override void RenderLine(string line)
    {
        var tokens = SyntaxTree.ParseTokens(line);
        foreach (var token in tokens)
        {
            switch (token.Kind)
            {
                case SyntaxKind.IdentifierToken:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case SyntaxKind.NumberToken:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case SyntaxKind.StringToken:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                default:
                    if (token.Kind.ToString().EndsWith("Keyword"))
                        Console.ForegroundColor = ConsoleColor.Blue;
                    else
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }

            Console.Write(token.Text);
            Console.ResetColor();
        }
    }
    protected override void EvaluateMetaCommand(string input)
    {
        switch (input)
        {
            case "#showTree":
                _showTree = !_showTree;
                Console.WriteLine(_showTree ? "Showing parse trees." : "Not showing parse trees");
                break;
            case "#showProgram":
                _showProgram = !_showProgram;
                Console.WriteLine(_showProgram ? "Showing bound trees." : "Not showing bound trees");
                break;
            case "#cls":
                Console.Clear();
                break;
            case "#reset":
                _previous = null;
                break;
            default:
                base.EvaluateMetaCommand(input);
                break;
        }
    }
    protected override bool IsCompletedSubmission(string text)
    {
        if (string.IsNullOrEmpty(text))
            return true;

        var lastTwoLinesAreBlank = text.Split(Environment.NewLine)
                                       .Reverse()
                                       .TakeWhile(s => string.IsNullOrEmpty(s))
                                       .Take(2)
                                       .Count() == 2;
        if (lastTwoLinesAreBlank)
            return true;
        var syntaxTree = SyntaxTree.Parse(text);

        if (syntaxTree.Root.Members.Last().GetLastToken().IsMissing)
            return false;

        return true;
    }
    protected override void EvaluateSubmission(string text)
    {
        var syntaxTree = SyntaxTree.Parse(text);

        var compilation = _previous == null
                            ? new Compilation(syntaxTree)
                            : _previous.ContinueWith(syntaxTree);


        if (_showTree)
            syntaxTree.Root.WriteTo(Console.Out);

        if (_showProgram)
            compilation.EmitTree(Console.Out);

        var result = compilation.Evaluate(_variables);

        if (!result.Diagnostics.Any())
        {
            if (result.Value != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(result.Value);
                Console.ResetColor();
            }
            _previous = compilation;
        }
        else
        {
            Console.Out.WriteDiagnostics(result.Diagnostics, syntaxTree);
        }
    }
}

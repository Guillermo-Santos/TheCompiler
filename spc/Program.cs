using SparkCore;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax.Tree;
namespace spc;

internal static class Program
{
    private static void Main(string[] args)
    {
        if(args.Length == 0)
        {
            Console.Error.WriteLine("usage: spc <source-paths> ");
            return;
        }
        if(args.Length > 1)
        {
            Console.WriteLine("error: only one path supported right now");
            return;
        }

        var path = args.Single();
        var text = File.ReadAllText(path);

        var syntaxTree = SyntaxTree.Parse(text);

        var compilation = new Compilation(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

        Console.WriteLine("Hello, World!");
    }
}
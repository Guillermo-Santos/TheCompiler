using Compiler.Core.Analyzers;
using System.Collections.Generic;
using System.Linq;
namespace Compiler.Core.Syntax
{
    public sealed class SyntaxTree
    {
        public SyntaxTree(IEnumerable<string> diagnostics, SyntaxExpression root, SyntaxToken endOfFile)
        {
            Diagnostics = diagnostics.ToArray();
            Root = root;
            EndOfFile = endOfFile;
        }

        public IReadOnlyList<string> Diagnostics { get; }
        public SyntaxExpression Root { get; }
        public SyntaxToken EndOfFile { get; }

        public static SyntaxTree Parse(string text)
        {
            var parser = new SyntaxAnalyzer(text);
            return parser.Parse();
        }
    }
}

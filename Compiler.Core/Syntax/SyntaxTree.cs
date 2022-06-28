using Compiler.Core.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Core.Syntax
{
    public sealed class SyntaxTree
    {
        public SyntaxTree(IEnumerable<Diagnostic> diagnostics, SyntaxExpression root, SyntaxToken endOfFile)
        {
            Diagnostics = diagnostics.ToArray();
            Root = root;
            EndOfFile = endOfFile;
        }

        public IReadOnlyList<Diagnostic> Diagnostics { get; }
        public SyntaxExpression Root { get; }
        public SyntaxToken EndOfFile { get; }

        public static SyntaxTree Parse(string text)
        {
            var parser = new SyntaxAnalyzer(text);
            return parser.Parse();
        }
    }
}

using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Syntax.Lexic;
using SparkCore.Analytics.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SparkCore.Analytics.Syntax.Tree
{
    public sealed class SyntaxTree
    {

        private SyntaxTree(SourceText text)
        {
            var parser = new Parser(text);
            var root = parser.ParseCompilationUnit();
            var diagnostics = parser.Diagnostics.ToImmutableArray();
            Text = text;
            Diagnostics = diagnostics;
            Root = root;
        }

        public SourceText Text { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public CompilationSyntaxUnit Root { get; }

        public static SyntaxTree Parse(string text)
        {
            var sourceText = SourceText.From(text);
            return Parse(sourceText);
        }
        public static SyntaxTree Parse(SourceText text)
        {
            return new SyntaxTree(text);
        }
        public static IEnumerable<SyntaxToken> ParseTokens(string text)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText);
        }
        public static IEnumerable<SyntaxToken> ParseTokens(SourceText text)
        {
            var lexic = new LexicAnalyzer(text);
            while (true)
            {
                var token = lexic.Lex();
                if (token.Type == SyntaxType.EndOfFileToken)
                    break;
                yield return token;
            }
        }
    }
}

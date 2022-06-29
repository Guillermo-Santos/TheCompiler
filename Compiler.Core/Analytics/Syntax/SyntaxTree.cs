using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Syntax.Lexic;
using SparkCore.Analytics.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SparkCore.Analytics.Syntax
{
    public sealed class SyntaxTree
    {
        public SyntaxTree(SourceText text,ImmutableArray<Diagnostic> diagnostics, SyntaxExpression root, SyntaxToken endOfFile)
        {
            Text = text;
            Diagnostics = diagnostics;
            Root = root;
            EndOfFile = endOfFile;
        }

        public SourceText Text { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public SyntaxExpression Root { get; }
        public SyntaxToken EndOfFile { get; }

        public static SyntaxTree Parse(string text)
        {
            var sourceText = SourceText.From(text);
            return Parse(sourceText);
        }
        public static SyntaxTree Parse(SourceText text)
        {
            var parser = new Parser(text);
            return parser.Parse();
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

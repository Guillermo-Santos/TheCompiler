using SparkCore.Analytics.Syntax.Tree.Statements;

namespace SparkCore.Analytics.Syntax.Tree
{
    public sealed class CompilationSyntaxUnit : SyntaxNode
    {
        public CompilationSyntaxUnit(SyntaxStatement statement, SyntaxToken endOfFileToken)
        {
            Statement = statement;
            EndOfFileToken = endOfFileToken;
        }
        public override SyntaxType Type => SyntaxType.CompilationUnit;
        public SyntaxStatement Statement { get; }
        public SyntaxToken EndOfFileToken { get; }

    }
}

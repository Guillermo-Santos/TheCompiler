using System.Collections.Immutable;

namespace SparkCore.Analytics.Syntax.Tree.Nodes;

public sealed partial class CompilationUnitSyntax : SyntaxNode
{
    public CompilationUnitSyntax(SyntaxTree syntaxTree, ImmutableArray<MemberSyntax> members, SyntaxToken endOfFileToken) : base(syntaxTree)
    {
        Members = members;
        EndOfFileToken = endOfFileToken;
    }
    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    public ImmutableArray<MemberSyntax> Members
    {
        get;
    }
    public SyntaxToken EndOfFileToken
    {
        get;
    }
}

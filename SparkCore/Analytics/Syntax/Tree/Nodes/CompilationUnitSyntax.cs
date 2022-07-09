using System.Collections.Immutable;

namespace SparkCore.Analytics.Syntax.Tree.Nodes;

public sealed class CompilationUnitSyntax : SyntaxNode
{
    public CompilationUnitSyntax(ImmutableArray<MemberSyntax> members, SyntaxToken endOfFileToken)
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

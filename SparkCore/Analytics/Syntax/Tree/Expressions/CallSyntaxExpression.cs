namespace SparkCore.Analytics.Syntax.Tree.Expressions;

public sealed class CallSyntaxExpression : SyntaxExpression
{
    public CallSyntaxExpression(SyntaxToken identifier, SyntaxToken openParentesis, SeparatedSyntaxList<SyntaxExpression> arguments, SyntaxToken closeParentesis)
    {
        Identifier = identifier;
        OpenParentesis = openParentesis;
        Arguments = arguments;
        CloseParentesis = closeParentesis;
    }
    public override SyntaxKind Kind => SyntaxKind.CallExpression;

    public SyntaxToken Identifier
    {
        get;
    }
    public SyntaxToken OpenParentesis
    {
        get;
    }
    public SeparatedSyntaxList<SyntaxExpression> Arguments
    {
        get;
    }
    public SyntaxToken CloseParentesis
    {
        get;
    }
}

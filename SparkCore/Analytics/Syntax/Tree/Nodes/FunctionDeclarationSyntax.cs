using SparkCore.Analytics.Syntax.Tree.Statements;

namespace SparkCore.Analytics.Syntax.Tree.Nodes;

public sealed partial class FunctionDeclarationSyntax : MemberSyntax
{

    public FunctionDeclarationSyntax(
                                     SyntaxTree syntaxTree, 
                                     SyntaxToken functionKeyword, 
                                     SyntaxToken identifier, 
                                     SyntaxToken openParentesisToken, 
                                     SeparatedSyntaxList<ParameterSyntax> parameters, 
                                     SyntaxToken closeParentesisToken, 
                                     TypeClauseSyntax type, 
                                     BlockStatementSyntax body
                                    ) : base(syntaxTree)
    {
        FunctionKeyword = functionKeyword;
        Identifier = identifier;
        OpenParenthesisToken = openParentesisToken;
        Parameters = parameters;
        CloseParethesisToken = closeParentesisToken;
        Type = type;
        Body = body;
    }

    public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;

    public SyntaxToken FunctionKeyword
    {
        get;
    }
    public SyntaxToken Identifier
    {
        get;
    }
    public SyntaxToken OpenParenthesisToken
    {
        get;
    }
    public SeparatedSyntaxList<ParameterSyntax> Parameters
    {
        get;
    }
    public SyntaxToken CloseParethesisToken
    {
        get;
    }
    public TypeClauseSyntax Type
    {
        get;
    }
    public BlockStatementSyntax Body
    {
        get;
    }
}
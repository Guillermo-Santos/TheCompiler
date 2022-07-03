namespace SparkCore.Analytics.Syntax
{
    public enum SyntaxKind
    {
        // TOKENS
        BadToken,
        EndOfFileToken,
        WhiteSpaceToken,
        NumberToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        OpenParentesisToken,
        CloseParentesisToken,
        OpenBraceToken,
        CloseBraceToken,
        IdentifierToken,
        BangToken,
        EqualsToken,
        TildeToken,
        HatToken,
        AmpersandToken,
        AmpersandAmpersandToken,
        PibeToken,
        PibePibeToken,
        BangEqualsToken,
        EqualsEqualsToken,
        LessToken,
        LessOrEqualsToken,
        GreaterToken,
        GreaterOrEqualsToken,
        //KEYWORDS
        ElseKeyword,
        FalseKeyword,
        ForKeyword,
        IfKeyword,
        ToKeyword,
        TrueKeyword,
        LetKeyword,
        VarKeyword,
        WhileKeyword,

        //NODES
        CompilationUnit,
        ElseClause,

        //STATEMENTS
        BlockStatement,
        ExpressionStatement,
        VariableDeclarationStatement,
        IfStatement,
        WhileStatement,
        ForStatement,

        // EXPRESSIONS
        LiteralExpression,
        NameExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        AssignmentExpression,
    }
}

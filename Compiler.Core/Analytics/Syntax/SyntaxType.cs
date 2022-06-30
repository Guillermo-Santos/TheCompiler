namespace SparkCore.Analytics.Syntax
{
    public enum SyntaxType
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
        AmpersandAmpersandToken,
        PibePibeToken,
        BangEqualsToken,
        EqualsEqualsToken,
        //KEYWORDS
        FalseKeyword,
        TrueKeyword,
        LetKeyword,
        VarKeyword,

        //NODES
        CompilationUnit,

        //STATEMENTS
        BlockStatement,
        ExpressionStatement,
        VariableDeclarationStatement,

        // EXPRESSIONS
        LiteralExpression,
        NameExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        AssignmentExpression,
    }
}

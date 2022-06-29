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
        IdentifierToken,
        BangToken,
        EqualsToken,
        AmpersandAmpersandToken,
        PibePibeToken,
        BangEqualsToken,
        EqualsEqualsToken,
        //KEYWORDS
        TrueKeyword,
        FalseKeyword,
        // EXPRESSIONS
        LiteralExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        NameExpression,
        AssignmentExpression,
    }
}

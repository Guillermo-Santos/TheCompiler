namespace Compiler.Core.Syntax
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
        IdentifierTolen,
        BangToken,
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
    }
}

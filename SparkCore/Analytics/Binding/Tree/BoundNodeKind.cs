namespace SparkCore.Analytics.Binding.Scope
{
    internal enum BoundNodeKind
    {
        //STATEMENTS
        BlockStatement,
        ExpressionStatement,
        ForStatement,
        GotoStatement,
        ConditionalGotoStatement,
        IfStatement,
        LabelStatement,
        VariableDeclaration,
        WhileStatement,

        //EXPRESSIONS
        UnaryExpression,
        LiteralExpression,
        BinaryExpression,
        VariableExpression,
        AssignmentExpression,
    }

}

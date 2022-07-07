namespace SparkCore.Analytics.Binding.Tree
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
        ErrorExpression,
        CallExpression,
    }

}

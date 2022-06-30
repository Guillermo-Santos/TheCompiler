namespace SparkCore.Analytics.Binding.Scope
{
    internal enum BoundNodeType
    {
        //STATEMENTS
        BlockStatement,
        ExpressionStatement,
        VariableDeclaration,

        //EXPRESSIONS
        UnaryExpression,
        LiteralExpression,
        BinaryExpression,
        VariableExpression,
        AssignmentExpression,
    }

}

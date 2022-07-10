namespace SparkCore.Analytics.Binding.Tree;

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
    DoWhileStatement,
    ReturnStatement,

    //EXPRESSIONS
    UnaryExpression,
    LiteralExpression,
    BinaryExpression,
    VariableExpression,
    AssignmentExpression,
    ErrorExpression,
    CallExpression,
    ConversionExpression,
}


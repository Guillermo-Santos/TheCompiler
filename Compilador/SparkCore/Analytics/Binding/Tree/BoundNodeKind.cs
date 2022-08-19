namespace SparkCore.Analytics.Binding.Tree;

internal enum BoundNodeKind
{
    //STATEMENTS
    BlockStatement,
    VariableDeclaration,
    IfStatement,
    WhileStatement,
    DoWhileStatement,
    ForStatement,
    LabelStatement,
    GotoStatement,
    ConditionalGotoStatement,
    ReturnStatement,
    NopStatement,
    ExpressionStatement,

    //EXPRESSIONS
    ErrorExpression,
    LiteralExpression,
    VariableExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    CallExpression,
    ConversionExpression,
}


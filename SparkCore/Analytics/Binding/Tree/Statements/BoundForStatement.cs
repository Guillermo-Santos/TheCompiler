using SparkCore.Analytics.Binding.Tree.Expressions;
using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Binding.Tree.Statements;

internal sealed class BoundForStatement : BoundLoopStatement
{

    public BoundForStatement(VariableSymbol variable, BoundExpression lowerBound, BoundExpression upperBound, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel)
        : base(breakLabel, continueLabel)
    {
        Variable = variable;
        LowerBound = lowerBound;
        UpperBound = upperBound;
        Body = body;
    }
    public override BoundNodeKind Kind => BoundNodeKind.ForStatement;
    public VariableSymbol Variable
    {
        get;
    }
    public BoundExpression LowerBound
    {
        get;
    }
    public BoundExpression UpperBound
    {
        get;
    }
    public BoundStatement Body
    {
        get;
    }
}

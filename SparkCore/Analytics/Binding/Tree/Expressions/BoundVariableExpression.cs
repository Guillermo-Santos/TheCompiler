using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Binding.Tree.Expressions;

internal sealed class BoundVariableExpression : BoundExpression
{
    public BoundVariableExpression(VariableSymbol variable)
    {
        Variable = variable;
    }

    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;

    public VariableSymbol Variable
    {
        get;
    }
    public override TypeSymbol Type => Variable.Type;
    public override BoundConstant? ConstantValue => Variable.Constant;
}


using System;

namespace Compiler.Core.Binding.Expressions
{
    internal sealed class BoundAssigmentExpression : BoundExpression
    {
        public BoundAssigmentExpression(VariableSymbol variable, BoundExpression expression)
        {
            Variable = variable;
            Expression = expression;
        }
        public override BoundNodeType NodeType => BoundNodeType.AssignmentExpression;
        public override Type Type => Expression.Type;
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }

}

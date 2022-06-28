using System;

namespace Compiler.Core.Binding.Expressions
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression(VariableSymbol variable)
        {
            Variable = variable;
        }

        public override BoundNodeType NodeType => BoundNodeType.VariableExpression;

        public VariableSymbol Variable { get; }
        public override Type Type => Variable.Type;

    }

}

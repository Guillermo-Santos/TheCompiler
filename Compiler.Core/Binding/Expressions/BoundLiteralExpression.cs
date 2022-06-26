using System;

namespace Compiler.Core.Binding.Expressions
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object value)
        {
            Value = value;
        }

        public override BoundNodeType NodeType => BoundNodeType.LiteralExpression;
        public override Type Type => Value.GetType();
        public object Value { get; }
    }

}

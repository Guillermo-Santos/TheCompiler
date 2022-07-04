using SparkCore.Analytics.Binding.Tree.Expressions;
using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Binding.Tree.Statements
{
    internal sealed class BoundVariableDeclaration : BoundStatement
    {
        public BoundVariableDeclaration(VariableSymbol variable, BoundExpression initializer)
        {
            Variable = variable;
            Initializer = initializer;
        }

        public VariableSymbol Variable
        {
            get;
        }
        public BoundExpression Initializer
        {
            get;
        }

        public override BoundNodeKind Kind => BoundNodeKind.VariableDeclaration;
    }
}

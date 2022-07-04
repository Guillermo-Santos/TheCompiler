using SparkCore.Analytics.Binding.Scope.Expressions;

namespace SparkCore.Analytics.Binding.Scope.Statements
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

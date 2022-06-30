using SparkCore.Analytics.Binding.Scope.Expressions;

namespace SparkCore.Analytics.Binding.Scope.Statements
{
    internal sealed class BoundVariableDeclarationStatement : BoundStatement
    {
        public BoundVariableDeclarationStatement(VariableSymbol variable, BoundExpression initializer)
        {
            Variable = variable;
            Initializer = initializer;
        }

        public VariableSymbol Variable { get; }
        public BoundExpression Initializer { get; }

        public override BoundNodeType NodeType => BoundNodeType.VariableDeclaration;
    }
}

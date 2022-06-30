using System.Collections.Immutable;

namespace SparkCore.Analytics.Binding.Scope.Statements
{
    internal sealed class BoundBlockStatement : BoundStatement
    {
        public BoundBlockStatement(ImmutableArray<BoundStatement> statements)
        {
            Statements = statements;
        }

        public override BoundNodeType NodeType => BoundNodeType.BlockStatement;
        public ImmutableArray<BoundStatement> Statements { get; }
    }
}

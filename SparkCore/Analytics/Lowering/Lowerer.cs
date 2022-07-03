using SparkCore.Analytics.Binding.Scope.Expressions;
using SparkCore.Analytics.Binding.Scope.Statements;
using SparkCore.Analytics.Binding.Tree;
using SparkCore.Analytics.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparkCore.Analytics.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private Lowerer()
        {
            
        }

        public static BoundStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            return lowerer.RewriteStatement(statement);
        }


        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            // for <var> = <lower> to <upper>
            //      <body>
            //
            // ------->
            //
            // {
            //   var <var> = <lower>
            //   while(<var> < <upper>)
            //   {
            //       <body>
            //       <var> = <var> + 1
            //   }
            // }.
            var variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);
            var variableExpression = new BoundVariableExpression(node.Variable);
            var condition = new BoundBinaryExpression(
                variableExpression,
                BoundBinaryOperator.Bind(SyntaxKind.LessToken,typeof(int), typeof(int)),
                node.UpperBound
            );
            var increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOperator.Bind(SyntaxKind.PlusToken, typeof(int), typeof(int)),
                        new BoundLiteralExpression(1)
                    )
                )
            ); ;
           
            var whileBlock = new BoundBlockStatement(ImmutableArray.Create(node.Body, increment));
            var whileStatement = new BoundWhileStatement(condition, whileBlock);
            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(variableDeclaration, whileStatement));
            
            return RewriteStatement(result);
        }

    }
}
using Compiler.Core.Binding;
using Compiler.Core.Binding.Expressions;
using Compiler.Core.Syntax;
using Compiler.Core.Syntax.Expressions;
using System;

namespace Compiler.Core.Analyzers
{
    internal class Evaluator
    {
        private readonly BoundExpression _root;
        public Evaluator(BoundExpression root)
        {
            _root = root;
        }
        public object Evaluate()
        {
            return EvaluateExpression(_root);
        }
        private object EvaluateExpression(BoundExpression node)
        {
            if (node is BoundLiteralExpression n)
                return (object)n.Value;
            if(node is BoundUnaryExpression u)
            {
                var operand = EvaluateExpression(u.Operand);
                switch (u.Op.BoundType)
                {
                    case BoundUnaryOperatorType.Negation:
                        return -(int)operand;
                    case BoundUnaryOperatorType.Identity:
                        return (int)operand;
                    case BoundUnaryOperatorType.LogicalNegation:
                        return !(bool)operand;
                    default:
                        throw new Exception($"Unexpected unary operator {u.Op}");
                }
            }
            if (node is BoundBinaryExpression b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);
                switch (b.Op.BoundType)
                {
                    case BoundBinaryOperatorType.Addition:
                        return (int)left + (int)right;
                    case BoundBinaryOperatorType.Substraction:
                        return (int)left - (int)right;
                    case BoundBinaryOperatorType.Multiplication:
                        return (int)left * (int)right;
                    case BoundBinaryOperatorType.Divicion:
                        return (int)left / (int)right;
                    case BoundBinaryOperatorType.LogicalAnd:
                        return (bool)left && (bool)right;
                    case BoundBinaryOperatorType.LogicalOr:
                        return (bool)left || (bool)right;
                    case BoundBinaryOperatorType.Equals:
                        return Equals(left,right);
                    case BoundBinaryOperatorType.NotEquals:
                        return !Equals(left, right);
                    default:
                        throw new Exception($"Unexpected binary operator {b.Op}");
                }
            }
            throw new Exception($"Unexpected node operator {node.Type}");
        }

    }
}
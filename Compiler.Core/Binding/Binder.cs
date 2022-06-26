using Compiler.Core.Syntax;
using Compiler.Core.Syntax.Expressions;
using Compiler.Core.Binding.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core.Binding
{
    internal sealed class Binder
    {
        private readonly List<string> _diagnostics = new List<string>();
        public IEnumerable<string> Diagnostics => _diagnostics;
        public BoundExpression BindExpression(SyntaxExpression syntax)
        {
            switch (syntax.Type)
            {
                case SyntaxType.LiteralExpression:
                    return BindLiteralExpression((LiteralSyntaxExpression)syntax);
                case SyntaxType.UnaryExpression:
                    return BindUnaryExpression((UnarySyntaxExpression)syntax);
                case SyntaxType.BinaryExpression:
                    return BindBinaryExpression((BinarySyntaxExpression)syntax);
                default:
                    throw new Exception($"Unexpected syntax: {syntax.Type}");
            }

        }

        private BoundExpression BindLiteralExpression(LiteralSyntaxExpression syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnarySyntaxExpression syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            var boundOperatorType = BoundUnaryOperator.Bind(syntax.OperatorToken.Type, boundOperand.Type);
            if(boundOperatorType == null)
            {
                _diagnostics.Add($"Unary operator '{syntax.OperatorToken.Text}' is not definded fo type {boundOperand.Type}");
                return boundOperand;
            }
            return new BoundUnaryExpression(boundOperatorType, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinarySyntaxExpression syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);
            var boundOperatorType = BoundBinaryOperator.Bind(syntax.OperatorToken.Type, boundLeft.Type, boundRight.Type);
            if (boundOperatorType == null)
            {
                _diagnostics.Add($"Biary operator '{syntax.OperatorToken.Text}' is not definded fo type {boundLeft.Type} and {boundRight.Type}");
                return boundLeft;
            }
            return new BoundBinaryExpression(boundLeft, boundOperatorType, boundRight);
        }
    }
}

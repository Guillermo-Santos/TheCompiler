using Compiler.Core.Syntax;
using Compiler.Core.Syntax.Expressions;
using Compiler.Core.Binding.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Core.Diagnostics;
using System.Linq;

namespace Compiler.Core.Binding
{
    internal sealed class Binder
    {
        private readonly Dictionary<VariableSymbol, object> _variables;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();

        public Binder(Dictionary<VariableSymbol, object> variables)
        {
            _variables = variables;
        }

        public DiagnosticBag Diagnostics => _diagnostics;


        public BoundExpression BindExpression(SyntaxExpression syntax)
        {
            switch (syntax.Type)
            {
                case SyntaxType.ParenthesizedExpression:
                    return BindParenthesizedExpression((ParenthesizedSyntaxExpression)syntax);
                case SyntaxType.LiteralExpression:
                    return BindLiteralExpression((LiteralSyntaxExpression)syntax);
                case SyntaxType.NameExpression:
                    return BindNameExpression((NameSyntaxExpression)syntax);
                case SyntaxType.AssignmentExpression:
                    return BindAssignmentExpression((AssignmentSyntaxExpression)syntax);
                case SyntaxType.UnaryExpression:
                    return BindUnaryExpression((UnarySyntaxExpression)syntax);
                case SyntaxType.BinaryExpression:
                    return BindBinaryExpression((BinarySyntaxExpression)syntax);
                default:
                    throw new Exception($"Unexpected syntax: {syntax.Type}");
            }

        }

        private BoundExpression BindParenthesizedExpression(ParenthesizedSyntaxExpression syntax)
        {
            return BindExpression(syntax.Expression);
        }

        private BoundExpression BindLiteralExpression(LiteralSyntaxExpression syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindNameExpression(NameSyntaxExpression syntax)
        {
            var name = syntax.IdentifierToken.Text;
            var variable = _variables.Keys.FirstOrDefault(v => v.Name == name);
            if (variable == null)
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundLiteralExpression(0);
            }
            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentSyntaxExpression syntax)
        {
            var name = syntax.IdentifierToken.Text;
            var boundExpression = BindExpression(syntax.Expression);

            var existingvariable = _variables.Keys.FirstOrDefault(v => v.Name == name);
            if (existingvariable != null)
                _variables.Remove(existingvariable);
            
            var variable = new VariableSymbol(name, boundExpression.Type);
            _variables[variable] = null;
           
            return new BoundAssigmentExpression(variable, boundExpression);
        }

        private BoundExpression BindUnaryExpression(UnarySyntaxExpression syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            var boundOperatorType = BoundUnaryOperator.Bind(syntax.OperatorToken.Type, boundOperand.Type);
            if(boundOperatorType == null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
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
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return boundLeft; 
            }
            return new BoundBinaryExpression(boundLeft, boundOperatorType, boundRight);
        }
    }
}

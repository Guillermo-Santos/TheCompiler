using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SparkCore.Analytics.Binding.Scope;
using SparkCore.Analytics.Binding.Scope.Expressions;
using SparkCore.Analytics.Binding.Scope.Statements;
using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Syntax.Tree.Expressions;
using SparkCore.Analytics.Syntax.Tree.Nodes;
using SparkCore.Analytics.Syntax.Tree.Statements;

namespace SparkCore.Analytics.Binding
{
    //TODO: Change for syntax and BoundLowering from 'for <var> = <lower> to <upper> <body>' to 'for(ExpressionStatement ; condition ; increment) <body>'
    internal sealed class Binder
    {
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private BoundScope _scope;

        public Binder(BoundScope parent)
        {
            _scope = new BoundScope(parent);
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationSyntaxUnit syntax)
        {
            var parentScope = CreateParenScopes(previous);
            var binder = new Binder(parentScope);
            var expression = binder.BindStatement(syntax.Statement);
            var variables = binder._scope.GetDeclaredVariales();
            var diagnostics = binder._diagnostics.ToImmutableArray();

            if (previous != null)
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

            return new BoundGlobalScope(previous, diagnostics, variables, expression);
        }

        private static BoundScope CreateParenScopes(BoundGlobalScope previous)
        {
            var stack = new Stack<BoundGlobalScope>();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }
            BoundScope parent = null;
            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);
                foreach (var v in previous.Variables)
                    scope.TryDeclare(v);
                parent = scope;
            }
            return parent;
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        private BoundStatement BindStatement(SyntaxStatement syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.BlockStatement:
                    return BindBlockStatement((BlockSyntaxStatement)syntax);
                case SyntaxKind.VariableDeclarationStatement:
                    return BindVariableDeclaration((VariableDeclarationSyntaxStatement)syntax);
                case SyntaxKind.IfStatement:
                    return BindIfStatement((IfSyntaxStatement)syntax);
                case SyntaxKind.WhileStatement:
                    return BindWhileStatement((WhileSyntaxStatement)syntax);
                case SyntaxKind.ForStatement:
                    return BindForStatement((ForSyntaxStatement)syntax);
                case SyntaxKind.ExpressionStatement:
                    return BindExpressionStatement((ExpressionSyntaxStatement)syntax);
                default:
                    throw new Exception($"Unexpected syntax: {syntax.Kind}");
            }

        }

        private BoundStatement BindBlockStatement(BlockSyntaxStatement syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);
            foreach (var statementSyntax in syntax.Statements)
            {
                var statement = BindStatement(statementSyntax);
                statements.Add(statement);
            }

            _scope = _scope.Parent;

            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundStatement BindVariableDeclaration(VariableDeclarationSyntaxStatement syntax)
        {
            var name = syntax.Identifier.Text;
            var isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
            var initializer = BindExpression(syntax.Initializer);
            var variable = new VariableSymbol(name, isReadOnly, initializer.Type);

            if (!_scope.TryDeclare(variable))
            {
                _diagnostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);
            }

            return new BoundVariableDeclaration(variable, initializer);
        }
        private BoundStatement BindIfStatement(IfSyntaxStatement syntax)
        {
            var condition = BindExpression(syntax.Condition, typeof(bool));
            var thenStatement = BindStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }
        private BoundStatement BindWhileStatement(WhileSyntaxStatement syntax)
        {
            var condition = BindExpression(syntax.Condition, typeof(bool));
            var body = BindStatement(syntax.Body);
            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement BindForStatement(ForSyntaxStatement syntax)
        {
            var lowerBound = BindExpression(syntax.LowerBound, typeof(int));
            var upperBound = BindExpression(syntax.UpperBound, typeof(int));

            _scope = new BoundScope(_scope);

            var name = syntax.Identifier.Text;
            var variable = new VariableSymbol(name, true, typeof(int));
            if (!_scope.TryDeclare(variable))
            {
                _diagnostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);
            }

            var body = BindStatement(syntax.Body);

            _scope = _scope.Parent;

            return new BoundForStatement(variable, lowerBound, upperBound, body);
        }
        private BoundStatement BindExpressionStatement(ExpressionSyntaxStatement syntax)
        {
            var expression = BindExpression(syntax.Expression);
            return new BoundExpressionStatement(expression);
        }

        private BoundExpression BindExpression(SyntaxExpression syntax, Type targetType)
        {
            var result = BindExpression(syntax);
            if (result.Type != targetType)
                _diagnostics.ReportCannotConvert(syntax.Span, result.Type, targetType);
            return result;
        }

        private BoundExpression BindExpression(SyntaxExpression syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.ParenthesizedExpression:
                    return BindParenthesizedExpression((ParenthesizedSyntaxExpression)syntax);
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralSyntaxExpression)syntax);
                case SyntaxKind.NameExpression:
                    return BindNameExpression((NameSyntaxExpression)syntax);
                case SyntaxKind.AssignmentExpression:
                    return BindAssignmentExpression((AssignmentSyntaxExpression)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnarySyntaxExpression)syntax);
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinarySyntaxExpression)syntax);
                default:
                    throw new Exception($"Unexpected syntax: {syntax.Kind}");
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
            if (string.IsNullOrEmpty(name))
            {
                //This means the token was inserted by the parser. we already
                //reported error so we can just return an error expression.
                return new BoundLiteralExpression(0);
            }

            if (!_scope.TryLookup(name, out var variable))
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

            if (!_scope.TryLookup(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return boundExpression;
            }

            if (variable.IsReadOnly)
                _diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, name);

            if (boundExpression.Type != variable.Type)
            {
                _diagnostics.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, variable.Type);
                return boundExpression;
            }


            return new BoundAssignmentExpression(variable, boundExpression);
        }

        private BoundExpression BindUnaryExpression(UnarySyntaxExpression syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            var boundOperatorType = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);
            if (boundOperatorType == null)
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
            var boundOperatorType = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);
            if (boundOperatorType == null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return boundLeft;
            }
            return new BoundBinaryExpression(boundLeft, boundOperatorType, boundRight);
        }
    }
}

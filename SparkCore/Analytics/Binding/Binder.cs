using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SparkCore.Analytics.Binding.Tree;
using SparkCore.Analytics.Binding.Tree.Expressions;
using SparkCore.Analytics.Binding.Tree.Statements;
using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.Analytics.Syntax.Tree.Expressions;
using SparkCore.Analytics.Syntax.Tree.Nodes;
using SparkCore.Analytics.Syntax.Tree.Statements;

namespace SparkCore.Analytics.Binding;

//TODO: Change for syntax and BoundLowering from 'for <var> = <lower> to <upper> <body>' to 'for(ExpressionStatement ; condition ; increment) <body>'
internal sealed class Binder
{
    private readonly DiagnosticBag _diagnostics = new();
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
        {
            diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);
        }

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
            {
                scope.TryDeclare(v);
            }

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
        var isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
        var initializer = BindExpression(syntax.Initializer);
        var variable = BindVariable(syntax.Identifier, isReadOnly, initializer.Type);

        return new BoundVariableDeclaration(variable, initializer);
    }
    private BoundStatement BindIfStatement(IfSyntaxStatement syntax)
    {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var thenStatement = BindStatement(syntax.ThenStatement);
        var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
        return new BoundIfStatement(condition, thenStatement, elseStatement);
    }
    private BoundStatement BindWhileStatement(WhileSyntaxStatement syntax)
    {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var body = BindStatement(syntax.Body);
        return new BoundWhileStatement(condition, body);
    }

    private BoundStatement BindForStatement(ForSyntaxStatement syntax)
    {
        var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
        var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);

        _scope = new BoundScope(_scope);
        var identifier = syntax.Identifier;
        var variable = BindVariable(identifier, isReadOnly: true, TypeSymbol.Int);

        var body = BindStatement(syntax.Body);

        _scope = _scope.Parent;

        return new BoundForStatement(variable, lowerBound, upperBound, body);
    }


    private BoundStatement BindExpressionStatement(ExpressionSyntaxStatement syntax)
    {
        var expression = BindExpression(syntax.Expression);
        return new BoundExpressionStatement(expression);
    }

    private BoundExpression BindExpression(SyntaxExpression syntax, TypeSymbol targetType)
    {
        var result = BindExpression(syntax);
        if(targetType != TypeSymbol.Error && 
           result.Type != TypeSymbol.Error &&
           result.Type != targetType)
        {
            _diagnostics.ReportCannotConvert(syntax.Span, result.Type, targetType);
        }
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
            case SyntaxKind.CallExpression:
                return BindCallExpression((CallSyntaxExpression)syntax);
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
        if (syntax.IdentifierToken.IsMissing)
        {
            //This means the token was inserted by the parser. we already
            //reported error so we can just return an error expression.
            return new BoundErrorExpression();
        }

        if (!_scope.TryLookup(name, out var variable))
        {
            _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
            return new BoundErrorExpression();
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
        {
            _diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, name);
        }

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

        if (boundOperand.Type == TypeSymbol.Error)
        {
            return new BoundErrorExpression();
        }

        var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);
        if (boundOperator == null)
        {
            _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
            return new BoundErrorExpression();
        }
        return new BoundUnaryExpression(boundOperator, boundOperand);
    }

    private BoundExpression BindBinaryExpression(BinarySyntaxExpression syntax)
    {
        var boundLeft = BindExpression(syntax.Left);
        var boundRight = BindExpression(syntax.Right);

        if(boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
        {
            return new BoundErrorExpression();
        }

        var boundOperatorType = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);
        if (boundOperatorType == null)
        {
            _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
            return new BoundErrorExpression();
        }
        return new BoundBinaryExpression(boundLeft, boundOperatorType, boundRight);
    }
    private BoundExpression BindCallExpression(CallSyntaxExpression syntax)
    {
        var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

        foreach(var argument in syntax.Arguments)
        {
            var boundArgument = BindExpression(argument);
            boundArguments.Add(boundArgument);
        }

        var functions = BuiltinFunctions.GetAll();
        var function = functions.SingleOrDefault(f => f.Name == syntax.Identifier.Text);
        if(function == null)
        {
            _diagnostics.ReportUndefinedFunction(syntax.Identifier.Span, syntax.Identifier.Text);
            return new BoundErrorExpression();
        }
        
        if(syntax.Arguments.Count != function.Parameters.Length)
        {
            _diagnostics.ReportWrongArgumentCount(syntax.Span, function.Name, function.Parameters.Length, syntax.Arguments.Count);
            return new BoundErrorExpression();
        }

        for(var i = 0; i < syntax.Arguments.Count; i++)
        {
            var argument = boundArguments[i];
            var parameter = function.Parameters[i];
            if(argument.Type != parameter.Type)
            {
                _diagnostics.ReportWrongArgumentType(syntax.Span, parameter.Name, parameter.Type, argument.Type);
                return new BoundErrorExpression();
            }
        }

        return new BoundCallExpression(function, boundArguments.ToImmutable());
    }

    private VariableSymbol BindVariable(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
    {
        var name = identifier.Text ?? "?";
        var declare = !identifier.IsMissing;
        var variable = new VariableSymbol(name, isReadOnly, type);

        if (declare && !_scope.TryDeclare(variable))
        {
            _diagnostics.ReportVariableAlreadyDeclared(identifier.Span, name);
        }

        return variable;
    }
}

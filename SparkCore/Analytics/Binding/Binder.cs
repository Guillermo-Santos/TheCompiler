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
using SparkCore.Analytics.Text;
using System.Linq.Expressions;
using SparkCore.Analytics.Lowering;

namespace SparkCore.Analytics.Binding;
// Waiting to have 'break' and 'continue' statements to do this:
// TODO: Change for syntax and BoundLowering from 'for <var> = <lower> to <upper> <body>' to 'for(ExpressionStatement ; condition ; increment) <body>'
internal sealed class Binder
{
    private readonly DiagnosticBag _diagnostics = new();
    private readonly FunctionSymbol _function;
    private readonly Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> _loopStack = new();
    private int _labelCounter;
    private BoundScope _scope;
    public DiagnosticBag Diagnostics => _diagnostics;

    public Binder(BoundScope parent, FunctionSymbol function)
    {
        _scope = new BoundScope(parent);
        _function = function;

        if(function != null)
        {
            foreach (var p in function.Parameters)
                _scope.TryDeclareVariable(p);
        }
    }

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax)
    {
        var parentScope = CreateParenScopes(previous);
        var binder = new Binder(parentScope, function: null);

        foreach (var function in syntax.Members.OfType<FunctionDeclarationSyntax>())
            binder.BindFunctionDeclaration(function);

        var statements = ImmutableArray.CreateBuilder<BoundStatement>();

        foreach(var globalStatement in syntax.Members.OfType<GlobalStatementSyntax>())
        {
            var statement = binder.BindStatement(globalStatement.Statement);
            statements.Add(statement);
        }

        var functions = binder._scope.GetDeclaredFunctions();
        var variables = binder._scope.GetDeclaredVariales();
        var diagnostics = binder._diagnostics.ToImmutableArray();

        if (previous != null)
        {
            diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);
        }

        return new BoundGlobalScope(previous, diagnostics, functions, variables, statements.ToImmutable());
    }

    public static BoundProgram BindProgram(BoundGlobalScope globalScope)
    {
        var parentScope = CreateParenScopes(globalScope);

        var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var scope = globalScope;

        while(scope != null)
        {

            foreach (var function in scope.Functions)
            {
                var binder = new Binder(parentScope, function);
                var body = binder.BindStatement(function.Declaration.Body);
                var loweredBody = Lowerer.Lower(body);
                functionBodies.Add(function, loweredBody);

                diagnostics.AddRange(binder.Diagnostics);
            }

            scope = scope.Previous;
        }

        var statement = Lowerer.Lower(new BoundBlockStatement(globalScope.Statements));

        return new BoundProgram(diagnostics.ToImmutable(), functionBodies.ToImmutable(), statement);
    }

    private void BindFunctionDeclaration(FunctionDeclarationSyntax syntax)
    {
        var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        var seenParameterNames = new HashSet<string>();

        foreach(var parameterSyntax in syntax.Parameters)
        {
            var parameterName = parameterSyntax.Identifier.Text;
            var parameterType = BindTypeClause(parameterSyntax.Type);
            if (!seenParameterNames.Add(parameterName))
            {
                _diagnostics.ReportParameterAlreadyDeclared(parameterSyntax.Span, parameterName);
            }
            else
            {
                var parameter = new ParameterSymbol(parameterName, parameterType);
                parameters.Add(parameter);
            }
        }

        var type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;
        if (type != TypeSymbol.Void)
            _diagnostics.XXX_ReportFunctionsAreUnsupported(syntax.Type.Span);

        var function = new FunctionSymbol(syntax.Identifier.Text, parameters.ToImmutable(), type, syntax);

        if (!_scope.TryDeclareFunction(function))
        {
            _diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Span, function.Name);
        }
    }

    private static BoundScope CreateParenScopes(BoundGlobalScope previous)
    {
        var stack = new Stack<BoundGlobalScope>();
        while (previous != null)
        {
            stack.Push(previous);
            previous = previous.Previous;
        }
        var parent = CreateRootScope();
        while (stack.Count > 0)
        {
            previous = stack.Pop();
            var scope = new BoundScope(parent);
            foreach (var f in previous.Functions)
            {
                scope.TryDeclareFunction(f);
            }
            foreach (var v in previous.Variables)
            {
                scope.TryDeclareVariable(v);
            }

            parent = scope;
        }
        return parent;
    }
    private static BoundScope CreateRootScope()
    {
        var result = new BoundScope(null);

        foreach(var f in BuiltinFunctions.GetAll())
            result.TryDeclareFunction(f);

        return result;
    }

    private BoundStatement BindErrorStatement() => new BoundExpressionStatement(new BoundErrorExpression());
    private BoundStatement BindStatement(StatementSyntax syntax)
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
            case SyntaxKind.DoWhileStatement:
                return BindDoWhileStatement((DoWhileSyntaxStatement)syntax);
            case SyntaxKind.ForStatement:
                return BindForStatement((ForSyntaxStatement)syntax);
            case SyntaxKind.BreakStatement:
                return BindBreakStatement((BreakStatementSyntax)syntax);
            case SyntaxKind.ContinueStatement:
                return BindContinueStatement((ContinueStatementSyntax)syntax);
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
        var type = BindTypeClause(syntax.TypeClause);
        var initializer = BindExpression(syntax.Initializer);
        var variableType = type ?? initializer.Type;
        var variable = BindVariable(syntax.Identifier, isReadOnly, variableType);
        var convertedInitializer = BindConversion(syntax.Initializer.Span, initializer, variableType);

        return new BoundVariableDeclaration(variable, convertedInitializer);
    }
    private TypeSymbol BindTypeClause(TypeClauseSyntax syntax)
    {
        if (syntax == null)
            return null;
        var type = LookUpType(syntax.Identifier.Text);
        if(type == null)
            _diagnostics.ReportUndefinedType(syntax.Identifier.Span, syntax.Identifier.Text);
        return type;
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
        var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
        return new BoundWhileStatement(condition, body, breakLabel, continueLabel);
    }
    private BoundStatement BindDoWhileStatement(DoWhileSyntaxStatement syntax)
    {
        var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        return new BoundDoWhileStatement(body, condition, breakLabel, continueLabel);
    }
    private BoundStatement BindForStatement(ForSyntaxStatement syntax)
    {
        var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
        var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);

        _scope = new BoundScope(_scope);
        var identifier = syntax.Identifier;
        var variable = BindVariable(identifier, isReadOnly: true, TypeSymbol.Int);

        var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);

        _scope = _scope.Parent;

        return new BoundForStatement(variable, lowerBound, upperBound, body, breakLabel, continueLabel);
    }
    private BoundStatement BindLoopBody(StatementSyntax body, out BoundLabel breakLabel, out BoundLabel continueLabel)
    {
        _labelCounter++;
        breakLabel = new($"break{_labelCounter}");
        continueLabel = new($"continue{_labelCounter}");

        _loopStack.Push((breakLabel, continueLabel));
        var boundBody = BindStatement(body);
        _loopStack.Pop();

        return boundBody;
    }
    private BoundStatement BindBreakStatement(BreakStatementSyntax syntax)
    {
        if(_loopStack.Count == 0)
        {
            _diagnostics.ReportInvalidBreackOrContinue(syntax.Keyword.Span, syntax.Keyword.Text);
            return BindErrorStatement();
        }
        var breakLabel = _loopStack.Peek().BreakLabel;
        return new BoundGotoStatement(breakLabel);
    }


    private BoundStatement BindContinueStatement(ContinueStatementSyntax syntax)
    {
        if (_loopStack.Count == 0)
        {
            _diagnostics.ReportInvalidBreackOrContinue(syntax.Keyword.Span, syntax.Keyword.Text);
            return BindErrorStatement();
        }
        var continueLabel = _loopStack.Peek().ContinueLabel;
        return new BoundGotoStatement(continueLabel);
    }
    private BoundStatement BindExpressionStatement(ExpressionSyntaxStatement syntax)
    {
        var expression = BindExpression(syntax.Expression, canBeVoid: true);
        return new BoundExpressionStatement(expression);
    }


    private BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol targetType)
    {
        return BindConversion(syntax, targetType);
    }
    private BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
    {
        var result = BindExpressionInternal(syntax);
        if(!canBeVoid && result.Type == TypeSymbol.Void)
        {
            _diagnostics.ReportExpressionMustHaveValue(syntax.Span);
            return new BoundErrorExpression();
        }
        return result;
    }
    private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
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

        if (!_scope.TryLookupVariable(name, out var variable))
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

        if (!_scope.TryLookupVariable(name, out var variable))
        {
            _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
            return boundExpression;
        }

        if (variable.IsReadOnly)
        {
            _diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, name);
        }

        var convertedExpression = BindConversion(syntax.Expression.Span, boundExpression, variable.Type);

        return new BoundAssignmentExpression(variable, convertedExpression);
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
        if (syntax.Arguments.Count == 1 && LookUpType(syntax.Identifier.Text) is TypeSymbol type)
            return BindConversion(syntax.Arguments[0], type, allowExplicit: true);

        var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

        foreach(var argument in syntax.Arguments)
        {
            var boundArgument = BindExpression(argument);
            boundArguments.Add(boundArgument);
        }

        if(!_scope.TryLookupFunction(syntax.Identifier.Text, out var function))
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
                _diagnostics.ReportWrongArgumentType(syntax.Arguments[i].Span, parameter.Name, parameter.Type, argument.Type);
                return new BoundErrorExpression();
            }
        }

        return new BoundCallExpression(function, boundArguments.ToImmutable());
    }
    private VariableSymbol BindVariable(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
    {
        var name = identifier.Text ?? "?";
        var declare = !identifier.IsMissing;
        var variable = _function == null
                         ? (VariableSymbol) new GlobalVariableSymbol(name, isReadOnly, type)
                         : new LocalVariableSymbol(name, isReadOnly, type);

        if (declare && !_scope.TryDeclareVariable(variable))
        {
            _diagnostics.ReportSymbolAlreadyDeclared(identifier.Span, name);
        }

        return variable;
    }

    private BoundExpression BindConversion(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicit = false)
    {
        var expression = BindExpression(syntax);
        return BindConversion(syntax.Span, expression, type, allowExplicit);
    }
    private BoundExpression BindConversion(TextSpan diagnosticSpan, BoundExpression expression, TypeSymbol type, bool allowExplicit = false)
    {
        var conversion = Conversion.Classify(expression.Type, type);

        if (!conversion.Exists)
        {
            if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
            {
                _diagnostics.ReportCannotConvert(diagnosticSpan, expression.Type, type);
            }
            return new BoundErrorExpression();
        }

        if(!allowExplicit && conversion.IsExplicit)
        {
            _diagnostics.ReportCannotImplicitlyConvert(diagnosticSpan, expression.Type, type);
        }

        if (conversion.IsIdentity)
            return expression;

        return new BoundConversionExpression(type, expression);
    }

    private TypeSymbol LookUpType(string name)
    {
        switch (name)
        {
            case "bool": 
                return TypeSymbol.Bool;
            case "int": 
                return TypeSymbol.Int;
            case "string": 
                return TypeSymbol.String;
            default:
                return null;
        }
    }
}

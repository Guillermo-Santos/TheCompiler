using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SparkCore.Analytics.Binding.Tree;
using SparkCore.Analytics.Binding.Tree.Expressions;
using SparkCore.Analytics.Binding.Tree.Statements;
using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.Analytics.Syntax.Tree.Expressions;
using SparkCore.Analytics.Syntax.Tree.Nodes;
using SparkCore.Analytics.Syntax.Tree.Statements;
using SparkCore.Analytics.Lowering;
using SparkCore.IO.Text;
using SparkCore.IO.Diagnostics;

namespace SparkCore.Analytics.Binding;


// TODO: Add ';' "SemiColonToken" to the lenguage and all expressions / statements that need it.
// TODO: Add non obligatory assigned variables (let x = "asdad"  --> let x : string;)
// TODO: Change for syntax and BoundLowering from 'for <var> = <lower> to <upper> <body>' to 'for(ExpressionStatement ; condition ; increment) <body>'
internal sealed class Binder
{
    private readonly DiagnosticBag _diagnostics = new();
    private readonly bool _isScript;
    private readonly FunctionSymbol _function;
    private readonly Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> _loopStack = new();
    private int _labelCounter;
    private BoundScope _scope;
    public DiagnosticBag Diagnostics => _diagnostics;

    public Binder(bool isScript, BoundScope parent, FunctionSymbol function)
    {
        _scope = new BoundScope(parent);
        _isScript = isScript;
        _function = function;

        if(function != null)
        {
            foreach (var p in function.Parameters)
                _scope.TryDeclareVariable(p);
        }
    }

    public static BoundGlobalScope BindGlobalScope(bool isScript, BoundGlobalScope previous, ImmutableArray<SyntaxTree> syntaxTrees)
    {
        var parentScope = CreateParenScopes(previous);
        var binder = new Binder(isScript, parentScope, function: null);

        var functionDelcarations = syntaxTrees.SelectMany(st => st.Root.Members)
                                              .OfType<FunctionDeclarationSyntax>();

        foreach (var function in functionDelcarations)
            binder.BindFunctionDeclaration(function);

        var globalStatements = syntaxTrees.SelectMany(st => st.Root.Members)
                                          .OfType<GlobalStatementSyntax>();

        var statements = ImmutableArray.CreateBuilder<BoundStatement>();

        foreach (var globalStatement in globalStatements)
        {
            var statement = binder.BindGlobalStatement(globalStatement.Statement);
            statements.Add(statement);
        }

        // Check global statements
        var firstGlobalStatementPerSyntaxTree = syntaxTrees.Select(st => st.Root.Members.OfType<GlobalStatementSyntax>().FirstOrDefault())
                                                           .Where(g => g != null)
                                                           .ToArray();
        if(firstGlobalStatementPerSyntaxTree.Length > 1)
        {
            foreach(var globalstatement in firstGlobalStatementPerSyntaxTree)
            {
                binder.Diagnostics.ReportOnlyOneFileCanHaveGlobalStatements(globalstatement.Location);
            }
        }

        // Check for main function
        
        var functions = binder._scope.GetDeclaredFunctions();
        FunctionSymbol mainFunction;
        FunctionSymbol scriptFunction;

        if (isScript)
        {
            mainFunction = null;
            if (globalStatements.Any())
            {
                scriptFunction = new FunctionSymbol("$eval", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Any);
            }
            else
            {
                scriptFunction = null;
            }
        }
        else
        {
            mainFunction = functions.SingleOrDefault(f => f.Name == "main");
            scriptFunction = null;

            if (mainFunction != null)
            {
                if (mainFunction.Type != TypeSymbol.Void || mainFunction.Parameters.Any())
                {
                    binder.Diagnostics.ReportMainMustHaveCorrectSignature(mainFunction.Declaration.Identifier.Location);
                }
            }

            if (globalStatements.Any())
            {
                if (mainFunction != null)
                {
                    binder.Diagnostics.ReportCannotMixMainAndGlobalStatements(mainFunction.Declaration.Identifier.Location);

                    foreach (var globalStatement in firstGlobalStatementPerSyntaxTree)
                    {
                        binder.Diagnostics.ReportCannotMixMainAndGlobalStatements(globalStatement.Location);
                    }
                }
                else
                {
                    mainFunction = new FunctionSymbol("main", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void);
                }
            }

        }

        var diagnostics = binder.Diagnostics.ToImmutableArray();


        var variables = binder._scope.GetDeclaredVariales();

        if (previous != null)
        {
            diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);
        }

        return new BoundGlobalScope(previous, diagnostics, mainFunction, scriptFunction, functions, variables, statements.ToImmutable());
    }

    public static BoundProgram BindProgram(bool isScript, BoundProgram previous, BoundGlobalScope globalScope)
    {
        var parentScope = CreateParenScopes(globalScope);

        var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

        foreach (var function in globalScope.Functions)
        {
            var binder = new Binder(isScript, parentScope, function);
            var body = binder.BindStatement(function.Declaration.Body);
            var loweredBody = Lowerer.Lower(body);
            if (function.Type != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(loweredBody))
                binder._diagnostics.ReportAllPathsMustReturn(function.Declaration.Identifier.Location);

            functionBodies.Add(function, loweredBody);

            diagnostics.AddRange(binder.Diagnostics);
        }

        if(globalScope.MainFunction != null && globalScope.Statements.Any())
        {
            var body = Lowerer.Lower(new BoundBlockStatement(globalScope.Statements));
            functionBodies.Add(globalScope.MainFunction, body);
        }
        else if(globalScope.ScriptFunction != null)
        {
            var statements = globalScope.Statements;

            if (statements.Length == 1 && 
                statements[0] is BoundExpressionStatement es &&
                es.Expression.Type != TypeSymbol.Void)
            {
                statements = statements.SetItem(0, new BoundReturnStatement(es.Expression));
            }
            else if (statements.Any() && statements.Last().Kind != BoundNodeKind.ReturnStatement)
            {
                var nullValue = new BoundLiteralExpression("");
                statements = statements.Add(new BoundReturnStatement(nullValue));
            }
            var body = Lowerer.Lower(new BoundBlockStatement(statements));
            functionBodies.Add(globalScope.ScriptFunction, body);
        }


        return new BoundProgram(previous, diagnostics.ToImmutable(), globalScope.MainFunction, globalScope.ScriptFunction, functionBodies.ToImmutable());
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
                _diagnostics.ReportParameterAlreadyDeclared(parameterSyntax.Location, parameterName);
            }
            else
            {
                var parameter = new ParameterSymbol(parameterName, parameterType);
                parameters.Add(parameter);
            }
        }

        var type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;

        var function = new FunctionSymbol(syntax.Identifier.Text, parameters.ToImmutable(), type, syntax);

        if (!function.Declaration.Identifier.IsMissing && !_scope.TryDeclareFunction(function))
        {
            _diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Location, function.Name);
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

    private BoundStatement BindGlobalStatement(StatementSyntax syntax)
    {
        return BindStatement(syntax, isGlobal: true);
    }
    private BoundStatement BindStatement(StatementSyntax syntax, bool isGlobal = false)
    {
        var result = BindStatementInternal(syntax);
        if(!_isScript || !isGlobal)
        {
            if(result is BoundExpressionStatement es)
            {
                var isAllowedExpression = es.Expression.Kind == BoundNodeKind.ErrorExpression ||
                                          es.Expression.Kind == BoundNodeKind.AssignmentExpression ||
                                          es.Expression.Kind == BoundNodeKind.CallExpression;
                if (!isAllowedExpression)
                    _diagnostics.ReportInvalidExpressionStatement(syntax.Location);

            }
        }

        return result;
    }
    private BoundStatement BindStatementInternal(StatementSyntax syntax)
    {
        switch (syntax.Kind)
        {
            case SyntaxKind.BlockStatement:
                return BindBlockStatement((BlockStatementSyntax)syntax);
            case SyntaxKind.VariableDeclarationStatement:
                return BindVariableDeclaration((VariableDeclarationStatementSyntax)syntax);
            case SyntaxKind.IfStatement:
                return BindIfStatement((IfStatementSyntax)syntax);
            case SyntaxKind.WhileStatement:
                return BindWhileStatement((WhileStatementSyntax)syntax);
            case SyntaxKind.DoWhileStatement:
                return BindDoWhileStatement((DoWhileStatementSyntax)syntax);
            case SyntaxKind.ForStatement:
                return BindForStatement((ForStatementSyntax)syntax);
            case SyntaxKind.BreakStatement:
                return BindBreakStatement((BreakStatementSyntax)syntax);
            case SyntaxKind.ContinueStatement:
                return BindContinueStatement((ContinueStatementSyntax)syntax);
            case SyntaxKind.ReturnStatement:
                return BindReturnStatement((ReturnStatementSyntax)syntax);
            case SyntaxKind.ExpressionStatement:
                return BindExpressionStatement((ExpressionStatementSyntax)syntax);
            default:
                throw new Exception($"Unexpected syntax: {syntax.Kind}");
        }
    }

    private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
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
    private BoundStatement BindVariableDeclaration(VariableDeclarationStatementSyntax syntax)
    {
        var isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
        var type = BindTypeClause(syntax.TypeClause);
        var initializer = BindExpression(syntax.Initializer);
        var variableType = type ?? initializer.Type;
        var variable = BindVariableDeclaration(syntax.Identifier, isReadOnly, variableType);
        var convertedInitializer = BindConversion(syntax.Initializer.Location, initializer, variableType);

        return new BoundVariableDeclaration(variable, convertedInitializer);
    }
    private TypeSymbol BindTypeClause(TypeClauseSyntax syntax)
    {
        if (syntax == null)
            return null;
        var type = LookUpType(syntax.Identifier.Text);
        if(type == null)
            _diagnostics.ReportUndefinedType(syntax.Identifier.Location, syntax.Identifier.Text);
        return type;
    }
    private BoundStatement BindIfStatement(IfStatementSyntax syntax)
    {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var thenStatement = BindStatement(syntax.ThenStatement);
        var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
        return new BoundIfStatement(condition, thenStatement, elseStatement);
    }
    private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
    {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
        return new BoundWhileStatement(condition, body, breakLabel, continueLabel);
    }
    private BoundStatement BindDoWhileStatement(DoWhileStatementSyntax syntax)
    {
        var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        return new BoundDoWhileStatement(body, condition, breakLabel, continueLabel);
    }
    private BoundStatement BindForStatement(ForStatementSyntax syntax)
    {
        var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
        var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);

        _scope = new BoundScope(_scope);
        var identifier = syntax.Identifier;
        var variable = BindVariableDeclaration(identifier, isReadOnly: true, TypeSymbol.Int);

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
            _diagnostics.ReportInvalidBreackOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
            return BindErrorStatement();
        }
        var breakLabel = _loopStack.Peek().BreakLabel;
        return new BoundGotoStatement(breakLabel);
    }
    private BoundStatement BindContinueStatement(ContinueStatementSyntax syntax)
    {
        if (_loopStack.Count == 0)
        {
            _diagnostics.ReportInvalidBreackOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
            return BindErrorStatement();
        }
        var continueLabel = _loopStack.Peek().ContinueLabel;
        return new BoundGotoStatement(continueLabel);
    }
    private BoundStatement BindReturnStatement(ReturnStatementSyntax syntax)
    {
        var expression = syntax.Expression == null ? null : BindExpression(syntax.Expression);

        if(_function == null)
        {
            if (_isScript)
            {
                // Ignore because we allow both return with and without values.
                if (expression == null)
                    expression = new BoundLiteralExpression("");
            }
            else
            {
                // Main does not support return values.
                _diagnostics.ReportInvalidWithValueInGlobalStatement(syntax.ReturnKeyword.Location);
            }
        }
        else
        {
            if (_function.Type == TypeSymbol.Void)
            {
                if (expression != null)
                    _diagnostics.ReportInvalidReturnExpression(syntax.Expression.Location, _function.Name);
            }
            else
            {
                if (expression == null)
                    _diagnostics.ReportMissingReturnExpression(syntax.ReturnKeyword.Location, _function.Type);
                else
                    expression = BindConversion(syntax.Expression.Location, expression, _function.Type);
            }
        }

        return new BoundReturnStatement(expression);
    }
    private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
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
            _diagnostics.ReportExpressionMustHaveValue(syntax.Location);
            return new BoundErrorExpression();
        }
        return result;
    }
    private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
    {
        switch (syntax.Kind)
        {
            case SyntaxKind.ParenthesizedExpression:
                return BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax);
            case SyntaxKind.LiteralExpression:
                return BindLiteralExpression((LiteralExpressionSyntax)syntax);
            case SyntaxKind.NameExpression:
                return BindNameExpression((NameExpressionSyntax)syntax);
            case SyntaxKind.AssignmentExpression:
                return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
            case SyntaxKind.UnaryExpression:
                return BindUnaryExpression((UnaryExpressionSyntax)syntax);
            case SyntaxKind.BinaryExpression:
                return BindBinaryExpression((BinaryExpressionSyntax)syntax);
            case SyntaxKind.CallExpression:
                return BindCallExpression((CallExpressionSyntax)syntax);
            default:
                throw new Exception($"Unexpected syntax: {syntax.Kind}");
        }

    }
    private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
    {
        return BindExpression(syntax.Expression);
    }
    private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
    {
        var value = syntax.Value ?? 0;
        return new BoundLiteralExpression(value);
    }
    private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
    {
        var name = syntax.IdentifierToken.Text;
        if (syntax.IdentifierToken.IsMissing)
        {
            //This means the token was inserted by the parser. we already
            //reported error so we can just return an error expression.
            return new BoundErrorExpression();
        }

        var variable = BindVariableReference(syntax.IdentifierToken);
        if(variable == null)
            return new BoundErrorExpression();

        return new BoundVariableExpression(variable);
    }
    private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
    {
        var name = syntax.IdentifierToken.Text;
        var boundExpression = BindExpression(syntax.Expression);
        
        var variable = BindVariableReference(syntax.IdentifierToken);
        if (variable == null)
            return boundExpression;

        if (variable.IsReadOnly)
        {
            _diagnostics.ReportCannotAssign(syntax.EqualsToken.Location, name);
        }

        var convertedExpression = BindConversion(syntax.Expression.Location, boundExpression, variable.Type);

        return new BoundAssignmentExpression(variable, convertedExpression);
    }
    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
    {
        var boundOperand = BindExpression(syntax.Operand);

        if (boundOperand.Type == TypeSymbol.Error)
        {
            return new BoundErrorExpression();
        }

        var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);
        if (boundOperator == null)
        {
            _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text, boundOperand.Type);
            return new BoundErrorExpression();
        }
        return new BoundUnaryExpression(boundOperator, boundOperand);
    }
    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
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
            _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
            return new BoundErrorExpression();
        }
        return new BoundBinaryExpression(boundLeft, boundOperatorType, boundRight);
    }
    private BoundExpression BindCallExpression(CallExpressionSyntax syntax)
    {
        if (syntax.Arguments.Count == 1 && LookUpType(syntax.Identifier.Text) is TypeSymbol type)
            return BindConversion(syntax.Arguments[0], type, allowExplicit: true);

        var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

        foreach(var argument in syntax.Arguments)
        {
            var boundArgument = BindExpression(argument);
            boundArguments.Add(boundArgument);
        }

        var symbol = _scope.TryLookupSymbol(syntax.Identifier.Text);
        if(symbol == null)
        {
            _diagnostics.ReportUndefinedFunction(syntax.Identifier.Location, syntax.Identifier.Text);
            return new BoundErrorExpression();
        }

        if (symbol is not FunctionSymbol function)
        {
            _diagnostics.ReportNotAFunction(syntax.Identifier.Location, syntax.Identifier.Text);
            return new BoundErrorExpression();
        }

        if (syntax.Arguments.Count != function.Parameters.Length)
        {
            TextSpan span;
            if (syntax.Arguments.Count > function.Parameters.Length)
            {
                SyntaxNode firstExceedingNode;
                if (function.Parameters.Length > 0)
                    firstExceedingNode = syntax.Arguments.GetSeparator(function.Parameters.Length - 1);
                else
                    firstExceedingNode = syntax.Arguments[0];
                var lastExceedingArgument = syntax.Arguments[syntax.Arguments.Count - 1];
                span = TextSpan.FromBounds(firstExceedingNode.Span.Start, lastExceedingArgument.Span.End);
            }
            else
            {
                span = syntax.CloseParentesis.Span;
            }
            var location = new TextLocation(syntax.SyntaxTree.Text, span);
            _diagnostics.ReportWrongArgumentCount(location, function.Name, function.Parameters.Length, syntax.Arguments.Count);
            return new BoundErrorExpression();
        }

        for(var i = 0; i < syntax.Arguments.Count; i++)
        {
            var argumentLocation = syntax.Arguments[i].Location;
            var argument = boundArguments[i];
            var parameter = function.Parameters[i];

            var convertedArgument = BindConversion(argumentLocation, argument, parameter.Type);
            boundArguments[i] = convertedArgument;
        }

        return new BoundCallExpression(function, boundArguments.ToImmutable());
    }
    private VariableSymbol BindVariableDeclaration(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
    {
        var name = identifier.Text ?? "?";
        var declare = !identifier.IsMissing;
        var variable = _function == null
                         ? (VariableSymbol) new GlobalVariableSymbol(name, isReadOnly, type)
                         : new LocalVariableSymbol(name, isReadOnly, type);

        if (declare && !_scope.TryDeclareVariable(variable))
        {
            _diagnostics.ReportSymbolAlreadyDeclared(identifier.Location, name);
        }

        return variable;
    }

    private VariableSymbol BindVariableReference(SyntaxToken identifierToken)
    {
        var name = identifierToken.Text;
        switch (_scope.TryLookupSymbol(name))
        {
            case VariableSymbol variable:
                return variable;
            case null:
                _diagnostics.ReportUndefinedVariable(identifierToken.Location, name);
                return null;
            default:
                _diagnostics.ReportNotAVariable(identifierToken.Location, name);
                return null;
        }
    }

    private BoundExpression BindConversion(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicit = false)
    {
        var expression = BindExpression(syntax);
        return BindConversion(syntax.Location, expression, type, allowExplicit);
    }
    private BoundExpression BindConversion(TextLocation diagnosticLocation, BoundExpression expression, TypeSymbol type, bool allowExplicit = false)
    {
        var conversion = Conversion.Classify(expression.Type, type);

        if (!conversion.Exists)
        {
            if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
            {
                _diagnostics.ReportCannotConvert(diagnosticLocation, expression.Type, type);
            }
            return new BoundErrorExpression();
        }

        if(!allowExplicit && conversion.IsExplicit)
        {
            _diagnostics.ReportCannotImplicitlyConvert(diagnosticLocation, expression.Type, type);
        }

        if (conversion.IsIdentity)
            return expression;

        return new BoundConversionExpression(type, expression);
    }

    private TypeSymbol LookUpType(string name)
    {
        switch (name)
        {
            case "any": 
                return TypeSymbol.Any;
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

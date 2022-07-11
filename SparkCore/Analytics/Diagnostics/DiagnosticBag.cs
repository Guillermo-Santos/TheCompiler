using System;
using System.Collections;
using System.Collections.Generic;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Text;

namespace SparkCore.Analytics.Diagnostics;

internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
{
    private readonly List<Diagnostic> _diagnostics = new();
    public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void AddRange(DiagnosticBag diagnostics)
    {
        _diagnostics.AddRange(diagnostics._diagnostics);
    }

    private void Report(TextSpan span, string message)
    {
        var diagnostic = new Diagnostic(span, message);
        _diagnostics.Add(diagnostic);
    }

    public void ReportInvalidNumber(TextSpan textSpan, string text, TypeSymbol type)
    {
        var message = $"The number {text} isn't valid {type}.";
        Report(textSpan, message);
    }

    public void ReportBadCharacter(int position, char character)
    {
        var span = new TextSpan(position, 1);
        var message = $"Bad character input: '{character}'.";
        Report(span, message);
    }

    public void ReportUnterminedString(TextSpan span)
    {
        var message = $"Undeterminated string literal.";
        Report(span, message);
    }

    public void ReportUnexpectedToken(TextSpan span, SyntaxKind CurrentType, SyntaxKind ExpectedType)
    {
        var message = $"Unexpected token <{CurrentType}>, expected <{ExpectedType}>.";
        Report(span, message);
    }

    public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, TypeSymbol operatorType)
    {
        var message = $"Unary operator '{operatorText}' is not defined for type '{operatorType}'.";
        Report(span, message);
    }

    public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
    {
        var message = $"Binary operator '{operatorText}' is not definded for types '{leftType}' and '{rightType}'.";
        Report(span, message);
    }

    public void ReportUndefinedName(TextSpan span, string name)
    {
        var message = $"Variable '{name}' doesn't exist.";
        Report(span, message);
    }
    public void ReportUndefinedFunction(TextSpan span, string name)
    {
        var message = $"Function '{name}' doesn't exist.";
        Report(span, message);
    }

    public void ReportUndefinedType(TextSpan span, string name)
    {
        var message = $"Type '{name}' doesn't exist.";
        Report(span, message);
    }

    internal void ReportParameterAlreadyDeclared(TextSpan span, string parameterName)
    {
        var message = $"Parameter with the name '{parameterName}' already exists.";
        Report(span, message);
    }

    public void ReportCannotConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
    {
        var message = $"Cannot convert type '{fromType}' to '{toType}'.";
        Report(span, message);
    }
    public void ReportCannotImplicitlyConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
    {
        var message = $"Cannot convert type '{fromType}' to '{toType}'. An explicit conversion exits (Are you missing a cast?)";
        Report(span, message);
    }

    public void ReportSymbolAlreadyDeclared(TextSpan span, string name)
    {
        var message = $"'{name}' is already declared.";
        Report(span, message);
    }

    internal void ReportCannotAssign(TextSpan span, string name)
    {
        var message = $"Variable '{name}' is read only and cannot be assigned to.";
        Report(span, message);
    }

    public void ReportWrongArgumentCount(TextSpan span, string name, int expectedCount, int actualCount)
    {
        var message = $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}.";
        Report(span, message);
    }

    public void ReportWrongArgumentType(TextSpan span, string name, TypeSymbol expectedType, TypeSymbol actualType)
    {
        var message = $"Parameter '{name}' requires a value of type {expectedType} but was given a value of type {actualType}.";
        Report(span, message);
    }

    public void ReportExpressionMustHaveValue(TextSpan span)
    {
        var message = "Expression must have a value, cannot be void.";
        Report(span, message);
    }

    public void ReportInvalidBreackOrContinue(TextSpan span, string text)
    {
        var message = $"The keyword '{text}' can only be used inside of loops.";
        Report(span, message);
    }

    public void ReportAllPathsMustReturn(TextSpan span)
    {
        var message = "Not all code paths return a value.";
        Report(span, message);
    }
    public void ReportInvalidReturn(TextSpan span)
    {
        var message = "The 'return' keyword can only be used insede of functions.";
        Report(span, message);
    }
    public void ReportInvalidReturnExpression(TextSpan span, string functionName)
    {
        var message = $"Since the function '{functionName}' does not return a value, the 'return' keyword cannot be followed by an expression.";
        Report(span, message);
    }
    public void ReportMissingReturnExpression(TextSpan span, TypeSymbol returnType)
    {
        var message = $"An expression of type '{returnType}' expected.";
        Report(span, message);
    }
}

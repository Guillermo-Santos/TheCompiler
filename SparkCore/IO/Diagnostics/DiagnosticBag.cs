using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax;
using SparkCore.IO.Text;

namespace SparkCore.IO.Diagnostics;

internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
{
    private readonly List<Diagnostic> _diagnostics = new();
    public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void AddRange(IEnumerable<Diagnostic> diagnostics)
    {
        _diagnostics.AddRange(diagnostics);
    }

    private void Report(TextLocation location, string message)
    {
        var diagnostic = new Diagnostic(location, message);
        _diagnostics.Add(diagnostic);
    }

    public void ReportInvalidNumber(TextLocation location, string text, TypeSymbol type)
    {
        var message = $"The number {text} isn't valid {type}.";
        Report(location, message);
    }

    public void ReportBadCharacter(TextLocation location, char character)
    {
        var message = $"Bad character input: '{character}'.";
        Report(location, message);
    }

    public void ReportUnterminedString(TextLocation location)
    {
        var message = $"Undeterminated string literal.";
        Report(location, message);
    }
    public void ReportUnterminedMultiLineComment(TextLocation location)
    {
        var message = $"Undeterminated multi-line comment.";
        Report(location, message);
    }
    public void ReportUnexpectedToken(TextLocation location, SyntaxKind CurrentType, SyntaxKind ExpectedType)
    {
        var message = $"Unexpected token <{CurrentType}>, expected <{ExpectedType}>.";
        Report(location, message);
    }

    public void ReportUndefinedUnaryOperator(TextLocation location, string operatorText, TypeSymbol operatorType)
    {
        var message = $"Unary operator '{operatorText}' is not defined for type '{operatorType}'.";
        Report(location, message);
    }

    public void ReportUndefinedBinaryOperator(TextLocation location, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
    {
        var message = $"Binary operator '{operatorText}' is not definded for types '{leftType}' and '{rightType}'.";
        Report(location, message);
    }

    public void ReportUndefinedVariable(TextLocation location, string name)
    {
        var message = $"Variable '{name}' doesn't exist.";
        Report(location, message);
    }
    public void ReportNotAVariable(TextLocation location, string name)
    {
        var message = $"'{name}' is not a variable.";
        Report(location, message);
    }

    public void ReportNotAFunction(TextLocation location, string name)
    {
        var message = $"'{name}' is not a function.";
        Report(location, message);
    }


    public void ReportUndefinedFunction(TextLocation location, string name)
    {
        var message = $"Function '{name}' doesn't exist.";
        Report(location, message);
    }

    public void ReportUndefinedType(TextLocation location, string name)
    {
        var message = $"Type '{name}' doesn't exist.";
        Report(location, message);
    }

    internal void ReportParameterAlreadyDeclared(TextLocation location, string parameterName)
    {
        var message = $"Parameter with the name '{parameterName}' already exists.";
        Report(location, message);
    }

    public void ReportCannotConvert(TextLocation location, TypeSymbol fromType, TypeSymbol toType)
    {
        var message = $"Cannot convert type '{fromType}' to '{toType}'.";
        Report(location, message);
    }
    public void ReportCannotImplicitlyConvert(TextLocation location, TypeSymbol fromType, TypeSymbol toType)
    {
        var message = $"Cannot convert type '{fromType}' to '{toType}'. An explicit conversion exits (Are you missing a cast?)";
        Report(location, message);
    }

    public void ReportSymbolAlreadyDeclared(TextLocation location, string name)
    {
        var message = $"'{name}' is already declared.";
        Report(location, message);
    }

    internal void ReportCannotAssign(TextLocation location, string name)
    {
        var message = $"Variable '{name}' is read only and cannot be assigned to.";
        Report(location, message);
    }

    public void ReportWrongArgumentCount(TextLocation location, string name, int expectedCount, int actualCount)
    {
        var message = $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}.";
        Report(location, message);
    }

    public void ReportExpressionMustHaveValue(TextLocation location)
    {
        var message = "Expression must have a value, cannot be void.";
        Report(location, message);
    }

    public void ReportInvalidBreackOrContinue(TextLocation location, string text)
    {
        var message = $"The keyword '{text}' can only be used inside of loops.";
        Report(location, message);
    }

    public void ReportAllPathsMustReturn(TextLocation location)
    {
        var message = "Not all code paths return a value.";
        Report(location, message);
    }
    public void ReportInvalidReturnExpression(TextLocation location, string functionName)
    {
        var message = $"Since the function '{functionName}' does not return a value, the 'return' keyword cannot be followed by an expression.";
        Report(location, message);
    }
    public void ReportMissingReturnExpression(TextLocation location, TypeSymbol returnType)
    {
        var message = $"An expression of type '{returnType}' is expected.";
        Report(location, message);
    }

    public void ReportInvalidWithValueInGlobalStatement(TextLocation location)
    {
        var message = "The 'return' keyword cannot be followed by an expession in global statements.";
        Report(location, message);
    }
    public void ReportInvalidExpressionStatement(TextLocation location)
    {
        // TODO: Add increment and decrement operators so the error message can be:
        // Only assignment, call, increment, and decrement expressions can be used as a statement.
        var message = "Only assignment and call expressions can be used as a statement.";
        Report(location, message);
    }
    public void ReportOnlyOneFileCanHaveGlobalStatements(TextLocation location)
    {
        var message = "At most one file can have global statements.";
        Report(location, message);
    }

    public void ReportMainMustHaveCorrectSignature(TextLocation location)
    {
        var message = "main must not take arguments and not return anything.";
        Report(location, message);
    }
    public void ReportCannotMixMainAndGlobalStatements(TextLocation location)
    {
        var message = "Cannot declare main function when global statements are used.";
        Report(location, message);
    }
    public void ReportInvalidReference(string path)
    {
        var message = $"The reference is not a valid .NET assembly: '{path}'";
        Report(default, message);
    }

    public void ReportRequiredTypeNotFound(string sparkName, string metadataName)
    {
        var message = sparkName == null
                    ? $"The required type '{metadataName}' cannot be resolved among the given references"
                    : $"The required type '{sparkName}' ('{metadataName}') cannot be resolved among the given references";
        Report(default, message);
    }
    public void ReportRequiredTypeAmbiguous(string sparkName, string metadataName, TypeDefinition[] foundTypes)
    {
        var assemblyNames = foundTypes.Select(t => t.Module.Assembly.Name.Name);
        var assemblyNameList = string.Join(", ", assemblyNames);


        var message = sparkName == null
                    ? $"The required type '{metadataName}' was found in multiple reference: {assemblyNameList}."
                    : $"The required type '{sparkName}' ('{metadataName}') was found in multiple reference: {assemblyNameList}.";
        Report(default, message);
    }

    public void ReportRequiredMethodNotFound(string typeName, string methodName, string[] parameterTypeNames)
    {
        var parameterTypeNameList = string.Join(", ", parameterTypeNames);
        var message = $"The required method '{typeName}.{methodName}({parameterTypeNameList})' cannot be resolved among the given references";
        Report(default, message);
    }

}

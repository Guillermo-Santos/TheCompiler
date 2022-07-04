using System;
using System.Collections;
using System.Collections.Generic;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Text;

namespace SparkCore.Analytics.Diagnostics
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();
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

        public void ReportCannotConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"Cannot convert type '{fromType}' to '{toType}'.";
            Report(span, message);
        }

        public void ReportVariableAlreadyDeclared(TextSpan span, string name)
        {
            var message = $"Variable '{name}' is already declared.";
            Report(span, message);
        }

        internal void ReportCannotAssign(TextSpan span, string name)
        {
            var message = $"Variable '{name}' is read only and cannot be assigned to.";
            Report(span, message);
        }
    }
}

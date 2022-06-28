using Compiler.Core.Syntax.Expressions;
using System.Collections.Generic;
using Compiler.Core.Diagnostics;
using Compiler.Core.Syntax.Lexic;
namespace Compiler.Core.Syntax
{
    internal sealed class SyntaxAnalyzer
    {
        private readonly SyntaxToken[] _tokens;
        private DiagnosticBag _diagnostics = new DiagnosticBag();
        private int _position;
        public DiagnosticBag Diagnostics => _diagnostics;
        public SyntaxAnalyzer(string text)
        {
            List<SyntaxToken> tokens = new List<SyntaxToken>();
            var lexer = new LexicAnalyzer(text);
            SyntaxToken token;
            do
            {
                token = lexer.NextToken();
                if (token.Type != SyntaxType.WhiteSpaceToken && token.Type != SyntaxType.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Type != SyntaxType.EndOfFileToken);
            _tokens = tokens.ToArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
                return _tokens[_tokens.Length - 1];
            if (index < 0)
                return _tokens[0];
            return _tokens[index];
        }
        private SyntaxToken Current => Peek(0);
        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }
        private SyntaxToken MathToken(SyntaxType type)
        {
            if (Current.Type == type)
                return NextToken();
            _diagnostics.ReportUnexpectedToken(Current.Span, Current.Type, type);
            return new SyntaxToken(type, Current.Position, null, null);
        }
        public SyntaxTree Parse()
        {
            var expression = ParseExpression();
            var endOfFileToken = MathToken(SyntaxType.EndOfFileToken);
            return new SyntaxTree(_diagnostics, expression, endOfFileToken);
        }
        private SyntaxExpression ParseExpression()
        {
            return ParseAssigmentExpression();
        }
        private SyntaxExpression ParseAssigmentExpression()
        {
            if (Peek(0).Type == SyntaxType.IdentifierToken &&
               Peek(1).Type == SyntaxType.EqualsToken)
            {
                var identifierToken = NextToken();
                var operatorToken = NextToken();
                var right = ParseAssigmentExpression();
                return new AssignmentSyntaxExpression(identifierToken, operatorToken, right);
            }

            return ParseBinaryExpression();
        }
        private SyntaxExpression ParseBinaryExpression(int parentPrecedence = 0)
        {
            SyntaxExpression left;
            var unaryOperatorPrecedence = Current.Type.GetBinaryOperatorPrecedence();

            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseBinaryExpression(unaryOperatorPrecedence);
                left = new UnarySyntaxExpression(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }
            while (true)
            {
                var precedence = Current.Type.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;
                var operatorToken = NextToken();
                var right = ParseBinaryExpression(precedence);
                left = new BinarySyntaxExpression(left, operatorToken, right);
            }
            return left;
        }

        private SyntaxExpression ParsePrimaryExpression()
        {
            switch (Current.Type)
            {
                case SyntaxType.OpenParentesisToken:
                    {
                        var left = NextToken();
                        var expression = ParseExpression();
                        var right = MathToken(SyntaxType.CloseParentesisToken);
                        return new ParenthesizedSyntaxExpression(left, expression, right);
                    }

                case SyntaxType.TrueKeyword:
                case SyntaxType.FalseKeyword:
                    {
                        var keywordToken = NextToken();
                        var value = keywordToken.Type == SyntaxType.TrueKeyword;
                        return new LiteralSyntaxExpression(keywordToken, value);
                    }
                case SyntaxType.IdentifierToken:
                    {
                        var identifierToken = NextToken();
                        return new NameSyntaxExpression(identifierToken);
                    }
                default:
                    var numberToken = MathToken(SyntaxType.NumberToken);
                    return new LiteralSyntaxExpression(numberToken);
            }
        }
    }
}

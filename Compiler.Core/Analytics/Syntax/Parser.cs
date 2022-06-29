using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Syntax.Expressions;
using SparkCore.Analytics.Syntax.Lexic;
using SparkCore.Analytics.Text;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SparkCore.Analytics.Syntax
{
    internal sealed class Parser
    {
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly SourceText _text;
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private int _position;
        public DiagnosticBag Diagnostics => _diagnostics;
        public Parser(SourceText text)
        {
            List<SyntaxToken> tokens = new List<SyntaxToken>();
            var lexer = new LexicAnalyzer(text);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();
                if (token.Type != SyntaxType.WhiteSpaceToken && token.Type != SyntaxType.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Type != SyntaxType.EndOfFileToken);
            _tokens = tokens.ToImmutableArray();
            _diagnostics.AddRange(lexer.Diagnostics);
            _text = text;
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
            return new SyntaxTree(_text, _diagnostics.ToImmutableArray(), expression, endOfFileToken);
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
            var unaryOperatorPrecedence = Current.Type.GetUnaryOperatorPrecedence();

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
                        return ParseParenthesizedExpression();
                case SyntaxType.TrueKeyword:
                case SyntaxType.FalseKeyword:
                        return ParseBooleanLiteralExpression();
                case SyntaxType.NumberToken:
                    return ParseNumberLiteralExpression();
                case SyntaxType.IdentifierToken:
                default:
                    return ParseNameExpression();
            }
        }

        private SyntaxExpression ParseNumberLiteralExpression()
        {
            var numberToken = MathToken(SyntaxType.NumberToken);
            return new LiteralSyntaxExpression(numberToken);
        }

        private SyntaxExpression ParseParenthesizedExpression()
        {
            var left = MathToken(SyntaxType.OpenParentesisToken);
            var expression = ParseExpression();
            var right = MathToken(SyntaxType.CloseParentesisToken);
            return new ParenthesizedSyntaxExpression(left, expression, right);
        }

        private SyntaxExpression ParseBooleanLiteralExpression()
        {
            var isTrue = Current.Type == SyntaxType.TrueKeyword;
            var keywordToken = isTrue ? MathToken(SyntaxType.TrueKeyword) : MathToken(SyntaxType.FalseKeyword);
            return new LiteralSyntaxExpression(keywordToken, isTrue);
        }

        private SyntaxExpression ParseNameExpression()
        {
            var identifierToken = MathToken(SyntaxType.IdentifierToken);
            return new NameSyntaxExpression(identifierToken);
        }
    }
}

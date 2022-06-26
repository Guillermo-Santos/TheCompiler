using Compiler.Core.Syntax.Expressions;
using Compiler.Core.Syntax;
using System.Collections.Generic;

namespace Compiler.Core.Analyzers
{
    internal sealed class SyntaxAnalyzer
    {
        private readonly SyntaxToken[] _tokens;
        private List<string> _diagnostics = new List<string>();
        private int _position;
        public IEnumerable<string> Diagnostics => _diagnostics;
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
            _diagnostics.Add($"ERROR: Unexpected token <{Current.Type}>, expectedd <{type}>");
            return new SyntaxToken(type, Current.Position, null, null);
        }
        public SyntaxTree Parse()
        {
            var expression = ParseExpression();
            var endOfFileToken = MathToken(SyntaxType.EndOfFileToken);
            return new SyntaxTree(_diagnostics, expression, endOfFileToken);
        }
        private SyntaxExpression ParseExpression(int parentPrecedence = 0)
        {
            SyntaxExpression left;
            var unaryOperatorPrecedence = Current.Type.GetBinaryOperatorPrecedence();

            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseExpression(unaryOperatorPrecedence);
                left = new UnarySyntaxExpression(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }
            while (true)
            {
                var precedence = Current.Type.GetBinaryOperatorPrecedence();
                if(precedence == 0 || precedence <= parentPrecedence)
                    break;
                var operatorToken = NextToken();
                var right = ParseExpression(precedence);
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
                        return new ParenthesisedExpressionSyntax(left, expression, right);
                    }

                case SyntaxType.TrueKeyword:
                case SyntaxType.FalseKeyword:
                    {
                        var keywordToken = NextToken();
                        var value = keywordToken.Type == SyntaxType.TrueKeyword;
                        return new LiteralSyntaxExpression(keywordToken, value);
                    }
                default:
                    var numberToken = MathToken(SyntaxType.NumberToken);
                    return new LiteralSyntaxExpression(numberToken);
            }
        }
    }
}

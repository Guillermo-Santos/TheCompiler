using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Syntax.Tree.Expressions;
using SparkCore.Analytics.Syntax.Lexic;
using SparkCore.Analytics.Syntax.Tree.Statements;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.Analytics.Text;
using System;
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

        

        public CompilationSyntaxUnit ParseCompilationUnit()
        {
            var statement = ParseStatement();
            var endOfFileToken = MathToken(SyntaxType.EndOfFileToken);
            return new CompilationSyntaxUnit(statement, endOfFileToken);
        }

        private SyntaxStatement ParseStatement()
        {
            switch (Current.Type)
            {
                case SyntaxType.OpenBraceToken:
                    return ParseBlockStatement();
                case SyntaxType.LetKeyword:
                case SyntaxType.VarKeyword:
                    return ParseVariableDeclaration();
            }
            return ParseExpressionStatement();
        }


        private SyntaxStatement ParseBlockStatement()
        {
            var statements = ImmutableArray.CreateBuilder<SyntaxStatement>();
            var openBraceToken = MathToken(SyntaxType.OpenBraceToken);
            while (Current.Type != SyntaxType.EndOfFileToken &&
                  Current.Type != SyntaxType.CloseBraceToken)
            {
                var statement = ParseStatement();
                statements.Add(statement);
            }
            var CloseBraceToken = MathToken(SyntaxType.CloseBraceToken);
            return new BlockSyntaxStatement(openBraceToken, statements.ToImmutable(), CloseBraceToken);
        }
        private SyntaxStatement ParseVariableDeclaration()
        {
            var expected = Current.Type == SyntaxType.LetKeyword ? SyntaxType.LetKeyword : SyntaxType.VarKeyword;
            var keyword = MathToken(expected);
            var identifier = MathToken(SyntaxType.IdentifierToken);
            var equals = MathToken(SyntaxType.EqualsToken);
            var initializer = ParseExpression();

            return new VariableDeclarationSyntaxStatement(keyword, identifier, equals, initializer);
        }
        private SyntaxStatement ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new ExpressionSyntaxStatement(expression);
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

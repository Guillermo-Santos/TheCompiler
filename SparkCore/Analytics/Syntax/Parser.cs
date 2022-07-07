﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Syntax.Lexic;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.Analytics.Syntax.Tree.Expressions;
using SparkCore.Analytics.Syntax.Tree.Nodes;
using SparkCore.Analytics.Syntax.Tree.Statements;
using SparkCore.Analytics.Text;

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
            var tokens = new List<SyntaxToken>();
            var lexer = new LexicAnalyzer(text);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();
                if (token.Kind != SyntaxKind.WhiteSpaceToken && token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);
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
        private SyntaxToken MatchToken(SyntaxKind type)
        {
            if (Current.Kind == type)
                return NextToken();
            _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, type);
            return new SyntaxToken(type, Current.Position, null, null);
        }



        public CompilationSyntaxUnit ParseCompilationUnit()
        {
            var statement = ParseStatement();
            var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new CompilationSyntaxUnit(statement, endOfFileToken);
        }

        private SyntaxStatement ParseStatement()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenBraceToken:
                    return ParseBlockStatement();
                case SyntaxKind.LetKeyword:
                case SyntaxKind.VarKeyword:
                    return ParseVariableDeclaration();
                case SyntaxKind.IfKeyword:
                    return ParseIfStatement();
                case SyntaxKind.WhileKeyword:
                    return ParseWhileStatement();
                case SyntaxKind.ForKeyword:
                    return ParseForStatement();
            }
            return ParseExpressionStatement();
        }

        private SyntaxStatement ParseBlockStatement()
        {
            var statements = ImmutableArray.CreateBuilder<SyntaxStatement>();
            var openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);
            var startToken = Current;
            while (Current.Kind != SyntaxKind.EndOfFileToken &&
                  Current.Kind != SyntaxKind.CloseBraceToken)
            {
                var statement = ParseStatement();
                statements.Add(statement);
                // If ParseStatement() did not consume any tokens,
                // let's skip the current token and continue
                // in order to avoid an infinite loop.
                //
                // We don't nned to report error, because we'll
                // already tried to parse an expression statement
                // and repored one.
                if (Current == startToken)
                {
                    NextToken();
                }

            }
            var CloseBraceToken = MatchToken(SyntaxKind.CloseBraceToken);
            return new BlockSyntaxStatement(openBraceToken, statements.ToImmutable(), CloseBraceToken);
        }
        private SyntaxStatement ParseVariableDeclaration()
        {
            var expected = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
            var keyword = MatchToken(expected);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var equals = MatchToken(SyntaxKind.EqualsToken);
            var initializer = ParseExpression();

            return new VariableDeclarationSyntaxStatement(keyword, identifier, equals, initializer);
        }

        private SyntaxStatement ParseIfStatement()
        {
            var keyword = MatchToken(SyntaxKind.IfKeyword);
            var condition = ParseExpression();
            var statement = ParseStatement();
            var elseClause = ParseElseClause();

            return new IfSyntaxStatement(keyword, condition, statement, elseClause);
        }

        private ElseClauseSyntax ParseElseClause()
        {
            if (Current.Kind != SyntaxKind.ElseKeyword)
                return null;
            var keyword = NextToken();
            var statement = ParseStatement();

            return new ElseClauseSyntax(keyword, statement);
        }
        private SyntaxStatement ParseWhileStatement()
        {
            var keyword = MatchToken(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();

            return new WhileSyntaxStatement(keyword, condition, body);
        }

        private SyntaxStatement ParseForStatement()
        {
            var keyword = MatchToken(SyntaxKind.ForKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var equalstoken = MatchToken(SyntaxKind.EqualsToken);
            var lowerBound = ParseExpression();
            var toKeyword = MatchToken(SyntaxKind.ToKeyword);
            var upperBound = ParseExpression();
            var body = ParseStatement();
            return new ForSyntaxStatement(keyword, identifier, equalstoken, lowerBound, toKeyword, upperBound, body);
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
            if (Peek(0).Kind == SyntaxKind.IdentifierToken &&
               Peek(1).Kind == SyntaxKind.EqualsToken)
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
            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();

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
                var precedence = Current.Kind.GetBinaryOperatorPrecedence();
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
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParentesisToken:
                    return ParseParenthesizedExpression();
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                    return ParseBooleanLiteral();
                case SyntaxKind.NumberToken:
                    return ParseNumberLiteral();
                case SyntaxKind.StringToken:
                    return ParseStringLiteral();
                case SyntaxKind.IdentifierToken:
                default:
                    return ParseNameOrCallExpression();
            }
        }


        private SyntaxExpression ParseParenthesizedExpression()
        {
            var left = MatchToken(SyntaxKind.OpenParentesisToken);
            var expression = ParseExpression();
            var right = MatchToken(SyntaxKind.CloseParentesisToken);
            return new ParenthesizedSyntaxExpression(left, expression, right);
        }

        private SyntaxExpression ParseBooleanLiteral()
        {
            var isTrue = Current.Kind == SyntaxKind.TrueKeyword;
            var keywordToken = isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
            return new LiteralSyntaxExpression(keywordToken, isTrue);
        }
        private SyntaxExpression ParseNumberLiteral()
        {
            var numberToken = MatchToken(SyntaxKind.NumberToken);
            return new LiteralSyntaxExpression(numberToken);
        }

        private SyntaxExpression ParseStringLiteral()
        {
            var stringToken = MatchToken(SyntaxKind.StringToken);
            return new LiteralSyntaxExpression(stringToken);
        }

        private SyntaxExpression ParseNameOrCallExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.OpenParentesisToken)
                return ParseCallExpression();
            return ParseNameExpression();
        }
        private SyntaxExpression ParseCallExpression()
        {
            var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
            var openParentesis = MatchToken(SyntaxKind.OpenParentesisToken);
            var arguments = ParseArguments();
            var closeParentesis = MatchToken(SyntaxKind.CloseParentesisToken);

            return new CallSyntaxExpression(identifierToken, openParentesis, arguments, closeParentesis);
        }

        private SeparatedSyntaxList<SyntaxExpression> ParseArguments()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            while (Current.Kind != SyntaxKind.CloseParentesisToken &&
                   Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var expression = ParseExpression();
                nodesAndSeparators.Add(expression);
                if (Current.Kind != SyntaxKind.CloseParentesisToken)
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }

            }
            return new SeparatedSyntaxList<SyntaxExpression>(nodesAndSeparators.ToImmutable());
        }

        private SyntaxExpression ParseNameExpression()
        {
            var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
            return new NameSyntaxExpression(identifierToken);
        }
    }
}

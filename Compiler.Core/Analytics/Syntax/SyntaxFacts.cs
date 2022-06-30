using System;
using System.Collections.Generic;

namespace SparkCore.Analytics.Syntax
{
    /// <summary>
    /// This class is, basically, the unchangable rules of the syntax of the lenguage,
    /// Used to call and confirms this facts.
    /// </summary>
    public static class SyntaxFacts
    {
        /// <summary>
        /// Get the precedence level of an unary operator.
        /// </summary>
        /// <param name="type">Represent the token of the binary operator.</param>
        /// <returns>
        ///     An int value that represent the level of precedence of the operator,
        ///     Or 0 if the Operator is unregistered as a unary operator.
        /// </returns>
        public static int GetUnaryOperatorPrecedence(this SyntaxType type)
        {
            switch (type)
            {
                case SyntaxType.PlusToken:
                case SyntaxType.MinusToken:
                case SyntaxType.BangToken:
                    return 6;
                default:
                    return 0;
            }
        }
        /// <summary>
        /// Get the precedence level of a binary operator.
        /// </summary>
        /// <param name="type">Represent the token of the binary operator.</param>
        /// <returns>
        ///     An int value that represent the level of precedence of the operator,
        ///     Or 0 if the Operator is unregistered as a binary operator.
        /// </returns>
        public static int GetBinaryOperatorPrecedence(this SyntaxType type)
        {
            switch (type)
            {
                case SyntaxType.StarToken:
                case SyntaxType.SlashToken:
                    return 5;
                case SyntaxType.PlusToken:
                case SyntaxType.MinusToken:
                    return 4;
                case SyntaxType.EqualsEqualsToken:
                case SyntaxType.BangEqualsToken:
                    return 3;
                case SyntaxType.AmpersandAmpersandToken:
                    return 2;
                case SyntaxType.PibePibeToken:
                    return 1;
                default:
                    return 0;
            }
        }

        public static SyntaxType GetKeywordType(string text)
        {
            switch (text)
            {
                case "false":
                    return SyntaxType.FalseKeyword;
                case "let":
                    return SyntaxType.LetKeyword;
                case "true":
                    return SyntaxType.TrueKeyword;
                case "var":
                    return SyntaxType.VarKeyword;
                default:
                    return SyntaxType.IdentifierToken;
            }
        }
        public static IEnumerable<SyntaxType> GetUnaryOperatorTypes()
        {
            var types = (SyntaxType[])Enum.GetValues(typeof(SyntaxType));
            foreach (var type in types)
            {
                if (GetUnaryOperatorPrecedence(type) > 0)
                    yield return type;
            }
        }
        public static IEnumerable<SyntaxType> GetBinaryOperatorTypes()
        {
            var types = (SyntaxType[])Enum.GetValues(typeof(SyntaxType));
            foreach (var type in types)
            {
                if (GetBinaryOperatorPrecedence(type) > 0)
                    yield return type;
            }
        }
        public static string GetText(SyntaxType type)
        {
            switch (type)
            {
                case SyntaxType.PlusToken:
                    return "+";
                case SyntaxType.MinusToken:
                    return "-";
                case SyntaxType.StarToken:
                    return "*";
                case SyntaxType.SlashToken:
                    return "/";
                case SyntaxType.OpenParentesisToken:
                    return "(";
                case SyntaxType.CloseParentesisToken:
                    return ")";
                case SyntaxType.OpenBraceToken:
                    return "{";
                case SyntaxType.CloseBraceToken:
                    return "}";
                case SyntaxType.BangToken:
                    return "!";
                case SyntaxType.EqualsToken:
                    return "=";
                case SyntaxType.AmpersandAmpersandToken:
                    return "&&";
                case SyntaxType.PibePibeToken:
                    return "||";
                case SyntaxType.BangEqualsToken:
                    return "!=";
                case SyntaxType.EqualsEqualsToken:
                    return "==";
                case SyntaxType.FalseKeyword:
                    return "false";
                case SyntaxType.LetKeyword:
                    return "let";
                case SyntaxType.TrueKeyword:
                    return "true";
                case SyntaxType.VarKeyword:
                    return "var";
                default:
                    return null;
            }
        }
    }

}

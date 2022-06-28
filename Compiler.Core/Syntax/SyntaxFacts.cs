using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core.Syntax
{
    internal static class SyntaxFacts
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
                case "true":
                    return SyntaxType.TrueKeyword;
                case "false":
                    return SyntaxType.FalseKeyword;
                default:
                    return SyntaxType.IdentifierToken;
            }
        }
    }
}

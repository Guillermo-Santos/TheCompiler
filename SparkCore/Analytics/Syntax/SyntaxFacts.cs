using System;
using System.Collections.Generic;

namespace SparkCore.Analytics.Syntax;

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
    public static int GetUnaryOperatorPrecedence(this SyntaxKind type)
    {
        switch (type)
        {
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
            case SyntaxKind.BangToken:
            case SyntaxKind.TildeToken:
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
    public static int GetBinaryOperatorPrecedence(this SyntaxKind type)
    {
        switch (type)
        {
            case SyntaxKind.StarToken:
            case SyntaxKind.SlashToken:
                return 5;
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
                return 4;
            case SyntaxKind.EqualsEqualsToken:
            case SyntaxKind.BangEqualsToken:
            case SyntaxKind.LessToken:
            case SyntaxKind.LessOrEqualsToken:
            case SyntaxKind.GreaterToken:
            case SyntaxKind.GreaterOrEqualsToken:
                return 3;
            case SyntaxKind.AmpersandToken:
            case SyntaxKind.AmpersandAmpersandToken:
                return 2;
            case SyntaxKind.PibeToken:
            case SyntaxKind.PibePibeToken:
            case SyntaxKind.HatToken:
                return 1;
            default:
                return 0;
        }
    }
    /// <summary>
    /// Get an keyword token or identifierToken, depending of if the string passed is a 
    /// registered keyword or not.
    /// </summary>
    /// <param name="text">The string to get the keyword from</param>
    /// <returns></returns>
    public static SyntaxKind GetKeywordType(string text)
    {
        switch (text)
        {
            case "break":
                return SyntaxKind.BreakKeyword;
            case "continue":
                return SyntaxKind.ContinueKeyword;
            case "else":
                return SyntaxKind.ElseKeyword;
            case "false":
                return SyntaxKind.FalseKeyword;
            case "for":
                return SyntaxKind.ForKeyword;
            case "function":
                return SyntaxKind.FunctionKeyword;
            case "if":
                return SyntaxKind.IfKeyword;
            case "let":
                return SyntaxKind.LetKeyword;
            case "return":
                return SyntaxKind.ReturnKeyword;
            case "to":
                return SyntaxKind.ToKeyword;
            case "true":
                return SyntaxKind.TrueKeyword;
            case "var":
                return SyntaxKind.VarKeyword;
            case "do":
                return SyntaxKind.DoKeyword;
            case "while":
                return SyntaxKind.WhileKeyword;
            default:
                return SyntaxKind.IdentifierToken;
        }
    }
    public static IEnumerable<SyntaxKind> GetUnaryOperatorTypes()
    {
        var types = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
        foreach (var type in types)
        {
            if (GetUnaryOperatorPrecedence(type) > 0)
                yield return type;
        }
    }
    public static IEnumerable<SyntaxKind> GetBinaryOperatorTypes()
    {
        var types = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
        foreach (var type in types)
        {
            if (GetBinaryOperatorPrecedence(type) > 0)
                yield return type;
        }
    }
    public static string GetText(SyntaxKind type)
    {
        switch (type)
        {
            case SyntaxKind.PlusToken:
                return "+";
            case SyntaxKind.MinusToken:
                return "-";
            case SyntaxKind.StarToken:
                return "*";
            case SyntaxKind.SlashToken:
                return "/";
            case SyntaxKind.OpenParentesisToken:
                return "(";
            case SyntaxKind.CloseParentesisToken:
                return ")";
            case SyntaxKind.OpenBraceToken:
                return "{";
            case SyntaxKind.CloseBraceToken:
                return "}";
            case SyntaxKind.ColonToken:
                return ":";
            case SyntaxKind.CommaToken:
                return ",";
            case SyntaxKind.BangToken:
                return "!";
            case SyntaxKind.EqualsToken:
                return "=";
            case SyntaxKind.TildeToken:
                return "~";
            case SyntaxKind.LessToken:
                return "<";
            case SyntaxKind.LessOrEqualsToken:
                return "<=";
            case SyntaxKind.GreaterToken:
                return ">";
            case SyntaxKind.GreaterOrEqualsToken:
                return ">=";
            case SyntaxKind.AmpersandToken:
                return "&";
            case SyntaxKind.AmpersandAmpersandToken:
                return "&&";
            case SyntaxKind.PibeToken:
                return "|";
            case SyntaxKind.PibePibeToken:
                return "||";
            case SyntaxKind.HatToken:
                return "^";
            case SyntaxKind.BangEqualsToken:
                return "!=";
            case SyntaxKind.EqualsEqualsToken:
                return "==";
            case SyntaxKind.BreakKeyword:
                return "break";
            case SyntaxKind.ContinueKeyword:
                return "continue";
            case SyntaxKind.ElseKeyword:
                return "else";
            case SyntaxKind.FalseKeyword:
                return "false";
            case SyntaxKind.ForKeyword:
                return "for";
            case SyntaxKind.FunctionKeyword:
                return "function";
            case SyntaxKind.IfKeyword:
                return "if";
            case SyntaxKind.LetKeyword:
                return "let";
            case SyntaxKind.ToKeyword:
                return "to";
            case SyntaxKind.ReturnKeyword:
                return "return";
            case SyntaxKind.TrueKeyword:
                return "true";
            case SyntaxKind.VarKeyword:
                return "var";
            case SyntaxKind.DoKeyword:
                return "do";
            case SyntaxKind.WhileKeyword:
                return "while";
            default:
                return null;
        }
    }
    public static bool IsTrivia(this SyntaxKind kind)
    {
        switch (kind)
        {
            case SyntaxKind.BadTokenTrivia:
            case SyntaxKind.WhiteSpaceTrivia:
            case SyntaxKind.SingleLineCommentTrivia:
            case SyntaxKind.MultiLineCommentTrivia:
                return true;
            default:
                return false;
        }
    }
    public static bool IsKeyWord(this SyntaxKind kind)
    {
        return kind.ToString().EndsWith("Keyword");
    }
    public static bool IsToken(this SyntaxKind kind)
    {
        return !kind.IsTrivia() &&
               (kind.IsKeyWord() || kind.ToString().EndsWith("Token"));
    }
}


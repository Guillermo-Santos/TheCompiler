using System.Collections.Immutable;
using Forge.Core.Models;
using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.IO.Text;

namespace Forge.Core.Helpers;

public sealed class Classifier
{
    public static ImmutableArray<ClassifiedSpan> Classify(SyntaxTree syntaxTree, TextSpan span)
    {
        var result = ImmutableArray.CreateBuilder<ClassifiedSpan>();
        ClassifyNode(syntaxTree.Root, span, result);
        return result.ToImmutable();
    }

    private static void ClassifyNode(SyntaxNode node, TextSpan span, ImmutableArray<ClassifiedSpan>.Builder result)
    {
        // HACK: node should never be null
        if (node == null || !node.FullSpan.OverlapsWith(span))
            return;
        if (node is SyntaxToken token)
        {
            ClassifyToken(token, span, result);
        }
        foreach (var child in node.GetChildren())
        {
            ClassifyNode(child, span, result);
        }
    }

    private static void ClassifyToken(SyntaxToken token, TextSpan span, ImmutableArray<ClassifiedSpan>.Builder result)
    {
        foreach (var leadingTrivia in token.LeadingTrivia)
        {
            ClassifyTrivia(leadingTrivia, span, result);
        }

        AddClassification(token.Kind, token.Span, span, result);

        foreach (var trailingTrivia in token.TrailingTrivia)
        {
            ClassifyTrivia(trailingTrivia, span, result);
        }
    }

    private static void ClassifyTrivia(SyntaxTrivia trivia, TextSpan span, ImmutableArray<ClassifiedSpan>.Builder result)
    {
        AddClassification(trivia.Kind, trivia.Span, span, result);
    }

    private static void AddClassification(SyntaxKind elementKind, TextSpan elementSpan, TextSpan span, ImmutableArray<ClassifiedSpan>.Builder result)
    {
        if (!elementSpan.OverlapsWith(span))
            return;
        var adjustedStart = Math.Max(elementSpan.Start, span.Start);
        var adjustedEnd = Math.Min(elementSpan.End, span.End);
        var adjustedSpan = TextSpan.FromBounds(adjustedStart, adjustedEnd);
        var classification = GetClassification(elementKind);

        var clasifiedSpan = new ClassifiedSpan(adjustedSpan, classification);
        result.Add(clasifiedSpan);
    }

    private static Classification GetClassification(SyntaxKind elementKind)
    {
        return elementKind switch
        {
            SyntaxKind.IdentifierToken
                => Classification.Identifier,
            SyntaxKind.NumberToken
                => Classification.Number,
            SyntaxKind.StringToken
                => Classification.String,
            SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia
                => Classification.Comment,
            _ => elementKind.IsKeyWord()
                    ? Classification.Keyword
                : elementKind.IsOperator()
                    ? Classification.Operator
                    : Classification.Text
        };
    }
}

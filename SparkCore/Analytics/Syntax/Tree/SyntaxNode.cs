using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SparkCore.Analytics.Syntax.Tree.Expressions;
using SparkCore.IO.Text;

namespace SparkCore.Analytics.Syntax.Tree;

public abstract class SyntaxNode
{
    protected SyntaxNode(SyntaxTree syntaxTree)
    {
        SyntaxTree = syntaxTree;
    }
    public SyntaxTree SyntaxTree
    {
        get;
    }
    public abstract SyntaxKind Kind
    {
        get;
    }
    public virtual TextSpan Span
    {
        get
        {
            var first = GetChildren().First().Span;
            var last = GetChildren().Last().Span;
            return TextSpan.FromBounds(first.Start, last.End);
        }
    }
    public virtual TextSpan FullSpan
    {
        get
        {
            var first = GetChildren().First().FullSpan;
            var last = GetChildren().Where(child => child != null).Last().FullSpan;
            return TextSpan.FromBounds(first.Start, last.End);
        }
    }

    public TextLocation Location => new(SyntaxTree.Text, Span);

    public SyntaxToken GetLastToken()
    {
        if (this is SyntaxToken token)
            return token;
        return GetChildren().Last().GetLastToken();
    }
    public abstract IEnumerable<SyntaxNode> GetChildren();
    public void WriteTo(TextWriter writter)
    {
        PrettyPrint(writter, this);
    }
    private static void PrettyPrint(TextWriter writter, SyntaxNode node, string indent = "", bool isLast = true)
    {
        if (node == null)
            return;

        var isToConsole = writter == Console.Out;
        var token = node as SyntaxToken;

        if (token != null)
        {
            foreach (var trivia in token.LeadingTrivia)
            {
                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                writter.Write(indent);
                writter.Write("├──");
                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.DarkGreen;

                writter.WriteLine($"L: {trivia.Kind}");
            }
        }

        var hasTrailingTrivia = token != null && token.TrailingTrivia.Any();
        var tokenMarker = !hasTrailingTrivia && isLast ? "└──" : "├──";

        if (isToConsole)
            Console.ForegroundColor = ConsoleColor.DarkGray;
        
        writter.Write(indent);
        writter.Write(tokenMarker);

        if (isToConsole)
            Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan;

        writter.Write(node.Kind);

        if (token != null && token.Value != null)
        {
            writter.Write(" ");
            writter.Write(token.Value);
        }

        writter.WriteLine();
        
        if (token != null)
        {
            foreach (var trivia in token.TrailingTrivia)
            {
                var isLastTrailingTrivia = trivia == token.TrailingTrivia.Last();
                var triviaMarker = isLast && isLastTrailingTrivia ? "└──" : "├──";

                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                writter.Write(indent);
                writter.Write(triviaMarker);
                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.DarkGreen;

                writter.WriteLine($"T: {trivia.Kind}");
            }
        }
        if (isToConsole)
            Console.ResetColor();

        indent += isLast ? "   " : "│  ";
        var lastChild = node.GetChildren().LastOrDefault();
        foreach (var child in node.GetChildren())
            PrettyPrint(writter, child, indent, child == lastChild);
    }

    public override string ToString()
    {
        using (var writter = new StringWriter())
        {
            WriteTo(writter);
            return writter.ToString();
        }
    }

}

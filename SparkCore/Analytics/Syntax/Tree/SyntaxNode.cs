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
        var isToConsole = writter == Console.Out;
        var marker = isLast ? "└──" : "├──";


        if (isToConsole)
            Console.ForegroundColor = ConsoleColor.DarkGray;
        writter.Write(indent);
        writter.Write(marker);

        if (isToConsole)
            Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan;

        writter.Write(node.Kind);

        if (node is SyntaxToken t && t.Value != null)
        {
            writter.Write(" ");
            writter.Write(t.Value);
        }

        if (isToConsole)
            Console.ResetColor();


        writter.WriteLine();

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

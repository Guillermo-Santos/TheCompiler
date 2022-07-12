using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.IO.Text;

namespace SparkCore.IO;
public static class TextWriterExtensions
{
    private static bool IsConsoleOut(this TextWriter writter)
    {
        if (writter == Console.Out)
            return true;
        if(writter is IndentedTextWriter iw && iw.InnerWriter.IsConsoleOut())
            return true;
        return false;
    }
    private static void SetForeground(this TextWriter writer, ConsoleColor color)
    {
        if(writer.IsConsoleOut())
            Console.ForegroundColor = color;
    }
    private static void ResetColor(this TextWriter writer)
    {
        if (writer.IsConsoleOut())
            Console.ResetColor();
    }
    public static void WriteKeyword(this TextWriter writer, SyntaxKind kind)
    {
        writer.WriteKeyword(SyntaxFacts.GetText(kind));
    }
    public static void WriteKeyword(this TextWriter writer, string test)
    {
        writer.SetForeground(ConsoleColor.Blue);
        writer.Write(test);
        writer.ResetColor();
    }
    public static void WriteIdentifier(this TextWriter writer, string test)
    {
        writer.SetForeground(ConsoleColor.Cyan);
        writer.Write(test);
        writer.ResetColor();
    }
    public static void WriteNumber(this TextWriter writer, string test)
    {
        writer.SetForeground(ConsoleColor.DarkYellow);
        writer.Write(test);
        writer.ResetColor();
    }
    public static void WriteString(this TextWriter writer, string test)
    {
        writer.SetForeground(ConsoleColor.Magenta);
        writer.Write(test);
        writer.ResetColor();
    }
    public static void WriteSpace(this TextWriter writer)
    {
        writer.Write(" ");
    }
    public static void WritePunctuation(this TextWriter writer, SyntaxKind kind)
    {
        writer.WritePunctuation(SyntaxFacts.GetText(kind));
    }
    public static void WritePunctuation(this TextWriter writer, string test)
    {
        writer.SetForeground(ConsoleColor.DarkGray);
        writer.Write(test);
        writer.ResetColor();
    }

    public static void WriteDiagnostics(this TextWriter writer, IEnumerable<Diagnostic> diagnostics, SyntaxTree syntaxTree)
    {
        foreach (var diagnostic in diagnostics.OrderBy(diag => diag.Span.Start).ThenBy(diag => diag.Span.Length))
        {
            var lineIndex = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
            var line = syntaxTree.Text.Lines[lineIndex];
            var lineNumber = lineIndex + 1;
            var character = diagnostic.Span.Start - line.Start + 1;

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write($"({lineNumber}, {character}): ");
            Console.WriteLine(diagnostic);
            Console.ResetColor();

            var prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
            var suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

            var prefix = syntaxTree.Text.ToString(prefixSpan);
            var error = syntaxTree.Text.ToString(diagnostic.Span);
            var suffix = syntaxTree.Text.ToString(suffixSpan);

            Console.Write("    ");
            Console.Write(prefix);

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(error);
            Console.ResetColor();

            Console.Write(suffix);
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}

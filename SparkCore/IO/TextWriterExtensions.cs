using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.IO.Diagnostics;
using SparkCore.IO.Text;

namespace SparkCore.IO;
public static class TextWriterExtensions
{
    private static bool IsConsole(this TextWriter writter)
    {
        if (writter == Console.Out)
            return !Console.IsOutputRedirected;

        if (writter == Console.Error)
            return !Console.IsErrorRedirected && !Console.IsOutputRedirected;

        if(writter is IndentedTextWriter iw && iw.InnerWriter.IsConsole())
            return true;
        return false;
    }
    private static void SetForeground(this TextWriter writer, ConsoleColor color)
    {
        if(writer.IsConsole())
            Console.ForegroundColor = color;
    }
    private static void ResetColor(this TextWriter writer)
    {
        if (writer.IsConsole())
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

    public static void WriteDiagnostics(this TextWriter writer, IEnumerable<Diagnostic> diagnostics)
    {
        foreach(var diagnostic in diagnostics.Where(d => d.Location.Text == null))
        {
            writer.SetForeground(ConsoleColor.DarkRed);
            writer.Write(diagnostic.Message);
            writer.ResetColor();
        }


        foreach (var diagnostic in diagnostics.Where(d => d.Location.Text != null)
                                              .OrderBy(d => d.Location.FileName)
                                              .ThenBy(d => d.Location.Span.Start)
                                              .ThenBy(d => d.Location.Span.Length))
        {
            var text = diagnostic.Location.Text;
            var fileName = diagnostic.Location.FileName;
            var startLine = diagnostic.Location.StartLine + 1;
            var startChar = diagnostic.Location.StartCharacter + 1;
            var endLine = diagnostic.Location.EndLine + 1;
            var endChar = diagnostic.Location.EndCharacter + 1;
            
            var Span = diagnostic.Location.Span;
            var lineIndex = text.GetLineIndex(Span.Start);
            var line = text.Lines[lineIndex];

            writer.WriteLine();

            writer.SetForeground(ConsoleColor.DarkRed);
            writer.Write($"{fileName}({startLine},{startChar},{endLine},{endChar}): ");
            writer.WriteLine(diagnostic);
            writer.ResetColor();

            var prefixSpan = TextSpan.FromBounds(line.Start, Span.Start);
            var suffixSpan = TextSpan.FromBounds(Span.End, line.End);

            var prefix = text.ToString(prefixSpan);
            var error = text.ToString(Span);
            var suffix = text.ToString(suffixSpan);

            writer.Write("    ");
            writer.Write(prefix);

            writer.SetForeground(ConsoleColor.DarkRed);
            writer.Write(error);
            writer.ResetColor();

            writer.Write(suffix);
            writer.WriteLine();
        }
        writer.WriteLine();
    }
}

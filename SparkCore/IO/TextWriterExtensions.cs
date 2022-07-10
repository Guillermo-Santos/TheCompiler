using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SparkCore.Analytics.Syntax;

namespace SparkCore.IO;
internal static class TextWriterExtensions
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
}

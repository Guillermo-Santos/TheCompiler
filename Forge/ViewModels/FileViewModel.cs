using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SparkCore;
using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.IO;
using SparkCore.IO.Diagnostics;
using SparkCore.IO.Text;
using Windows.UI;
using Windows.UI.WebUI;

namespace Forge.ViewModels;
public class FileViewModel : BaseViewModel
{
    private string _fileName;
    private string _text;
    private List<Diagnostic> diagnostics = new();

    private ObservableCollection<string> symbols = new();
    public List<Diagnostic> Diagnostics
    {
        get => diagnostics;
        set => SetProperty(ref diagnostics, value);
    }
    public ObservableCollection<string> Symbols
    {
        get => symbols;
        set => SetProperty(ref symbols, value);
    }
    public string FileName
    {
        get => _fileName;
        set => SetProperty(ref _fileName, value);
    }

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }
    public FileViewModel()
    {
        FileName = "default";
        Text = string.Empty;
    }
    public FileViewModel(string fileName, string text)
    {
        FileName = fileName;
        Text = text;
    }

    public void DoTextHightLighting(RichEditBox sender, string plainText, ITextRange position)//, RichEditBox tokenText, RichEditBox syntaxTreeText, RichEditBox intermText)
    {
        var document = sender.Document;
        var oLines = Text.Split("\r");
        var nLines = plainText.Split("\r").Where(dl => !(string.IsNullOrEmpty(dl) || string.IsNullOrWhiteSpace(dl)));

        var l = nLines.Except(oLines).ToList();

        var sourceText = SourceText.From(plainText);
        var lines = sourceText.Lines.Select(a => a).Where(a => l.Contains(a.ToString())).ToImmutableArray();

        var tokens = SyntaxTree.ParseTokens(sourceText);
        PaintTokens(document, tokens, lines);

        document.Selection.SetRange(position.StartPosition, position.EndPosition);
        Text = plainText;
        needErrorCheck = true;
    }
    private bool needErrorCheck = false;
    public void CheckErrors(RichEditBox sender, string plainText, ITextRange position)
    {
        if (!needErrorCheck)
            return;
        IsBusy = true;
        var document = sender.Document;
        var sourceText = SourceText.From(plainText);
        var syntaxTree = SyntaxTree.Parse(plainText);
        var compilation = Compilation.Create(syntaxTree);
        var result = compilation.Evaluate(new());
        var oldDiagnostics = Diagnostics;
        var newDiagnostics = result.Diagnostics;
        ImmutableArray<string> GetLines(int startLine, int endLine)
        {
            var lines = ImmutableArray.CreateBuilder<string>();
            for (var i = startLine; i <= endLine; i++)
            {
                if (i > sourceText.Lines.Count() - 1)
                    break;
                lines.Add(sourceText.Lines[i].ToString());
            }
            return lines.ToImmutable();
        }
        ImmutableArray<TextLine> GetTextLines(int startLine, int endLine)
        {
            var lines = ImmutableArray.CreateBuilder<TextLine>();
            for (var i = startLine; i <= endLine; i++)
            {
                if (i > sourceText.Lines.Count() - 1)
                    break;
                lines.Add(sourceText.Lines[i]);
            }
            return lines.ToImmutable();
        }
        List<TextLine> tlines = new();
        foreach (var diagnostic in Diagnostics)
        {
            var startLine = diagnostic.Location.StartLine;
            var endLine = diagnostic.Location.EndLine;
            var rlines = GetLines(startLine, endLine);
            var diag = result.Diagnostics.Where(rd => GetLines(rd.Location.StartLine, rd.Location.EndLine).Equals(rlines));
            if (!diag.Any())
                tlines.AddRange(GetTextLines(startLine, endLine));
        }
        var tokens = SyntaxTree.ParseTokens(sourceText);
        PaintTokens(document,tokens, tlines.ToImmutableArray());
        Diagnostics.Clear();
        ShowDiagnostics(result.Diagnostics, sender);
        Diagnostics = result.Diagnostics.ToList();


        document.Selection.SetRange(position.StartPosition, position.EndPosition);
        needErrorCheck = false;
    }

    private static void PaintTokens(RichEditTextDocument document, ImmutableArray<TextLine> lines)
    {
        var tokens = ImmutableArray.CreateBuilder<SyntaxToken>();
        foreach (var line in lines)
        {
            tokens.AddRange(SyntaxTree.ParseTokens(lines[0].Text));
        }
        PaintTokens(document, tokens.ToImmutable(), lines);
    }
    private static void PaintTokens(RichEditTextDocument document, ImmutableArray<SyntaxToken> tokens, ImmutableArray<TextLine> lines)
    {
        foreach (var token in tokens)
        {
            var line = lines.Where(x => x.Span.OverlapsWith(token.Span)).FirstOrDefault();
            if (line == null)
                continue;

            var tokenStart = Math.Max(token.Span.Start, line.Span.Start);
            var tokenEnd = Math.Min(token.Span.End, line.Span.End);
            var tokenSpan = TextSpan.FromBounds(tokenStart, tokenEnd);
            // StartPosition = firstLineCharacter + firstTokenCharacter
            document.Selection.StartPosition = tokenSpan.Start;
            // StartPosition = firstLineCharacter + LastTokenCharacter
            document.Selection.EndPosition = tokenSpan.End;
            switch (token.Kind)
            {
                case SyntaxKind.IdentifierToken:
                    document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 156, 220, 254);
                    break;
                case SyntaxKind.NumberToken:
                    document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 181, 206, 168);
                    break;
                case SyntaxKind.StringToken:
                    document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 214, 157, 133);
                    break;
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                    document.Selection.CharacterFormat.ForegroundColor = Colors.DarkGreen;
                    break;
                default:
                    if (token.Kind.ToString().EndsWith("Keyword"))
                        document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 86, 156, 214);
                    else
                        document.Selection.CharacterFormat.ForegroundColor = Colors.DarkGray;
                    break;
            }
            if (document.Selection.CharacterFormat.Underline != UnderlineType.None)
                document.Selection.CharacterFormat.Underline = UnderlineType.None;
        }
    }

    private void ShowDiagnostics(ImmutableArray<Diagnostic> diagnostics, RichEditBox sender)
    {
        sender.Document.Selection.GetPoint(HorizontalCharacterAlignment.Left, VerticalCharacterAlignment.Baseline, PointOptions.ClientCoordinates, out var point);
        var position = sender.Document.GetRangeFromPoint(point, PointOptions.ClientCoordinates);
        foreach (var diagnostic in diagnostics)
        {
            sender.Document.Selection.StartPosition = diagnostic.Location.Span.Start;
            sender.Document.Selection.EndPosition = diagnostic.Location.Span.End;
            sender.Document.Selection.CharacterFormat.ForegroundColor = Colors.Red;
            sender.Document.Selection.CharacterFormat.Underline = UnderlineType.DoubleWave;
        }
        sender.Document.Selection.SetRange(position.StartPosition, position.EndPosition);
    }
}
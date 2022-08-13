using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Forge.Contracts.Messages;
using Forge.Core.Helpers;
using Forge.Core.Models;
using Forge.Services;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.IO.Diagnostics;
using SparkCore.IO.Text;
using Windows.UI;

namespace Forge.ViewModels;
public class FileViewModel : BaseViewModel
{
    private string _fileName = string.Empty;
    private string _text = string.Empty;
    private SyntaxTree _syntaxTree;
    private List<Diagnostic> diagnostics = new();

    private ObservableCollection<string> symbols = new();
    public ImmutableArray<Diagnostic> Diagnostics = ImmutableArray<Diagnostic>.Empty;
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
    public SyntaxTree SyntaxTree
    {
        get => _syntaxTree;
        set => SetProperty(ref _syntaxTree, value);
    }
    public RichEditBox CodeEditBox
    {
        get;
        set;
    }
    public FileViewModel()
    {
        Messenger.Register<FileViewModel, BringDiagnosticIntoView>(this, (r, m) =>
        {
            r.ShowDiagnostic(m.Value);
        });
    }
    private void ShowDiagnostic(Diagnostic diagnostic)
    {
        CodeEditBox.Focus(FocusState.Keyboard);
        var startPosition = diagnostic.Location.Span.Start;
        var endPosition = diagnostic.Location.Span.End;
        CodeEditBox.Document.Selection.SetRange(startPosition, endPosition);
    }

    public FileViewModel(string fileName, string text) : this()
    {
        FileName = fileName;
        Text = text;
        SyntaxTree = SyntaxTree.Parse(text);
    }

    public void DoTextHightLighting(RichEditBox sender, string plainText, ITextRange position)//, RichEditBox tokenText, RichEditBox syntaxTreeText, RichEditBox intermText)
    {
        var document = sender.Document;
        var oLines = Text.Split("\r");
        var nLines = plainText.Split("\r").Where(dl => !(string.IsNullOrEmpty(dl) || string.IsNullOrWhiteSpace(dl)));

        var l = nLines.Except(oLines).ToList();

        SyntaxTree = SyntaxTree.Parse(plainText);
        var editedLines = SyntaxTree.Text.Lines.Select(a => a)
                                         .Where(a => l.Contains(a.ToString()))
                                         .ToImmutableArray();
        PaintTokens(document, SyntaxTree, editedLines);

        document.Selection.SetRange(position.StartPosition, position.EndPosition);
        Text = plainText;
        Messenger.Send(new UpdateDiagnosticsRequest(null));
    }
    public void CheckErrors(RichEditBox sender, ITextRange position)
    {
        var diagnostics = DiagnosticService.Instance.GetDiagnostics(FileName);
        if (Diagnostics.Equals(diagnostics))
        {
            return;
        }

        var document = sender.Document;
        var sourceText = SyntaxTree.Text;
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
        var dlines = ImmutableArray.CreateBuilder<TextLine>();
        foreach (var diagnostic in diagnostics)
        {
            var lines = GetTextLines(diagnostic.Location.StartLine, diagnostic.Location.EndLine);
            dlines.AddRange(lines);
        }
        PaintTokens(document, SyntaxTree, sourceText.Lines.Except(dlines.ToImmutable()).ToImmutableArray());
        ShowDiagnostic(diagnostics, sender);
        Diagnostics = diagnostics;

        document.Selection.SetRange(position.StartPosition, position.EndPosition);
    }
    private static void PaintTokens(RichEditTextDocument document, SyntaxTree syntaxTree, ImmutableArray<TextLine> lines)
    {
        foreach (var line in lines)
        {
            PaintTokens(document, syntaxTree, line);
        }
    }
    private static void PaintTokens(RichEditTextDocument document, SyntaxTree syntaxTree, TextLine line)
    {
        var classifiedSpans = Classifier.Classify(syntaxTree, line.Span);
        foreach (var classifiedSpan in classifiedSpans)
        {
            // StartPosition = firstLineCharacter + firstTokenCharacter
            document.Selection.StartPosition = classifiedSpan.Span.Start;
            // StartPosition = firstLineCharacter + LastTokenCharacter
            document.Selection.EndPosition = classifiedSpan.Span.End;

            document.Selection.CharacterFormat.Underline = UnderlineType.None;
            switch (classifiedSpan.Classification)
            {
                case Classification.Keyword:
                    document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 86, 156, 214);
                    break;
                case Classification.Identifier:
                    document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 156, 220, 254);
                    break;
                case Classification.Number:
                    document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 181, 206, 168);
                    break;
                case Classification.String:
                    document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 214, 157, 133);
                    break;
                case Classification.Comment:
                    document.Selection.CharacterFormat.ForegroundColor = Colors.DarkGreen;
                    break;
                case Classification.Operator:
                    document.Selection.CharacterFormat.ForegroundColor = Colors.DarkGray;
                    break;
                case Classification.Text:
                default:
                    document.Selection.CharacterFormat.ForegroundColor = Colors.White;
                    break;
            }
        }
    }
    private void ShowDiagnostic(ImmutableArray<Diagnostic> diagnostics, RichEditBox sender)
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
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
using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.IO;
using SparkCore.IO.Diagnostics;
using Windows.UI;

namespace SparkCore.spc.ViewModels;
public class FileViewModel : BaseViewModel
{
    private string _fileName;
    private string _text;
    private ObservableCollection<string> diagnostics = new();
    private ObservableCollection<string> symbols = new();
    public ObservableCollection<string> Diagnostics
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
    }
    public FileViewModel(string fileName, string text)
    {
        FileName = fileName;
        Text = text;
    }

    public void ChangeDisplay(RichEditBox sender, RichEditBox tokenText, RichEditBox syntaxTreeText, RichEditBox intermText)
    {

        sender.Document.Selection.GetPoint(HorizontalCharacterAlignment.Left, VerticalCharacterAlignment.Baseline, PointOptions.ClientCoordinates, out var point);
        var position = sender.Document.GetRangeFromPoint(point, PointOptions.ClientCoordinates);

        foreach (var line in _text.Split(Environment.NewLine))
        {
            var tokens = SyntaxTree.ParseTokens(line);
            foreach (var token in tokens)
            {
                sender.Document.Selection.StartPosition = token.Span.Start;
                sender.Document.Selection.EndPosition = token.Span.End;
                switch (token.Kind)
                {
                    case SyntaxKind.IdentifierToken:
                        sender.Document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 156, 220, 254);
                        break;
                    case SyntaxKind.NumberToken:
                        sender.Document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 181, 206, 168);
                        break;
                    case SyntaxKind.StringToken:
                        sender.Document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 214, 157, 133);
                        break;
                    default:
                        if (token.Kind.ToString().EndsWith("Keyword"))
                            sender.Document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 86, 156, 214);
                        else
                            sender.Document.Selection.CharacterFormat.ForegroundColor = Colors.DarkGray;
                        break;
                }
            }

            sender.Document.Selection.SetRange(position.StartPosition, position.EndPosition);
        }

        Diagnostics.Clear();
        FillTokens(tokenText);
        var syntaxTree = SyntaxTree.Parse(_text);
        FillSyntaxTree(syntaxTree, syntaxTreeText);
        FillIntermediate(syntaxTree, intermText, sender);
    }
    private void FillTokens(RichEditBox tokenText)
    {
        var tokensList = SyntaxTree.ParseTokens(_text);
        var text = "";
        for (var i = 0; i < tokensList.Length - 1; i++)
        {
            var token = tokensList[i];
            switch (token.Kind)
            {
                case SyntaxKind.IdentifierToken:
                    text += token.Kind + " " + token.Text + "\n";
                    break;
                case SyntaxKind.NumberToken:
                    text += token.Kind + " " + token.Text + "\n";
                    break;
                case SyntaxKind.StringToken:
                    text += token.Kind + " " + token.Text + "\n";
                    break;
                case SyntaxKind.WhiteSpaceToken:
                    break;
                default:
                    text += token.Kind + " " + SyntaxFacts.GetText(token.Kind) + "\n";
                    break;
            }
        }
        tokenText.Document.SetText(TextSetOptions.None, text);
    }
    private void FillSyntaxTree(SyntaxTree syntaxTree, RichEditBox syntaxTreeText)
    {
        using StringWriter writer = new();
        syntaxTree.Root.WriteTo(writer);

        syntaxTreeText.Document.SetText(TextSetOptions.None, writer.ToString());
    }
    private void FillIntermediate(SyntaxTree syntaxTree, RichEditBox intermText, RichEditBox sender)
    {
        using StringWriter writer = new();
        var text = syntaxTree.Text.ToString();
        if (!string.IsNullOrWhiteSpace(text))
        {
            var compilation = Compilation.Create(syntaxTree);

            var result = compilation.Evaluate(new());
            if (!result.Diagnostics.Any())
            {
                compilation.EmitTree(writer);
            }
            else
            {
                using StringWriter diag = new();
                diag.WriteDiagnostics(result.Diagnostics);
                Diagnostics.Add(diag.ToString());
                ShowDiagnostics(result.Diagnostics, sender);
            }

            FillSymbols(compilation);

        }
        intermText.Document.SetText(TextSetOptions.None, writer.ToString());
    }

    private void FillSymbols(Compilation compilation)
    {
        Symbols.Clear();
        var symbols = compilation.GetSymbols().OrderBy(s => s.Kind).ThenBy(s => s.Name);
        foreach (var symbol in symbols)
        {
            using StringWriter writer = new();
            symbol.WriteTo(writer);
            Symbols.Add(writer.ToString());
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

        }
        sender.Document.Selection.SetRange(position.StartPosition, position.EndPosition);

    }
}

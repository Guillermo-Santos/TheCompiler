using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Syntax.Tree;
using Windows.UI;

namespace SparkCore.spc.ViewModels;
public class FileViewModel : BaseViewModel
{
    private string _fileName;
    private string _text;

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


    public void OnTextChange(RichEditBox rich)
    {
        rich.Document.Selection.GetPoint(HorizontalCharacterAlignment.Left, VerticalCharacterAlignment.Baseline, PointOptions.ClientCoordinates, out var point);
        var position = rich.Document.GetRangeFromPoint(point, PointOptions.ClientCoordinates);

        foreach (var line in _text.Split(Environment.NewLine))
        {
            var tokens = SyntaxTree.ParseTokens(line);
            foreach (var token in tokens)
            {

                rich.Document.Selection.StartPosition = token.Span.Start;
                rich.Document.Selection.EndPosition = token.Span.End;
                switch (token.Kind)
                {
                    case SyntaxKind.IdentifierToken:
                        rich.Document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 156, 220, 254);
                        break;
                    case SyntaxKind.NumberToken:
                        rich.Document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 181, 206, 168);
                        break;
                    case SyntaxKind.StringToken:
                        rich.Document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 214, 157, 133);
                        break;
                    default:
                        if (token.Kind.ToString().EndsWith("Keyword"))
                            rich.Document.Selection.CharacterFormat.ForegroundColor = Color.FromArgb(1, 86, 156, 214);
                        else
                            rich.Document.Selection.CharacterFormat.ForegroundColor = Colors.DarkGray;
                        break;
                }
            }

            rich.Document.Selection.SetRange(position.StartPosition, position.EndPosition);
        }
    }
}

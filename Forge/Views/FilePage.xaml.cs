using Forge.ViewModels;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Forge.Views;

// TODO: Change ExamplePage to 'FilePage' and make logic to control tabs of files.
public sealed partial class FilePage : Page
{
    // TODO: add a DispatcherTimer to control diagnostics.
    private readonly DispatcherTimer _syntaxHightLighterTimer;
    private readonly DispatcherTimer _errorCheckerTimer;
    public FileViewModel ViewModel
    {
        get;
    }

    public FilePage()
    {
        ViewModel = App.GetService<FileViewModel>();
        InitializeComponent();
        // Disable undo option of the richteditbox as i do not know how to work with it yet.
        code.Document.UndoLimit = 0;
        // Setting up the syntas Hightlighter timer.
        _syntaxHightLighterTimer = new();
        _syntaxHightLighterTimer.Interval = new(0, 0, 0, 0, 250);
        _syntaxHightLighterTimer.Tick += ChangeText;
        // Setting up the error checker timer.
        _errorCheckerTimer = new();
        _errorCheckerTimer.Interval = new(0, 0, 1);
        _errorCheckerTimer.Tick += CheckErros;
    }

    private void ChangeText(object? sender, object e)
    {
        _syntaxHightLighterTimer.Stop(); 
        code.Document.GetText(TextGetOptions.None, out var plainText);
        if (!IsNewInput(plainText))
        {
            return;
        }

        code.Document.Selection.GetPoint(HorizontalCharacterAlignment.Left, VerticalCharacterAlignment.Baseline, PointOptions.ClientCoordinates, out var point);
        var position = code.Document.GetRangeFromPoint(point, PointOptions.ClientCoordinates);
        ViewModel.DoTextHightLighting(code, plainText, position);
    }
    private void CheckErros(object? sender, object e)
    {
        _errorCheckerTimer.Stop(); 
        code.Document.GetText(TextGetOptions.None, out var plainText);
        if (ViewModel.IsBusy)
        {
            return;
        }

        code.Document.Selection.GetPoint(HorizontalCharacterAlignment.Left, VerticalCharacterAlignment.Baseline, PointOptions.ClientCoordinates, out var point);
        var position = code.Document.GetRangeFromPoint(point, PointOptions.ClientCoordinates);
        ViewModel.CheckErrors(code, plainText, position);
    }
    private bool IsNewInput(string text)
    {
        return !(text == ViewModel.Text);
    }
    private void code_TextChanged(object sender, RoutedEventArgs e)
    {
        _syntaxHightLighterTimer.Stop();
        _syntaxHightLighterTimer.Start();
        if (!ViewModel.IsBusy)
        {
            _errorCheckerTimer.Stop();
            _errorCheckerTimer.Start();
        }
        //ViewModel.ChangeDisplay(code);
    }
}

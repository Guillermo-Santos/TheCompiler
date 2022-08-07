using Forge.Services;
using Forge.ViewModels;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
namespace Forge.Views;

public sealed partial class FilePage : Page
{
    // TODO: move timers and eventhandling to ViewModel and make error checker generic to files.
    private readonly DispatcherTimer _syntaxHightLighterTimer;
    private readonly DispatcherTimer _errorCheckerTimer;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, (string)value);
    }

    public DependencyProperty TextProperty = DependencyProperty.Register(
                                                nameof(Text),
                                                typeof(string),
                                                typeof(FilePage),
                                                new PropertyMetadata(default(string), new PropertyChangedCallback(OnTextChanged))
                                                );
    public string FileName
    {
        get => (string)GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, (string)value);
    }

    public DependencyProperty FileNameProperty = DependencyProperty.Register(
                                                nameof(FileName),
                                                typeof(string),
                                                typeof(FilePage),
                                                new PropertyMetadata(default(string))
                                                );
    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        FilePage? filePageControl = d as FilePage; //null checks omitted
        filePageControl.code.Document.GetText(TextGetOptions.None, out var text);
        if (text != filePageControl.Text)
        {
            filePageControl?.code.Document.SetText(TextSetOptions.None, filePageControl.Text);
        }
        SparkFileService.Instance.SetText(filePageControl.FileName, filePageControl.Text);
    }
    public FileViewModel ViewModel
    {
        get;
    }

    public FilePage()
    {
        ViewModel = App.GetService<FileViewModel>();
        InitializeComponent();

        ViewModel.CodeEditBox = code;
        // Disable undo option of the richteditbox as i do not know how to work with it yet.
        code.Document.UndoLimit = 0;
        // Setting up the syntas Hightlighter timer.
        _syntaxHightLighterTimer = new();
        _syntaxHightLighterTimer.Interval = new(0, 0, 0, 0, 250);
        _syntaxHightLighterTimer.Tick += ChangeText;
        // Setting up the error checker timer.
        _errorCheckerTimer = new();
        _errorCheckerTimer.Interval = new(0, 0, 0, 1, 500);
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
        Text = plainText;
        _errorCheckerTimer.Stop();
        _errorCheckerTimer.Start();
    }
    private void CheckErros(object? sender, object e)
    {
        _errorCheckerTimer.Stop();
        ViewModel.FileName = FileName;
        code.Document.Selection.GetPoint(HorizontalCharacterAlignment.Left, VerticalCharacterAlignment.Baseline, PointOptions.ClientCoordinates, out var point);
        var position = code.Document.GetRangeFromPoint(point, PointOptions.ClientCoordinates);
        ViewModel.CheckErrors(code, position);
    }
    private bool IsNewInput(string text)
    {
        return !(text == ViewModel.Text);
    }

    private void code_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
    {
        _syntaxHightLighterTimer.Stop();
        _syntaxHightLighterTimer.Start();
    }
}

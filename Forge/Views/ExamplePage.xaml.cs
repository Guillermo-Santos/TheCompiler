using Forge.ViewModels;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;

namespace Forge.Views;

// TODO: Change ExamplePage to 'FilePage' and make logic to control tabs of files.
public sealed partial class ExamplePage : Page
{
    public FileViewModel ViewModel
    {
        get;
    }

    public ExamplePage()
    {
        ViewModel = App.GetService<FileViewModel>();
        InitializeComponent();
    }

    private void code_TextChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        code.Document.GetText(TextGetOptions.None, out var text);
        if (text == ViewModel.Text)
            return;
        ViewModel.Text = text;
        ViewModel.ChangeDisplay(code);//, code, code, code);
    }
}

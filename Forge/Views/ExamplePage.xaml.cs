using Forge.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Forge.Views;

public sealed partial class ExamplePage : Page
{
    public ExampleViewModel ViewModel
    {
        get;
    }

    public ExamplePage()
    {
        ViewModel = App.GetService<ExampleViewModel>();
        InitializeComponent();
    }
}

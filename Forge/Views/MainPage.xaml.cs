using Forge.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Forge.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    private void TabView_AddTabButtonClick(TabView sender, object args)
    {
        ViewModel.Files.Add(new SparkCore.IO.Text.SourceText(@"print(""Hellow world"")", "New Document"));
    }
}

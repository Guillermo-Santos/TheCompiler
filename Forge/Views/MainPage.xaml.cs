using Forge.Models;
using Forge.Services;
using Forge.ViewModels;
using Microsoft.UI.Xaml.Controls;
using SparkCore.IO.Diagnostics;

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
        var document = new Document("New Document", "");
        SparkFileService.Instance.AddFile(document);
        SparkFileService.Instance.OpenFile(document);
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        SparkFileService.Instance.CloseFile(args.Item as Document);
    }

    private void errors_DoubleTapped(object? sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        var listbox = (ListBox)sender;
        var diagnostic = (Diagnostic)listbox.SelectedItem;
        if (diagnostic == null)
            return;
        listbox.IsEnabled = false;
        codeTabs.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
        ViewModel.LoadDiagnosticFile(diagnostic);
        listbox.IsEnabled = true;
    }
}

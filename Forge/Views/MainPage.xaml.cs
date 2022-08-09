using Forge.Core.Models;
using Forge.Services;
using Forge.ViewModels;
using Microsoft.UI.Xaml.Controls;
using SparkCore.IO.Diagnostics;
using Windows.Storage;
using Windows.Storage.Pickers;

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
        ProjectService.Instance.LoadProject();
        var document = new Document("New Document", "");
        SparkFileService.Instance.AddFile(document);
        SparkFileService.Instance.OpenFile(document);
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        // LoadFileAsync();
        SparkFileService.Instance.CloseFile(args.Item as Document);
    }
    private async Task<StorageFolder> LoadFileAsync()
    {
        var window = App.MainWindow;
        var FolderPicker = new FolderPicker
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        var hwnd = App.MainWindow.GetWindowHandle();
        WinRT.Interop.InitializeWithWindow.Initialize(FolderPicker, hwnd);
        var folder = await FolderPicker.PickSingleFolderAsync();


        if (folder == null)
        {
            var messageDialg = window.CreateMessageDialog("Folder no seleccionado.", title: "Error");
            await messageDialg.ShowAsync();
        }
        return folder;
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

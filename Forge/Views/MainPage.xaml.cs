using Forge.Models;
using Forge.Services;
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

        //var newTab = new TabViewItem();
        //newTab.IconSource = new SymbolIconSource() { Symbol = Symbol.Document };
        //newTab.Header = "New Document";

        //// The Content of a TabViewItem is often a frame which hosts a page.
        //Frame frame = new Frame();
        //frame.Content = new FilePage();
        //newTab.Content = frame;

        //sender.TabItems.Add(newTab);
        var document = new Document("New Document", "");
        SparkFileService.Instance.AddFile(document);
        SparkFileService.Instance.OpenFile(document);
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        SparkFileService.Instance.CloseFile(args.Item as Document);
    }
}

using Forge.Models;
using Forge.Services;
using Forge.ViewModels;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Build.Utilities;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using SparkCore.IO.Diagnostics;
using Windows.Foundation;

namespace Forge.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    //public bool Run(FileInfo msbuildFile, string[] targets = null, IDictionary<string, string> properties = null, LoggerVerbosity loggerVerbosity = LoggerVerbosity.Detailed)
    //{
    //    if (!msbuildFile.Exists) throw new ArgumentException("msbuildFile does not exist");

    //    if (targets == null)
    //    {
    //        targets = new string[] { };
    //    }
    //    if (properties == null)
    //    {
    //        properties = new Dictionary<string, string>();
    //    }
    //    var toolsetVersion = ToolLocationHelper.CurrentToolsVersion;
    //    var msbuildDir = ToolLocationHelper.GetPathToBuildTools(toolsetVersion);
    //    Console.Out.WriteLine("Running {0} targets: {1} properties: {2}, cwd: {3}",
    //                          msbuildFile.FullName,
    //                          string.Join(",", targets),
    //                          string.Join(",", properties),
    //                          Environment.CurrentDirectory);
    //    var project = new Project(msbuildFile.FullName, properties, "4.0");
    //    return project.Build(targets, new ILogger[] { new ConsoleLogger(loggerVerbosity) });
    //}
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

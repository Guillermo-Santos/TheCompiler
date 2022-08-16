using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Forge;
using Forge.Contracts.Messages;
using Forge.Contracts.Services;
using Forge.Core.Models;
using Forge.Helpers;
using Forge.Services;
using Forge.ViewModels;
using Forge.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;

public class ShellViewModel : ObservableRecipient
{
    private bool _isBackEnabled;
    private ProjectService ProjectService = ProjectService.Instance;

    public ICommand MenuFileOpenProjectCommand
    {
        get;
    }
    public ICommand MenuFileNewProjectCommand
    {
        get;
    }
    public ICommand MenuFileSaveCommand
    {
        get;
    }
    public ICommand MenuFileSaveAllCommand
    {
        get;
    }
    public ICommand MenuFileExitCommand
    {
        get;
    }
    public ICommand MenuRunBuildCommand
    {
        get;
    }
    public ICommand MenuRunDeployCommand
    {
        get;
    }
    public ICommand MenuRunBuildDeployCommand
    {
        get;
    }
    public ICommand MenuSettingsCommand
    {
        get;
    }
    public INavigationService NavigationService
    {
        get;
    }

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    public ShellViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;

        MenuFileOpenProjectCommand = new RelayCommand(OnMenuFileOpenProject);
        MenuFileNewProjectCommand = new RelayCommand(OnMenuFileNewProject);
        MenuFileSaveCommand = new RelayCommand(OnMenuFileSave);
        MenuFileSaveAllCommand = new RelayCommand(OnMenuFileSaveAll);
        MenuFileExitCommand = new RelayCommand(OnMenuFileExit);
        MenuRunBuildCommand = new RelayCommand(OnMenuRunBuild);
        MenuRunDeployCommand = new RelayCommand(OnMenuRunDeploy);
        MenuRunBuildDeployCommand = new RelayCommand(OnMenuRunBuildDeploy);
        MenuSettingsCommand = new RelayCommand(OnMenuSettings);
    }

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = NavigationService.CanGoBack;
    private void OnMenuFileOpenProject()
    {
        _ = ProjectService.LoadProject();
    }
    private void OnMenuFileNewProject()
    {
        _ = GetNewProjectData();
    }
    private async Task GetNewProjectData()
    {
        var newProjectDialog = new NewProjectContentDialog
        {
            XamlRoot = App.MainWindow.Content.XamlRoot,
        };
        var option = await newProjectDialog.ShowAsync();
        // TODO: Add code to handling new project
        //if(option == ContentDialogResult.Primary)
        //{

        //}
        //else
        //{
        //}
        await Task.CompletedTask;
    }
    private void OnMenuFileSave()
    {
        Document document = Messenger.Send<OpenDocumentRequest>();
        if (document == null)
        {
            return;
        }

        ProjectService.SaveFile(document);
    }
    private void OnMenuFileSaveAll()
    {
        ProjectService.SaveAll();
    }
    private void OnMenuFileExit() => Application.Current.Exit();
    private void OnMenuRunBuild()
    {
        ProjectService.SaveAll();
        _ = CompilationService.Build(BuildType.Build, ProjectService.GetProjectFile());
    }
    private void OnMenuRunDeploy()
    {
        ProjectService.SaveAll();
        _ = CompilationService.Build(BuildType.Deploy, ProjectService.GetProjectFile());
    }
    private void OnMenuRunBuildDeploy()
    {
        ProjectService.SaveAll();
        _ = CompilationService.Build(BuildType.BuildDeploy, ProjectService.GetProjectFile());
    }
    private void OnMenuSettings()
    {
        if(NavigationService.Frame!.Content is SettingsPage)
        {
            NavigationService.GoBack();
        }
        else
        {
            NavigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        }
    }
}

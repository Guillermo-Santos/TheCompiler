using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forge;
using Forge.Contracts.Services;
using Forge.Services;
using Forge.ViewModels;
using Forge.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
    public ICommand MenuFileExitCommand
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
        MenuFileExitCommand = new RelayCommand(OnMenuFileExit);
        MenuSettingsCommand = new RelayCommand(OnMenuSettings);
    }

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = NavigationService.CanGoBack;
    private void OnMenuFileOpenProject()
    {
        ProjectService.LoadProject();
    }
    string path = string.Empty;
    private void OnMenuFileNewProject()
    {
        GetNewProjectData();
        //ProjectService.LoadProject(path);
    }

    private async Task GetNewProjectData()
    {
        var newProjectDialog = new NewProjectContentDialog
        {
            XamlRoot = App.MainWindow.Content.XamlRoot
        };
        var option = await newProjectDialog.ShowAsync();
        if(option == ContentDialogResult.Primary)
        {

        }
        else
        {
        }
        path = newProjectDialog.ToString();
        await Task.CompletedTask;
    }

    private void OnMenuFileExit() => Application.Current.Exit();
    private void OnMenuSettings()
    {
        if(NavigationService.Frame.Content is SettingsPage)
        {
            NavigationService.GoBack();
        }
        else
        {
            NavigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        }
    }
}

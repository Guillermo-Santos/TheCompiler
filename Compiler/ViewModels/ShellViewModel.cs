using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Compiler.Contracts.Services;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;

namespace Compiler.ViewModels;

public class ShellViewModel : ObservableRecipient
{
    private bool _isBackEnabled;
    private object _selected;
    private ICommand _menuFileExitCommand;
    private ICommand _menuSettingsCommand;
    private ICommand _menuViewsMainCommand;

    public ICommand MenuFileExitCommand => _menuFileExitCommand ??= new RelayCommand(OnMenuFileExit);

    public ICommand MenuSettingsCommand => _menuSettingsCommand ??= new RelayCommand(OnMenuSettings);

    public ICommand MenuViewsMainCommand => _menuViewsMainCommand ??= new RelayCommand(OnMenuViewsMain);

    public INavigationService NavigationService
    {
        get;
    }

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    public object Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public ShellViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
    }

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = NavigationService.CanGoBack;

    private void OnMenuFileExit() => Application.Current.Exit();

    private void OnMenuSettings() => NavigationService.NavigateTo(typeof(SettingsViewModel).FullName);

    private void OnMenuViewsMain() => NavigationService.NavigateTo(typeof(MainViewModel).FullName);
}

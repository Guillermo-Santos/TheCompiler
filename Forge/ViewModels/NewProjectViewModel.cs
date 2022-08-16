using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Forge.Contracts.Messages;
using Forge.Services;
using Windows.Storage.Pickers;

namespace Forge.ViewModels;

public partial class NewProjectViewModel : ObservableRecipient
{
    private const string proj = "Forge\\Projects";
    private string LocalData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Path))]
    private string _projectName;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Path))]
    private string _rootPath;
    public string Path => $"{RootPath}\\{ProjectName}";
    
    public NewProjectViewModel()
    {
        RootPath = System.IO.Path.Combine(LocalData, proj);
    }

    [RelayCommand]
    private void OnCreate()
    {
        ProjectService.Instance.CreateProject(Path, ProjectName);
    }
    [RelayCommand]
    private async Task OnBrowse()
    {
        var FolderPicker = new FolderPicker
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        var hwnd = App.MainWindow.GetWindowHandle();
        WinRT.Interop.InitializeWithWindow.Initialize(FolderPicker, hwnd);
        var folder = await FolderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            RootPath = folder.Path;
        }
    }
}
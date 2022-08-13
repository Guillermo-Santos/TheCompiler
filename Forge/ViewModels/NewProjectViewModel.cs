using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Forge.ViewModels;

public class NewProjectViewModel : ObservableObject
{
    private string? _projectName;
    public string? ProjectName
    {
        get => _projectName;
        set
        {
            if(SetProperty(ref _projectName, value))
            {
                OnPropertyChanged(nameof(ProjectPath));
            }
        }
    }
    private const string projectsPath = "Forge\\Projects";
    private string LocalData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private string? _rootPath;
    public string? RootPath
    {
        get => _rootPath;
        set
        {
            if (SetProperty(ref _rootPath, value))
            {
                OnPropertyChanged(nameof(ProjectPath));
            }
        }
    }
    public string ProjectPath
    {
        get;
        set;
    }
    public NewProjectViewModel()
    {
        RootPath = Path.Combine(LocalData, projectsPath);
        ProjectPath = $"{_rootPath}\\{ProjectName}";
    }
}
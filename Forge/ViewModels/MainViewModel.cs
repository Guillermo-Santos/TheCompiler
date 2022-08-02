using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SparkCore.IO.Text;

namespace Forge.ViewModels;

public class MainViewModel : BaseViewModel
{
    private ObservableCollection<SourceText> _files = new();
    public ObservableCollection<SourceText> Files
    {
        get => _files;
        set => SetProperty(ref _files, value);
    }
    public MainViewModel()
    {
    }
}

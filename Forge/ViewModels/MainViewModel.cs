using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Forge.Services;
using Forge.Models;
using SparkCore.IO.Text;

namespace Forge.ViewModels;

public class MainViewModel : BaseViewModel
{
    SparkFileService fileService = SparkFileService.Instance;
    public ObservableCollection<Document> Files
    {
        get => fileService.OpenDocuments;
        set => fileService.OpenDocuments = value;
    }
    public MainViewModel()
    {
        fileService.AddFile(new("archivo1", ""));
        fileService.AddFile(new("archivo2", ""));
        fileService.AddFile(new("archivo3", ""));
        fileService.AddFile(new("archivo4", ""));
        fileService.OpenFile(fileService.Files[0]);
        fileService.OpenFile(fileService.Files[1]);
        fileService.OpenFile(fileService.Files[2]);
        fileService.OpenFile(fileService.Files[3]);
        fileService.OpenFile(fileService.Files[0]);
    }
}

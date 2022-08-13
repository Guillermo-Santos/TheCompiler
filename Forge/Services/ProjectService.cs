using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Forge.Core.Models;
using Forge.Views;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace Forge.Services;
public sealed class ProjectService: ObservableRecipient
{
    private const string projectExtension = ".spksproj";
    private const string fileExtension = ".spks";
    private static ProjectService? _instance;
    private ObservableCollection<Folder> _folderRoot = new();
    public ObservableCollection<Folder> ProjectRoot
    {
        get => _folderRoot;
        set => SetProperty(ref _folderRoot, value);
    }
    private ProjectService()
    {
    }
    public static ProjectService Instance => _instance ??= new ProjectService();

    public async Task LoadProject()
    {
        var projFile = await LoadFileAsync(projectExtension);
        if (projFile == null)
            return;
        var projDirectoryPath = Path.GetDirectoryName(projFile.Path);
        ProjectRoot.Clear();
        ProjectRoot.Add(GetFolder(projDirectoryPath));
    }
    public void LoadProject(string path)
    {
        ProjectRoot.Clear();
        ProjectRoot.Add(GetFolder(path));
    }
    private static Folder GetFolder(string? path)
    {
        var directions = ImmutableArray.CreateBuilder<Direction>();
        foreach(var directory in Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly)
                                          .Where(d => 
                                                      !d.Contains("\\obj") && 
                                                      !d.Contains("\\bin"))
                                          .ToImmutableSortedSet())
        {
            directions.Add(GetFolder(directory));
        }
        foreach (var filePath in Directory.EnumerateFiles(path, "*" + fileExtension, SearchOption.TopDirectoryOnly).ToImmutableSortedSet())
        {
            var text = File.ReadAllText(filePath);
            var document = new Document(filePath, text);
            directions.Add(document);
        }
        return new Folder(path, directions.ToImmutable());
    }

    public void LoadFile()
    {
        var file = LoadFileAsync(fileExtension).Result;
    }
    private async Task<StorageFile> LoadFileAsync(string fileExtension)
    {
        if (!a)
        {
            await App.MainWindow.CreateMessageDialog("No se compilo").ShowAsync();
        }
        var FilePicker = App.MainWindow.CreateOpenFilePicker();
        FilePicker.ViewMode = PickerViewMode.Thumbnail;
        FilePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        FilePicker.FileTypeFilter.Add(fileExtension);

        var file = await FilePicker.PickSingleFileAsync();
        return file;
    }
}

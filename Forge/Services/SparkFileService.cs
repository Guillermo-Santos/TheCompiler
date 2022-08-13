using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Forge.Core.Models;
using SparkCore.IO.Diagnostics;
using Windows.Storage.Pickers;

namespace Forge.Services;
public sealed class SparkFileService : ObservableObject
{
    private ObservableCollection<Document> _files = new();
    private ObservableCollection<Document> _openDocuments = new();
    private static SparkFileService? _instance;
    private SparkFileService()
    {
    }

    public static SparkFileService Instance => _instance ??= (_instance = new SparkFileService());

    public ObservableCollection<Document> Files
    {
        get => _files;
        set => SetProperty(ref _files, value);
    }
    public ObservableCollection<Document> OpenDocuments
    {
        get => _openDocuments;
        set => SetProperty(ref _openDocuments, value);
    }


    public void SetText(string fileName, string text)
    {
        Files.First(f => f.Path == fileName).Text = text;
    }
    public void AddFile(Document document) => Files.Add(document);
    public void RemoveFile(Document document)
    {
        CloseFile(document);
        Files.Remove(document);
    }
    public int GetFileIndex(Document document)
    {
        return Files.IndexOf(document);
    }
    public void OpenFile(int index) => OpenFile(Files[index]);
    public void OpenFile(Document document)
    {
        if (!OpenDocuments.Contains(document))
        {
            OpenDocuments.Add(document);
        }
    }
    public Document OpenFile(Diagnostic diagnostic)
    {
        var sourceText = diagnostic.Location.Text;
        var document = Files.First(doc => doc.Path.Equals(sourceText.FileName) && doc.Text.Equals(sourceText.ToString()));
        OpenFile(document);
        return document;
    }
    public void CloseFile(int index) => CloseFile(Files[index]);
    public void CloseFile(Document document)
    {
        OpenDocuments.Remove(document);
    }

}

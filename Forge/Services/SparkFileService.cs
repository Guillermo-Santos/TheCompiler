using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Forge.Core.Models;
using SparkCore.IO.Diagnostics;
using Windows.Storage.Pickers;

namespace Forge.Services;
public sealed partial class SparkFileService : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Document> _documents = new();
    [ObservableProperty]
    private ObservableCollection<Document> _openDocuments = new();
    private static SparkFileService? _instance;
    private SparkFileService()
    {
    }

    public static SparkFileService Instance => _instance ??= (_instance = new SparkFileService());

    public void SetText(string fileName, string text)
    {
        Documents.First(f => f.Path == fileName).Text = text;
    }
    public void AddFile(Document document) => Documents.Add(document);
    public void RemoveFile(Document document)
    {
        CloseFile(document);
        Documents.Remove(document);
    }
    public int GetFileIndex(Document document)
    {
        return Documents.IndexOf(document);
    }
    public int GetOpenFileIndex(Document document)
    {
        return OpenDocuments.IndexOf(document);
    }
    public void OpenFile(int index) => OpenFile(Documents[index]);
    public void OpenFile(Document document)
    {
        var openDocument = OpenDocuments.Where(d => d.Path == document.Path).FirstOrDefault();
        if (openDocument == null)
        {
            OpenDocuments.Add(document);
        }
    }
    public Document OpenFile(Diagnostic diagnostic)
    {
        var sourceText = diagnostic.Location.Text;
        var document = Documents.First(doc => doc.Path.Equals(sourceText.FileName) && doc.Text.Equals(sourceText.ToString()));
        OpenFile(document);
        return document;
    }
    public void CloseFile(int index) => CloseFile(Documents[index]);
    public void CloseFile(Document document)
    {
        var openDocument = OpenDocuments.Where(d => d.Path == document.Path).FirstOrDefault();
        if (openDocument != null)
        {
            OpenDocuments.Remove(openDocument);
        }
    }
    public void CloseAll()
    {
        OpenDocuments.Clear();
    }
    public void Clean()
    {
        OpenDocuments.Clear();
        Documents.Clear();
    }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Forge.Services;
using Forge.Models;
using SparkCore.IO.Text;
using SparkCore.IO.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Forge.Contracts.Messages;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Immutable;
using CommunityToolkit.Mvvm.Input;

namespace Forge.ViewModels;

public class MainViewModel : ObservableRecipient
{
    readonly SparkFileService fileService = SparkFileService.Instance;

    public ObservableCollection<Diagnostic> Diagnostics { get; private set; } = new();
    public ObservableCollection<Document> Files => fileService.OpenDocuments;
    private Document _selectedDocument;
    public Document SelectedDocument {
        get => _selectedDocument;
        set => SetProperty(ref _selectedDocument, value);
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

        SelectedDocument = Files[0];
        
        Messenger.Register<MainViewModel, UpdateDiagnosticsView>(this, (r, m) =>
        {
            r.RefreshDiagnostics(m.Value);
        });
    }
    public void RefreshDiagnostics(ImmutableArray<Diagnostic> diagnostics)
    {
        if (!Diagnostics.Equals(diagnostics))
        {
            Diagnostics.Clear();
            foreach (var diagnostic in diagnostics)
            {
                Diagnostics.Add(diagnostic);
            }
        }
    }
    public void LoadDiagnosticFile(Diagnostic diagnostic)
    {
        var document = fileService.OpenFile(diagnostic);
        SelectedDocument = document;
        Messenger.Send(new BringDiagnosticIntoView(diagnostic));
    }
}

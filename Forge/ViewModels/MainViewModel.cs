using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Forge.Contracts.Messages;
using Forge.Contracts.ViewModels;
using Forge.Core.Models;
using Forge.Services;
using SparkCore.IO.Diagnostics;

namespace Forge.ViewModels;

public partial class MainViewModel : ObservableRecipient, INavigationAware
{
    readonly SparkFileService fileService = SparkFileService.Instance;
    public ProjectService ProjectService = ProjectService.Instance;
    public ObservableCollection<Diagnostic> Diagnostics { get; private set; } = new();
    public ObservableCollection<Document> Files => fileService.OpenDocuments;

    private Document _selectedDocument;
    public Document SelectedDocument
    {
        get => _selectedDocument;
        set => SetProperty(ref _selectedDocument, value);
    }
    [ObservableProperty]
    private Direction _selectedDirection;
    public MainViewModel()
    {
        Messenger.Register<MainViewModel, UpdateDiagnosticsView>(this, (r, m) =>
        {
            r.RefreshDiagnostics(m.Value);
        });
        Messenger.Register<MainViewModel, OpenDocumentRequest>(this, (r, m) =>
        {
            m.Reply(r.SelectedDocument);
        });
    }
    [RelayCommand]
    public void OnRefresh()
    {
        ProjectService.RefreshProject();
    }
    [RelayCommand]
    public void OnOpenRequest()
    {
        if(SelectedDirection is Document document)
        {
            fileService.OpenFile(document);
            SelectedDocument = document;
        }
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
    public void OnNavigatedTo(object parameter)
    {
        SelectedDocument = null;
    }
    public void OnNavigatedFrom()
    {
        SelectedDocument = null;
    }
}

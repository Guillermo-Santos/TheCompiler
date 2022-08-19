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
using SparkCore;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.IO.Diagnostics;

namespace Forge.ViewModels;

public partial class MainViewModel : ObservableRecipient, INavigationAware
{
    readonly SparkFileService fileService = SparkFileService.Instance;
    public ProjectService ProjectService = ProjectService.Instance;
    public ObservableCollection<Diagnostic> Diagnostics { get; private set; } = new();
    public ObservableCollection<SyntaxToken> Tokens = new();
    public ObservableCollection<Symbol> Symbols { get; private set; } = new();
    public ObservableCollection<Document> Files => fileService.OpenDocuments;
    [ObservableProperty]
    private Document _selectedDocument;
    [ObservableProperty]
    private string _syntraxTreeText;
    [ObservableProperty]
    private string _intermediateRepresentation;

    partial void OnSelectedDocumentChanged(Document? value)
    {
        Tokens.Clear();
        if (value == null)
            return;
        foreach (var token in SyntaxTree.ParseTokens(value.SourceText))
        {
            Tokens.Add(token);
        }
        var syntaxTree = SyntaxTree.Parse(value.SourceText);
        using var writter = new StringWriter();
        syntaxTree.Root.WriteTo(writter);
        SyntraxTreeText = writter.ToString();
    }

    [ObservableProperty]
    private Direction _selectedDirection;
    public MainViewModel()
    {
        Messenger.Register<MainViewModel, UpdateDiagnosticsView>(this, (r, m) =>
        {
            r.RefreshDiagnostics(m.Value);
        });
        Messenger.Register<MainViewModel, UpdateSymbolsView>(this, (r, m) =>
        {
            r.RefreshSymbols(m.Value);
        });
        Messenger.Register<MainViewModel, UpdateIntermediateView>(this, (r, m) =>
        {
            r.IntermediateRepresentation = m.Value;
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
        Diagnostics.Clear();
        foreach (var diagnostic in diagnostics)
        {
            Diagnostics.Add(diagnostic);
        }
    }
    private void RefreshSymbols(ImmutableArray<Symbol> symbols)
    {
        Symbols.Clear();
        foreach (var symbol in symbols)
        {
            Symbols.Add(symbol);
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

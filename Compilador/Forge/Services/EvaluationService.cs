﻿using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Forge.Contracts.Messages;
using Microsoft.UI.Xaml;
using SparkCore;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.IO.Diagnostics;

namespace Forge.Services;
public sealed class EvaluationService : ObservableRecipient
{
    private static EvaluationService? _instance;
    private readonly DispatcherTimer CheckErrors;
    private EvaluationService()
    {
        CheckErrors = new();
        CheckErrors.Interval = new(0, 0, 0, 0, 500);
        CheckErrors.Tick += UpdateDiagnostics;
        CheckErrors.Start();
        Messenger.Register<EvaluationService, UpdateDiagnosticsRequest>(this, (r, m) => r.ResetTimer());
    }

    public static EvaluationService Instance => _instance ??= (_instance = new EvaluationService());

    private readonly Dictionary<string, ImmutableArray<Diagnostic>> Diagnostics = new();
    public ImmutableArray<Diagnostic> GetDiagnostics(string fileName)
    {
        if (Diagnostics.ContainsKey(fileName))
        {
            return Diagnostics[fileName];
        }
        return ImmutableArray<Diagnostic>.Empty;
    }
    public void SetDiagnostics(string filename, ImmutableArray<Diagnostic> diagnostics)
    {
        if (Diagnostics.ContainsKey(filename))
        {
            Diagnostics[filename] = diagnostics;
        }
        else
        {
            Diagnostics.Add(filename, diagnostics);
        }
    }
    public void ResetTimer()
    {
        CheckErrors.Stop();
        CheckErrors.Start();
    }
    private void UpdateDiagnostics(object? sender, object e)
    {
        CheckErrors.Stop();
        var syntaxTrees = new List<SyntaxTree>();
        foreach (var document in SparkFileService.Instance.Documents)
        {
            var syntaxTree = SyntaxTree.Parse(document.SourceText);
            syntaxTrees.Add(syntaxTree);
        }
        var compilation = Compilation.Create(syntaxTrees.ToArray());
        var result = compilation.Evaluate();
        var symbols = compilation.GetSymbols()
                                 .OrderBy(s => s.Kind)
                                 .ThenBy(s => s.Name)
                                 .ToImmutableArray();
        var diagnostics = result.Diagnostics.OrderBy(d => d.Location.FileName)
                                            .ThenBy(d => d.Location.StartLine)
                                            .ThenBy(d => d.Location.StartCharacter)
                                            .ThenBy(d => d.Location.EndLine)
                                            .ThenBy(d => d.Location.EndCharacter)
                                            .ToImmutableArray();
        Update(diagnostics);

        using var writter = new StringWriter();
        compilation.EmitTree(writter);
        var intermediate = writter.ToString();

        Messenger.Send(new UpdateDiagnosticsView(diagnostics));
        Messenger.Send(new UpdateSymbolsView(symbols));
        Messenger.Send(new UpdateIntermediateView(intermediate));
    }
    private void Update(ImmutableArray<Diagnostic> diagnostics)
    {
        if (!diagnostics.Any())
        {
            Diagnostics.Clear();
            return;
        }
        var diags = ImmutableArray.CreateBuilder<Diagnostic>();
        var fileName = diagnostics.First().Location.FileName;
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Location.FileName.Equals(fileName))
            {
                diags.Add(diagnostic);
            }
            else
            {
                SetDiagnostics(fileName, diags.ToImmutable());
                diags.Clear();
                diags.Add(diagnostic);
                fileName = diagnostic.Location.FileName;
            }
        }
        if (diags.Any())
        {
            SetDiagnostics(fileName, diags.ToImmutable());
        }
    }
}
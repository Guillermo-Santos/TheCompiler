using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Forge.Core.Models;
using Forge.Views;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace Forge.Services;
public sealed class ProjectService : ObservableRecipient
{
    private const string projectExtension = ".spksproj";
    private const string fileExtension = ".spks";
    private const string mathFile = @"
/*
    A file with some functions to make easier
    create a project with this lenguaje.

    Here, you will find some incredible functions to help you with calculations.
*/
// Factorial of a number using recursion
function Fact(number: int): int
{
    if(number == 0)
        return 1
    if(number < 3)
        return number
    return number * Fact(number-1)
}
// Max number between two numbers
function Max(a: int, b: int): int
{
    if(a > b)
        return a
    return b
}
// Min number between two numbers
function Min(a: int, b: int): int{
    if(a < b)
        return a
    return b
}
// Random number between lowerBound and upperBound
function Rnd(lowerBound:int, upperBound:int): int
{
    return random(upperBound) + lowerBound
}
// Make one function to find the square(x) of a number, i dare you.
";
    private const string ioFile = @"
/*
    A file with some functions to make easier
    create a project with this lenguaje.
    
    Here, you will find some forms to get or tranform your input
*/
// Basic input getter with a prompt message.
function readLine(prompt: string): string
{
    print(prompt)
    let value = input()
    return value
}

// Breaking lenguage limitations.
function IntToBool(a: int): bool
{
    return bool(any(a))
}
function BoolToInt(a: bool): int
{
    return int(any(a))
}
";
    private const string mainFile = @"
// This is your main function
function main ()
{
    var name = readLine(""Diga su nombre"")
    print(""Hola "" + name + ""!"")
}
";
    private string projectFile => @"
<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>netcoreapp3.1</TargetFramework>
    </PropertyGroup>
    <PropertyGroup>
        <DefaultLanguageSourceExtension>.spks</DefaultLanguageSourceExtension>
    </PropertyGroup>
    <PropertyGroup>
        <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
        <AppendRuntimeIdentifierToOutputPath>false</AppendRuntimeIdentifierToOutputPath>
    </PropertyGroup>
    <Target Name=""CreateManifestResourceNames"" />
    <Target Name=""CoreCompile"" DependsOnTargets=""$(CoreCompileDependsOn)"">
        <ItemGroup>
            <ReferencePath Remove=""@(ReferencePath)""
                        Condition= ""'%(FileName)' != 'System.Runtime' AND 
                                    '%(FileName)' != 'System.Console' AND 
                                    '%(FileName)' != 'System.Runtime.Extensions' ""/>
        </ItemGroup>
"+$"        <Exec Command=\"&quot;{Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)?.Substring(6)}\\spksc.exe&quot; @(Compile->'&quot;%(Identity)&quot;', ' ') /o &quot;@(IntermediateAssembly)&quot; @(ReferencePath->'/r &quot;%(Identity)&quot;', ' ')\""+@"
            WorkingDirectory=""$(MSBuildProjectDirectory)"" />
    </Target>
</Project>
";
    private static ProjectService? _instance;
    private ObservableCollection<Folder> _folderRoot = new();
    private string? currentProjectFile;
    public ObservableCollection<Folder> ProjectRoot
    {
        get => _folderRoot;
        set => SetProperty(ref _folderRoot, value);
    }
    private ProjectService()
    {
    }
    public static ProjectService Instance => _instance ??= new ProjectService();
    public string? GetProjectFile() => currentProjectFile;
    public void SaveFile(Document document)
    {
        File.WriteAllText(document.Path, document.Text);
    }
    public void SaveAll()
    {
        var documents = SparkFileService.Instance.Documents;
        foreach (var document in documents)
        {
            SaveFile(document);
        }
    }
    public async Task CreateProject(string projectPath, string projectName)
    {
        if (string.IsNullOrEmpty(projectName))
        {
            await App.MainWindow.CreateMessageDialog("The project name was not provided.").ShowAsync();
            return;
        }
        if (string.IsNullOrEmpty(projectPath))
        {
            await App.MainWindow.CreateMessageDialog("The project path was not provided.").ShowAsync();
            return;
        }

        var functionsPath = Path.Combine(projectPath,"Functions");
        Directory.CreateDirectory(functionsPath);

        var projectFilePath = Path.Combine(projectPath,projectName + projectExtension);
        var mainFilePath = Path.Combine(projectPath, "Main"+fileExtension);
        var ioFilePath = Path.Combine(functionsPath, "IO" + fileExtension);
        var mathFilePath = Path.Combine(functionsPath, "Math" + fileExtension);

        File.WriteAllText(projectFilePath, projectFile);
        File.WriteAllText(mainFilePath, mainFile);
        File.WriteAllText(ioFilePath, ioFile);
        File.WriteAllText(mathFilePath, mathFile);

        LoadProject(projectPath);
        currentProjectFile = projectFilePath;

        await Task.CompletedTask;
    }
    public void RefreshProject()
    {
        LoadProject(Path.GetDirectoryName(currentProjectFile));
    }
    public async Task LoadProject()
    {
        var projFile = await LoadFileAsync(projectExtension);
        if (projFile == null)
            return;
        var projDirectoryPath = Path.GetDirectoryName(projFile.Path);
        LoadProject(projDirectoryPath);
        currentProjectFile = projFile.Path;
    }
    public void LoadProject(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        ProjectRoot.Clear();
        SparkFileService.Instance.Clean();
        ProjectRoot.Add(GetFolder(path));
        DiagnosticService.Instance.ResetTimer();
    }
    private Folder GetFolder(string path)
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
            SparkFileService.Instance.AddFile(document);
            directions.Add(document);
        }
        return new Folder(path, directions.ToImmutable());
    }

    private async Task<StorageFile> LoadFileAsync(string fileExtension)
    {
        var FilePicker = App.MainWindow.CreateOpenFilePicker();
        FilePicker.ViewMode = PickerViewMode.Thumbnail;
        FilePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        FilePicker.FileTypeFilter.Add(fileExtension);

        var file = await FilePicker.PickSingleFileAsync();
        return file;
    }
}

using System.Diagnostics;
using Forge.Views;
using System;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore;
using System.Reflection;

namespace Forge.Services;
public enum BuildType
{
    BuildDeploy,
    Build,
    Deploy,
}
internal static class CompilationService
{
    public static async Task<bool> Build(BuildType buildType, string? projectFile)//FileInfo msbuildFile, string[] targets = null, IDictionary<string, string> properties = null, LoggerVerbosity loggerVerbosity = LoggerVerbosity.Detailed)
    {
        if (string.IsNullOrEmpty(projectFile) || string.IsNullOrWhiteSpace(projectFile) || !File.Exists(projectFile))
        {
            await App.MainWindow.CreateMessageDialog($"The project file does not exist:\n '{projectFile}' not found.", "Error").ShowAsync();
            return false;
        }
        var action = string.Empty;
        switch (buildType)
        {
            case BuildType.Build:
                action = "build";
                break;
            case BuildType.Deploy:
                action = "run --no-build --project";
                break;
            case BuildType.BuildDeploy:
                action = "run --project";
                break;
        }

        var startInfo = new ProcessStartInfo()
        {
            FileName = $"cmd",
            Arguments = $"/C dotnet {action} \"{projectFile}\" &&pause",
            RedirectStandardOutput = false,
            RedirectStandardError = true,
        };

        try
        {
            using (var process = Process.Start(startInfo))
            {
                await process!.WaitForExitAsync();
                if(process.ExitCode != 0)
                {
                    await App.MainWindow.CreateMessageDialog(process.StandardError.ReadToEnd()).ShowAsync();
                    return false;
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}

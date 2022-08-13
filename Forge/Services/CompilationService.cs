using System.Diagnostics;
using Forge.Views;
using System;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore;
using System.Reflection;

namespace Forge.Services;
internal class CompilationService
{
    public enum BuildType
    {
        BuildDeploy,
        Build,
        Deploy,
    }
    public static async Task<bool> Build(BuildType buildType, string projectFile)//FileInfo msbuildFile, string[] targets = null, IDictionary<string, string> properties = null, LoggerVerbosity loggerVerbosity = LoggerVerbosity.Detailed)
    {
        var action = string.Empty;
        switch (buildType)
        {
            case BuildType.Build:
                action = "build";
                break;
            case BuildType.Deploy:
                action = "run --no-build";
                break;
            case BuildType.BuildDeploy:
                action = "run";
                break;
        }
        projectFile = @"D:\Aplicaciones\Microsoft Visual Studio\Projects\Compiler\Samples\hello\hello.spksproj";
        var startInfo = new ProcessStartInfo()
        {
            FileName = $"cmd",
            Arguments = $"/C dotnet {action} --project \"{projectFile}\" &&pause",
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

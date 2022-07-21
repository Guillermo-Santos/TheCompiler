using SparkCore;
using SparkCore.IO;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax.Tree;
using Mono.Options;
using Mono.Cecil.Pdb;

namespace spc;

internal static class Program
{
    private static int Main(string[] args)
    {
        var outputPath = null as string;
        var moduleName = null as string;
        var referencePaths = new List<string>();
        var sourcePaths = new List<string>();
        var helpRequested = false;
        var options = new OptionSet
        {
            "usage: spc <source-paths> [options]",
            { "r=", "The {path} of an assembly reference.", v => referencePaths.Add(v) },
            { "o=", "The output {path} of the assembly to create", v =>  outputPath = v},
            { "m=", "The {name} of the module", v =>  moduleName = v},
            { "?|h|help", "Prints help", v => helpRequested = true },
            { "<>", v => sourcePaths.Add(v) }
        };

        options.Parse(args);

        if (helpRequested)
        {
            options.WriteOptionDescriptions(Console.Out);
            return 0;
        }

        if(sourcePaths.Count == 0)
        {
            Console.Error.WriteLine("error: need at least a path to compile.");
            return 1;
        }

        if(outputPath == null)
        {
            outputPath = Path.ChangeExtension(sourcePaths[0], ".il");
        }
        if (moduleName == null)
        {
            moduleName = Path.GetFileNameWithoutExtension(outputPath);
        }

        var syntaxTrees = new List<SyntaxTree>();
        
        var hasErrors = false;
        foreach(var path in sourcePaths)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"error: file '{path}' doesn't exists.");
                hasErrors = true;
                continue;
            }
            var syntaxTree = SyntaxTree.Load(path);
            syntaxTrees.Add(syntaxTree);
        }

        foreach(var path in referencePaths)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"error: file '{path}' doesn't exists.");
                hasErrors = true;
                continue;
            }
        }

        if (hasErrors) return 1;

        var compilation = Compilation.Create(syntaxTrees.ToArray());
        //var result = compilation.Evaluate(new());

        //if (result.Diagnostics.Any())
        //{
        //    Console.Error.WriteDiagnostics(result.Diagnostics);
        //    return 1;
        //}
        var diagnostics = compilation.Emit(moduleName, referencePaths.ToArray(), outputPath);


        if (diagnostics.Any())
        {
            Console.Error.WriteDiagnostics(diagnostics);
            return 1;
        }

        return 0;
    }

    private static IEnumerable<string> GetFilePaths(IEnumerable<string> paths)
    {
        var result = new SortedSet<string>();
        foreach (var path in paths)
        {
            if (Directory.Exists(path))
            {
                result.UnionWith(Directory.EnumerateFiles(path, "*.sp", SearchOption.AllDirectories));
            }
            else
            {
                result.Add(path);
            }
        }

        return result;
    }

}
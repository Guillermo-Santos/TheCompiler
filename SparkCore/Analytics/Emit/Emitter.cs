using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SparkCore.Analytics.Binding.Tree;
using SparkCore.Analytics.Symbols;
using SparkCore.IO.Diagnostics;

namespace SparkCore.Analytics.Emit;
internal static class Emitter
{
    public static ImmutableArray<Diagnostic> Emit(BoundProgram program, string moduleName, string[] references, string outputPath)
    {
        if (program.Diagnostics.Any())
            return program.Diagnostics;

        var assemblies = new List<AssemblyDefinition>();
        var result = new DiagnosticBag();

        foreach(var reference in references)
        {
            try
            {
                var assembly = AssemblyDefinition.ReadAssembly(reference);
                assemblies.Add(assembly);

            }
            catch (BadImageFormatException)
            {
                result.ReportInvalidReference(reference);
            }
        }

        var builtInTypes = new List<(TypeSymbol type, string MetadataName)>
        {
            (TypeSymbol.Any, "System.Object"),
            (TypeSymbol.Bool, "System.Boolean"),
            (TypeSymbol.Int, "System.Int32"),
            (TypeSymbol.String, "System.String"),
            (TypeSymbol.Void, "System.Void")
        };


        var assemblyName = new AssemblyNameDefinition(moduleName, new Version(1, 0));
        var assemmblyDefinition = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Console);
        var knowTypes = new Dictionary<TypeSymbol, TypeReference>();

        foreach(var (typeSymbol, metadataName) in builtInTypes)
        {
            var typeReference = ResolveType(typeSymbol.Name, metadataName);
            knowTypes.Add(typeSymbol, typeReference);
        }

        TypeReference ResolveType(string sparkName, string metadataName)
        {
            var foundTypes = assemblies.SelectMany(a => a.Modules)
                                       .SelectMany(m => m.Types)
                                       .Where(t => t.FullName == metadataName)
                                       .ToArray();
            if (foundTypes.Length == 1)
            {
                var typeReference = assemmblyDefinition.MainModule.ImportReference(foundTypes[0]);
                return typeReference;
            }
            else if (foundTypes.Length == 0)
            {
                result.ReportRequiredTypeNotFound(sparkName, metadataName);
            }
            else if (foundTypes.Length > 1)
            {
                result.ReportRequiredTypeAmbiguous(sparkName, metadataName, foundTypes);
            }
            return null;
        }

        MethodReference ResolveMethod(string typeName, string methodName, string[] parameterTypeNames)
        {
            var foundTypes = assemblies.SelectMany(a => a.Modules)
                                       .SelectMany(m => m.Types)
                                       .Where(t => t.FullName == typeName)
                                       .ToArray();
            if (foundTypes.Length == 1)
            {
                var foundType = foundTypes[0];
                var methods = foundType.Methods.Where(m => m.Name == methodName);
                
                foreach(var method in methods)
                {
                    if (method.Parameters.Count != parameterTypeNames.Length)
                        continue;
                    
                    var allParametersMath = true;
                    
                    for(var i = 0; i < parameterTypeNames.Length; i++)
                    {
                        if (method.Parameters[i].ParameterType.FullName != parameterTypeNames[i])
                        {
                            allParametersMath = false;
                            break;
                        }
                    }

                    if (!allParametersMath)
                        continue;
                    
                    return assemmblyDefinition.MainModule.ImportReference(method);
                }

                result.ReportRequiredMethodNotFound(typeName, methodName, parameterTypeNames);
                return null;
            }
            else if (foundTypes.Length == 0)
            {
                result.ReportRequiredTypeNotFound(null, typeName);
            }
            else
            {
                result.ReportRequiredTypeAmbiguous(null, typeName, foundTypes);
            }
            return null;
        }

        var consolewriteLineReference = ResolveMethod("System.Console", "WriteLine", new [] { "System.String" });

        if (result.Any())
            return result.ToImmutableArray();

        var objectType = knowTypes[TypeSymbol.Any];
        var typeDefinition = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Public, objectType);
        assemmblyDefinition.MainModule.Types.Add(typeDefinition);

        var voidType = knowTypes[TypeSymbol.Void];
        var mainMethod = new MethodDefinition("Main", MethodAttributes.Static | MethodAttributes.Private, voidType);
        typeDefinition.Methods.Add(mainMethod);

        var ilProcessor = mainMethod.Body.GetILProcessor();
        ilProcessor.Emit(OpCodes.Ldstr, "Hello world from spark!");
        ilProcessor.Emit(OpCodes.Call, consolewriteLineReference);
        ilProcessor.Emit(OpCodes.Ret);

        assemmblyDefinition.EntryPoint = mainMethod;

        assemmblyDefinition.Write(outputPath);

        return result.ToImmutableArray();
    }
}

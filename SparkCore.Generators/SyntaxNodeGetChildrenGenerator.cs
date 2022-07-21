using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SparkCore.Generators
{
    [Generator]
    public class SyntaxNodeGetChildrenGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }
        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = (CSharpCompilation)context.Compilation;

            var types = GetAllTypes(compilation.Assembly);
            var immutableArrayType = compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1");
            var separatedSyntaxListType = compilation.GetTypeByMetadataName("SparkCore.Analytics.Syntax.Tree.SeparatedSyntaxList`1");
            var syntaxNodeType = compilation.GetTypeByMetadataName("SparkCore.Analytics.Syntax.Tree.SyntaxNode");
            var syntaxNodeTypes = types.Where(t => !t.IsAbstract && IsPartial(t) && IsDerivedFrom(t, syntaxNodeType));//.OrderBy(t => t.ContainingNamespace.MetadataName).ThenBy(t => t.MetadataName);
            SourceText sourceText;
            using (var stringWriter = new StringWriter())
            using (var indentedTextWriter = new IndentedTextWriter(stringWriter, "    "))
            {
                var lastNameSpace = syntaxNodeTypes.First().ContainingNamespace;

                indentedTextWriter.WriteLine("using System;");
                indentedTextWriter.WriteLine("using System.Collections.Generic;");
                indentedTextWriter.WriteLine("using System.Collections.Immutable;");
                //indentedTextWriter.WriteLine("using SparkCore.Analytics.Syntax.Tree.Expressions;");
                //indentedTextWriter.WriteLine("using SparkCore.Analytics.Syntax.Tree.Nodes;");
                //indentedTextWriter.WriteLine("using SparkCore.Analytics.Syntax.Tree.Statements;");
                //indentedTextWriter.WriteLine();
                WriteNameSpaceHead(syntaxNodeType, indentedTextWriter, syntaxNodeTypes.First());
                StartBlock(indentedTextWriter);

                foreach (var type in syntaxNodeTypes)
                {
                    if (!SymbolEqualityComparer.Default.Equals(type.ContainingNamespace, lastNameSpace))
                    {
                        CloseBlock(indentedTextWriter);

                        indentedTextWriter.WriteLine();

                        WriteNameSpaceHead(syntaxNodeType, indentedTextWriter, type);
                        StartBlock(indentedTextWriter);
                        lastNameSpace = type.ContainingNamespace;
                    }

                    indentedTextWriter.WriteLine($"partial class {type.Name}");
                    StartBlock(indentedTextWriter);

                    indentedTextWriter.WriteLine("public override IEnumerable<SyntaxNode> GetChildren()");
                    StartBlock(indentedTextWriter);

                    foreach(var property in type.GetMembers().OfType<IPropertySymbol>())
                    {
                        if (property.Type is INamedTypeSymbol propertyType)
                        {
                            if (IsDerivedFrom(propertyType, syntaxNodeType))
                            {
                                indentedTextWriter.WriteLine($"yield return {property.Name};");
                            }
                            else if (propertyType.TypeArguments.Length == 1 &&
                                     IsDerivedFrom(propertyType.TypeArguments[0], syntaxNodeType) &&
                                     SymbolEqualityComparer.Default.Equals(propertyType.OriginalDefinition, immutableArrayType))
                            {
                                indentedTextWriter.WriteLine($"foreach (var child in {property.Name})");
                                StartBlock(indentedTextWriter);
                                indentedTextWriter.WriteLine("yield return child;");
                                CloseBlock(indentedTextWriter);
                            }
                            else if (SymbolEqualityComparer.Default.Equals(propertyType.OriginalDefinition, separatedSyntaxListType) &&
                                     IsDerivedFrom(propertyType.TypeArguments[0], syntaxNodeType))
                            {
                                indentedTextWriter.WriteLine($"foreach (var child in {property.Name}.GetWithSeparators())");
                                StartBlock(indentedTextWriter);
                                indentedTextWriter.WriteLine("yield return child;");
                                CloseBlock(indentedTextWriter);
                            }

                        }
                    }
                    CloseBlock(indentedTextWriter);

                    CloseBlock(indentedTextWriter);
                }
                CloseBlock(indentedTextWriter);
                indentedTextWriter.Flush();

                sourceText = SourceText.From(stringWriter.ToString(), Encoding.UTF8);
            }
            context.AddSource("Generated.cs", sourceText);

            var syntaxNodeFileName = syntaxNodeType.DeclaringSyntaxReferences.First().SyntaxTree.FilePath;
            var treeDirectory = Path.GetDirectoryName(syntaxNodeFileName);

            var fileName = Path.Combine(treeDirectory, "SyntaxNode_GetChildren.txt");
            using (var writer = new StreamWriter(fileName))
                sourceText.Write(writer);
        }

        private static void WriteNameSpaceHead(INamedTypeSymbol syntaxNodeType, IndentedTextWriter indentedTextWriter, INamedTypeSymbol type)
        {
            indentedTextWriter.WriteLine($"namespace SparkCore.Analytics.Syntax.Tree.{type.ContainingNamespace.MetadataName}");
        }
        private static void StartBlock(IndentedTextWriter indentedTextWriter)
        {
            indentedTextWriter.WriteLine("{");
            indentedTextWriter.Indent++;
        }
        private static void CloseBlock(IndentedTextWriter indentedTextWriter)
        {
            indentedTextWriter.Indent--;
            indentedTextWriter.WriteLine("}");
        }

        private IReadOnlyList<INamedTypeSymbol> GetAllTypes(IAssemblySymbol symbol)
        {
            var result = new List<INamedTypeSymbol>();
            GetAllTypes(result, symbol.GlobalNamespace);
            result.OrderBy(t => t.ContainingNamespace.MetadataName).ThenBy(x => x.MetadataName);
            return result;
        }

        private void GetAllTypes(List<INamedTypeSymbol> result, INamespaceOrTypeSymbol symbol)
        {
            if(symbol is INamedTypeSymbol type)
            {
                result.Add(type);
            }

            foreach (var child in symbol.GetMembers())
            {
                if (child is INamespaceOrTypeSymbol nsChild)
                {
                    GetAllTypes(result, nsChild);
                }
            }
        }
        private bool IsDerivedFrom(ITypeSymbol type, INamedTypeSymbol baseType)
        {
            while(type != null)
            {
                if (SymbolEqualityComparer.Default.Equals(type, baseType))
                {
                    return true;
                }

                type = type.BaseType;
            }
            return false;
        }
        private bool IsPartial(INamedTypeSymbol type)
        {
            foreach(var declaration in type.DeclaringSyntaxReferences)
            {
                var syntax = declaration.GetSyntax();
                if(syntax is ClassDeclarationSyntax typeDeclaration)
                {
                    foreach(var modifier in typeDeclaration.Modifiers)
                    {
                        if (modifier.ValueText == "partial")
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
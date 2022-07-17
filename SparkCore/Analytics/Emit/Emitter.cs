﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SparkCore.Analytics.Binding;
using SparkCore.Analytics.Binding.Tree;
using SparkCore.Analytics.Binding.Tree.Expressions;
using SparkCore.Analytics.Binding.Tree.Statements;
using SparkCore.Analytics.Symbols;
using SparkCore.IO.Diagnostics;

namespace SparkCore.Analytics.Emit;
internal sealed class Emitter
{
    private readonly DiagnosticBag _diagnostics = new();

    private readonly Dictionary<TypeSymbol, TypeReference> _knowTypes;
    private readonly MethodReference _consoleWriteLineReference;
    private readonly MethodReference _consoleReadLineReference;
    private readonly MethodReference _stringConcatReference;
    private readonly AssemblyDefinition _assemmblyDefinition;
    private readonly Dictionary<FunctionSymbol, MethodDefinition> _methods = new();
    private readonly Dictionary<VariableSymbol, VariableDefinition> _locals = new();
    
    private TypeDefinition _typeDefinition;
    private Emitter(string moduleName, string[] references)
    {
        var assemblies = new List<AssemblyDefinition>();

        foreach (var reference in references)
        {
            try
            {
                var assembly = AssemblyDefinition.ReadAssembly(reference);
                assemblies.Add(assembly);

            }
            catch (BadImageFormatException)
            {
                _diagnostics.ReportInvalidReference(reference);
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
        _assemmblyDefinition = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Console);
        _knowTypes = new Dictionary<TypeSymbol, TypeReference>();

        foreach (var (typeSymbol, metadataName) in builtInTypes)
        {
            var typeReference = ResolveType(typeSymbol.Name, metadataName);
            _knowTypes.Add(typeSymbol, typeReference);
        }

        TypeReference ResolveType(string sparkName, string metadataName)
        {
            var foundTypes = assemblies.SelectMany(a => a.Modules)
                                       .SelectMany(m => m.Types)
                                       .Where(t => t.FullName == metadataName)
                                       .ToArray();
            if (foundTypes.Length == 1)
            {
                var typeReference = _assemmblyDefinition.MainModule.ImportReference(foundTypes[0]);
                return typeReference;
            }
            else if (foundTypes.Length == 0)
            {
                _diagnostics.ReportRequiredTypeNotFound(sparkName, metadataName);
            }
            else if (foundTypes.Length > 1)
            {
                _diagnostics.ReportRequiredTypeAmbiguous(sparkName, metadataName, foundTypes);
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

                foreach (var method in methods)
                {
                    if (method.Parameters.Count != parameterTypeNames.Length)
                        continue;

                    var allParametersMath = true;

                    for (var i = 0; i < parameterTypeNames.Length; i++)
                    {
                        if (method.Parameters[i].ParameterType.FullName != parameterTypeNames[i])
                        {
                            allParametersMath = false;
                            break;
                        }
                    }

                    if (!allParametersMath)
                        continue;

                    return _assemmblyDefinition.MainModule.ImportReference(method);
                }

                _diagnostics.ReportRequiredMethodNotFound(typeName, methodName, parameterTypeNames);
                return null;
            }
            else if (foundTypes.Length == 0)
            {
                _diagnostics.ReportRequiredTypeNotFound(null, typeName);
            }
            else
            {
                _diagnostics.ReportRequiredTypeAmbiguous(null, typeName, foundTypes);
            }
            return null;
        }

        _consoleReadLineReference = ResolveMethod("System.Console", "ReadLine", Array.Empty<string>());
        _consoleWriteLineReference = ResolveMethod("System.Console", "WriteLine", new[] { "System.String" });
        _stringConcatReference = ResolveMethod("System.String", "Concat", new[] { "System.String", "System.String" });
    }

    public static ImmutableArray<Diagnostic> Emit(BoundProgram program, string moduleName, string[] references, string outputPath)
    {
        if (program.Diagnostics.Any())
            return program.Diagnostics;

        var emitter = new Emitter(moduleName, references);
        return emitter.Emit(program, outputPath);
    }
    //public ImmutableArray<Diagnostic> GetDiagnostics() => _diagnostics.ToImmutableArray();
    public ImmutableArray<Diagnostic> Emit(BoundProgram program, string outputPath)
    {
        if (_diagnostics.Any())
            return _diagnostics.ToImmutableArray();

        var objectType = _knowTypes[TypeSymbol.Any];
        _typeDefinition = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Public, objectType);
        _assemmblyDefinition.MainModule.Types.Add(_typeDefinition);


        foreach(var functionWithBody in program.Functions)
        {
            EmitFunctionDeclaration(functionWithBody.Key);
        }

        foreach (var functionWithBody in program.Functions)
        {
            EmitFunctionBody(functionWithBody.Key, functionWithBody.Value);
        }

        if (program.MainFunction != null)
            _assemmblyDefinition.EntryPoint = _methods[program.MainFunction];

        _assemmblyDefinition.Write(outputPath);

        return _diagnostics.ToImmutableArray();
    }

    private void EmitFunctionDeclaration(FunctionSymbol function)
    {
        var voidType = _knowTypes[TypeSymbol.Void];
        var method = new MethodDefinition(function.Name, MethodAttributes.Static | MethodAttributes.Private, voidType);
        _typeDefinition.Methods.Add(method);
        _methods.Add(function, method);
    }
    private void EmitFunctionBody(FunctionSymbol function, BoundBlockStatement body)
    {
        var method = _methods[function];
        _locals.Clear();
        var ilProcessor = method.Body.GetILProcessor();

        foreach(var statement in body.Statements)
        {
            EmitStatement(ilProcessor, statement);
        }
        // HACK: we should make sure that our bound tree has explicit returns.
        if (function.Type == TypeSymbol.Void)
            ilProcessor.Emit(OpCodes.Ret);


        method.Body.OptimizeMacros();
    }
    private void EmitStatement(ILProcessor ilProcessor, BoundStatement node)
    {
        switch (node.Kind)
        {
            case BoundNodeKind.VariableDeclaration:
                EmitVariableDeclaration(ilProcessor, (BoundVariableDeclaration)node);
                break;
            case BoundNodeKind.LabelStatement:
                EmitLabelStatement(ilProcessor, (BoundLabelStatement)node);
                break;
            case BoundNodeKind.GotoStatement:
                EmitGotoStatement(ilProcessor,(BoundGotoStatement)node);
                break;
            case BoundNodeKind.ConditionalGotoStatement:
                EmitConditionalGotoStatement(ilProcessor,(BoundConditionalGotoStatement)node);
                break;
            case BoundNodeKind.ReturnStatement:
                EmitReturnStatement(ilProcessor,(BoundReturnStatement)node);
                break;
            case BoundNodeKind.ExpressionStatement:
                EmitExpressionStatement(ilProcessor, (BoundExpressionStatement)node);
                break;
            default:
                throw new Exception($"Unexpected node kind {node.Kind}");
        }
    }

    private void EmitVariableDeclaration(ILProcessor ilProcessor, BoundVariableDeclaration node)
    {
        var typeReference = _knowTypes[node.Variable.Type];
        var variableDefinition = new VariableDefinition(typeReference);
        _locals.Add(node.Variable, variableDefinition);
        ilProcessor.Body.Variables.Add(variableDefinition);

        EmitExpression(ilProcessor, node.Initializer);
        ilProcessor.Emit(OpCodes.Stloc, variableDefinition);
    }
    private void EmitLabelStatement(ILProcessor ilProcessor, BoundLabelStatement node)
    {

    }
    private void EmitGotoStatement(ILProcessor ilProcessor, BoundGotoStatement node)
    {

    }
    private void EmitConditionalGotoStatement(ILProcessor ilProcessor, BoundConditionalGotoStatement node)
    {

    }
    private void EmitReturnStatement(ILProcessor ilProcessor, BoundReturnStatement node)
    {

    }
    private void EmitExpressionStatement(ILProcessor ilProcessor, BoundExpressionStatement node)
    {
        EmitExpression(ilProcessor, node.Expression);

        if (node.Expression.Type != TypeSymbol.Void)
            ilProcessor.Emit(OpCodes.Pop);

    }

    private void EmitExpression(ILProcessor ilProcessor, BoundExpression node)
    {
        switch (node.Kind)
        {
            case BoundNodeKind.LiteralExpression:
                EmitLiteralExpression(ilProcessor, (BoundLiteralExpression)node);
                break;
            case BoundNodeKind.VariableExpression:
                EmitVariableExpression(ilProcessor, (BoundVariableExpression)node);
                break;
            case BoundNodeKind.AssignmentExpression:
                EmitAssignmentExpression(ilProcessor, (BoundAssignmentExpression)node);
                break;
            case BoundNodeKind.UnaryExpression:
                EmitUnaryExpression(ilProcessor, (BoundUnaryExpression)node);
                break;
            case BoundNodeKind.BinaryExpression:
                EmitBinaryExpression(ilProcessor, (BoundBinaryExpression)node);
                break;
            case BoundNodeKind.CallExpression:
                EmitCallExpression(ilProcessor, (BoundCallExpression)node);
                break;
            case BoundNodeKind.ConversionExpression:
                EmitConversionExpression(ilProcessor, (BoundConversionExpression)node);
                break;
            default:
                throw new Exception($"Unexpected node kind {node.Kind}");
        }
    }

    private void EmitLiteralExpression(ILProcessor ilProcessor, BoundLiteralExpression node)
    {
        if(node.Type == TypeSymbol.Bool)
        {
            var value = (bool)node.Value;
            var instruction = value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
            ilProcessor.Emit(instruction);
        }
        else if (node.Type == TypeSymbol.Int)
        {
            var value = (int)node.Value;
            ilProcessor.Emit(OpCodes.Ldc_I4, value);
        }
        else if (node.Type == TypeSymbol.String)
        {
            var value = (string)node.Value;
            ilProcessor.Emit(OpCodes.Ldstr, value);
        }
        else
        {
            throw new Exception($"Unexpected literal type: {node.Type}");
        }
    }
    private void EmitVariableExpression(ILProcessor ilProcessor, BoundVariableExpression node)
    {
        var variableDefinition = _locals[node.Variable];
        ilProcessor.Emit(OpCodes.Ldloc, variableDefinition);
    }
    private void EmitAssignmentExpression(ILProcessor ilProcessor, BoundAssignmentExpression node)
    {

    }
    private void EmitUnaryExpression(ILProcessor ilProcessor, BoundUnaryExpression node)
    {

    }
    private void EmitBinaryExpression(ILProcessor ilProcessor, BoundBinaryExpression node)
    {
        if (node.Op.Kind == BoundBinaryOperatorKind.Addition)
        {
            if (node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String)
            {
                EmitExpression(ilProcessor, node.Left);
                EmitExpression(ilProcessor, node.Right);
                ilProcessor.Emit(OpCodes.Call, _stringConcatReference);
                return;
            }
            else
            {

            }
        }
        else
        {

        }
    }
    private void EmitCallExpression(ILProcessor ilProcessor, BoundCallExpression node)
    {
        foreach (var argument in node.Arguments)
        {
            EmitExpression(ilProcessor, argument);
        }

        if (node.Function == BuiltinFunctions.Input)
        {
            ilProcessor.Emit(OpCodes.Call, _consoleReadLineReference);
        }
        else if (node.Function == BuiltinFunctions.Print)
        {
            ilProcessor.Emit(OpCodes.Call, _consoleWriteLineReference);
        }
        else if (node.Function == BuiltinFunctions.Random)
        {

        }
        else
        {
            var methodDefinition = _methods[node.Function];
            ilProcessor.Emit(OpCodes.Call, methodDefinition);
        }
    }
    private void EmitConversionExpression(ILProcessor ilProcessor, BoundConversionExpression node)
    {

    }
}
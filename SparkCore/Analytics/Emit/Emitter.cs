using System;
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
using SparkCore.Analytics.Syntax;
using SparkCore.IO.Diagnostics;

namespace SparkCore.Analytics.Emit;
internal sealed class Emitter
{
    private readonly DiagnosticBag _diagnostics = new();

    private readonly Dictionary<TypeSymbol, TypeReference> _knowTypes;
    private readonly MethodReference _objectEqualsReference;
    private readonly MethodReference _consoleWriteLineReference;
    private readonly MethodReference _consoleReadLineReference;
    private readonly MethodReference _stringConcatReference;
    private readonly MethodReference _convertToBooleanReference;
    private readonly MethodReference _convertToInt32Reference;
    private readonly MethodReference _convertToStringReference;
    private readonly TypeReference _randomReference;
    private readonly MethodReference _randomCtorReference;
    private readonly MethodReference _randomNextReference;
    private readonly AssemblyDefinition _assemmblyDefinition;
    private readonly Dictionary<FunctionSymbol, MethodDefinition> _methods = new();
    private readonly Dictionary<VariableSymbol, VariableDefinition> _locals = new();
    private readonly Dictionary<BoundLabel,int> _labels = new();
    private readonly List<(int InstructionIndex, BoundLabel Target)> _fixuds = new();
    
    private TypeDefinition _typeDefinition;
    private FieldDefinition _randomFieldDefinition;

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

        var a = _assemmblyDefinition.ToString();
        _objectEqualsReference = ResolveMethod("System.Object", "Equals", new[] { "System.Object", "System.Object" });
        _consoleReadLineReference = ResolveMethod("System.Console", "ReadLine", Array.Empty<string>());
        _consoleWriteLineReference = ResolveMethod("System.Console", "WriteLine", new[] { "System.Object" });
        _stringConcatReference = ResolveMethod("System.String", "Concat", new[] { "System.String", "System.String" });
        _convertToBooleanReference = ResolveMethod("System.Convert", "ToBoolean", new[] { "System.Object" });
        _convertToInt32Reference = ResolveMethod("System.Convert", "ToInt32", new[] { "System.Object" });
        _convertToStringReference = ResolveMethod("System.Convert", "ToString", new[] { "System.Object" });
        _randomReference = ResolveType(null, "System.Random");
        _randomCtorReference = ResolveMethod("System.Random", ".ctor", Array.Empty<string>());
        _randomNextReference = ResolveMethod("System.Random", "Next", new[] { "System.Int32" });

        var objectType = _knowTypes[TypeSymbol.Any];
        if(objectType != null)
        {
            _typeDefinition = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, objectType);
            _assemmblyDefinition.MainModule.Types.Add(_typeDefinition);
        }
        else
        {
            _typeDefinition = null;
        }
    }
     
    public static ImmutableArray<Diagnostic> Emit(BoundProgram program, string moduleName, string[] references, string outputPath)
    {
        if (program.Diagnostics.Any())
            return program.Diagnostics;

        var emitter = new Emitter(moduleName, references);
        return emitter.Emit(program, outputPath);
    }
    public ImmutableArray<Diagnostic> Emit(BoundProgram program, string outputPath)
    {
        if (_diagnostics.Any())
            return _diagnostics.ToImmutableArray();

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
        var functionType = _knowTypes[function.Type];
        var method = new MethodDefinition(function.Name, MethodAttributes.Static | MethodAttributes.Private, functionType);

        foreach(var parameter in function.Parameters)
        {
            var parameterType = _knowTypes[parameter.Type];
            var parameterAttribute = ParameterAttributes.None;
            var parameterDefinition = new ParameterDefinition(parameter.Name, parameterAttribute, parameterType);
            method.Parameters.Add(parameterDefinition);
        }

        _typeDefinition.Methods.Add(method);
        _methods.Add(function, method);
    }
    private void EmitFunctionBody(FunctionSymbol function, BoundBlockStatement body)
    {
        var method = _methods[function];
        _locals.Clear();
        _labels.Clear();
        _fixuds.Clear();

        var ilProcessor = method.Body.GetILProcessor();

        foreach(var statement in body.Statements)
        {
            EmitStatement(ilProcessor, statement);
        }

        foreach(var fixup in _fixuds)
        {
            var targetLabel = fixup.Target;
            var targetInstructionIndex = _labels[targetLabel];
            var targetInstruction = ilProcessor.Body.Instructions[targetInstructionIndex];
            var instructionToFixup = ilProcessor.Body.Instructions[fixup.InstructionIndex];
            instructionToFixup.Operand = targetInstruction;
        }

        method.Body.Optimize();
        //method.Body.OptimizeMacros();
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
        _labels.Add(node.Label, ilProcessor.Body.Instructions.Count);
    }
    private void EmitGotoStatement(ILProcessor ilProcessor, BoundGotoStatement node)
    {
        _fixuds.Add((ilProcessor.Body.Instructions.Count, node.Label));
        ilProcessor.Emit(OpCodes.Br, Instruction.Create(OpCodes.Nop));
    }
    private void EmitConditionalGotoStatement(ILProcessor ilProcessor, BoundConditionalGotoStatement node)
    {
        EmitExpression(ilProcessor, node.Condition);

        var opCode = node.JumpIfTrue ? OpCodes.Brtrue : OpCodes.Brfalse;
        _fixuds.Add((ilProcessor.Body.Instructions.Count, node.Label));
        ilProcessor.Emit(opCode, Instruction.Create(OpCodes.Nop));
    }
    private void EmitReturnStatement(ILProcessor ilProcessor, BoundReturnStatement node)
    {
        if(node.Expression != null)
            EmitExpression(ilProcessor, node.Expression);
        ilProcessor.Emit(OpCodes.Ret);
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
        if(node.Variable is ParameterSymbol parameter)
        {
            ilProcessor.Emit(OpCodes.Ldarg, parameter.Ordinal);
        }
        else
        {
            var variableDefinition = _locals[node.Variable];
            ilProcessor.Emit(OpCodes.Ldloc, variableDefinition);
        }
    }
    private void EmitAssignmentExpression(ILProcessor ilProcessor, BoundAssignmentExpression node)
    {
        var variableDefinition = _locals[node.Variable];
        EmitExpression(ilProcessor, node.Expression);
        ilProcessor.Emit(OpCodes.Dup);
        ilProcessor.Emit(OpCodes.Stloc, variableDefinition);
    }
    private void EmitUnaryExpression(ILProcessor ilProcessor, BoundUnaryExpression node)
    {
        EmitExpression(ilProcessor, node.Operand);

        if(node.Op.Kind == BoundUnaryOperatorKind.Identity)
        {
            // Nothing to do
        }
        else if(node.Op.Kind == BoundUnaryOperatorKind.LogicalNegation)
        {
            ilProcessor.Emit(OpCodes.Ldc_I4_0);
            ilProcessor.Emit(OpCodes.Ceq);
        }
        else if(node.Op.Kind == BoundUnaryOperatorKind.Negation)
        {
            ilProcessor.Emit(OpCodes.Neg);
        }
        else if(node.Op.Kind == BoundUnaryOperatorKind.OnesComplement)
        {
            ilProcessor.Emit(OpCodes.Not);
        }
        else
        {
            throw new Exception($"Unexpected unary operator {SyntaxFacts.GetText(node.Op.SyntaxKind)}{node.Operand.Type}");
        }
    }
    private void EmitBinaryExpression(ILProcessor ilProcessor, BoundBinaryExpression node)
    {

        EmitExpression(ilProcessor, node.Left);
        EmitExpression(ilProcessor, node.Right);
        
        // +(string, string)
        if (node.Op.Kind == BoundBinaryOperatorKind.Addition)
        {
            if (node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String)
            {
                ilProcessor.Emit(OpCodes.Call, _stringConcatReference);
                return;
            }
        }

        // ==(any, any)
        // ==(string, string)
        if (node.Op.Kind == BoundBinaryOperatorKind.Equals)
        {
            if (node.Left.Type == TypeSymbol.Any && node.Right.Type == TypeSymbol.Any ||
                node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String)
            {
                ilProcessor.Emit(OpCodes.Call, _objectEqualsReference);
                return;
            }
        }

        // !=(any, any)
        // !=(string, string)
        else if (node.Op.Kind == BoundBinaryOperatorKind.NotEquals)
        {
            if (node.Left.Type == TypeSymbol.Any && node.Right.Type == TypeSymbol.Any ||
                node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String)
            {
                ilProcessor.Emit(OpCodes.Call, _objectEqualsReference);
                ilProcessor.Emit(OpCodes.Ldc_I4_0);
                ilProcessor.Emit(OpCodes.Ceq);
                return;
            }
        }

        switch (node.Op.Kind)
        {
            case BoundBinaryOperatorKind.Addition:
                ilProcessor.Emit(OpCodes.Add);
                break;
            case BoundBinaryOperatorKind.Substraction:
                ilProcessor.Emit(OpCodes.Sub);
                break;
            case BoundBinaryOperatorKind.Multiplication:
                ilProcessor.Emit(OpCodes.Mul);
                break;
            case BoundBinaryOperatorKind.Division:
                ilProcessor.Emit(OpCodes.Div);
                break;
            // TODO: Implement short-circuit evaluation
            case BoundBinaryOperatorKind.LogicalAnd:
            case BoundBinaryOperatorKind.BitwiseAnd:
                ilProcessor.Emit(OpCodes.And);
                break;
            // TODO: Implement short-circuit evaluation
            case BoundBinaryOperatorKind.LogicalOr:
            case BoundBinaryOperatorKind.BitwiseOr:
                ilProcessor.Emit(OpCodes.Or);
                break;
            case BoundBinaryOperatorKind.BitwiseXor:
                ilProcessor.Emit(OpCodes.Xor);
                break;
            case BoundBinaryOperatorKind.Equals:
                ilProcessor.Emit(OpCodes.Ceq);
                break;
            case BoundBinaryOperatorKind.NotEquals:
                ilProcessor.Emit(OpCodes.Ceq);
                ilProcessor.Emit(OpCodes.Ldc_I4_0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;
            case BoundBinaryOperatorKind.Less:
                ilProcessor.Emit(OpCodes.Clt);
                break;
            case BoundBinaryOperatorKind.LessOrEquals:
                ilProcessor.Emit(OpCodes.Cgt);
                ilProcessor.Emit(OpCodes.Ldc_I4_0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;
            case BoundBinaryOperatorKind.Greater:
                ilProcessor.Emit(OpCodes.Cgt);
                break;
            case BoundBinaryOperatorKind.GreaterOrEquals:
                ilProcessor.Emit(OpCodes.Clt);
                ilProcessor.Emit(OpCodes.Ldc_I4_0);
                ilProcessor.Emit(OpCodes.Ceq);
                break;
            default:
                throw new Exception($"Unexpectede binary operator {SyntaxFacts.GetText(node.Op.SyntaxKind)}");
        }


    }
    private void EmitCallExpression(ILProcessor ilProcessor, BoundCallExpression node)
    {

        if (node.Function == BuiltinFunctions.Random)
        {
            if (_randomFieldDefinition == null)
            {
                EmitRandomField();
            }

            ilProcessor.Emit(OpCodes.Ldsfld, _randomFieldDefinition);
            foreach (var argument in node.Arguments)
            {
                EmitExpression(ilProcessor, argument);
            }
            ilProcessor.Emit(OpCodes.Callvirt, _randomNextReference);
            return;
        }

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
        else
        {
            var methodDefinition = _methods[node.Function];
            ilProcessor.Emit(OpCodes.Call, methodDefinition);
        }
    }

    private void EmitRandomField()
    {
        _randomFieldDefinition = new FieldDefinition("$random",
                                                     FieldAttributes.Static | FieldAttributes.Private,
                                                     _randomReference);
        _typeDefinition.Fields.Add(_randomFieldDefinition);

        var staticConstructor = new MethodDefinition(".cctor",
                                                     MethodAttributes.Static | 
                                                     MethodAttributes.Private | 
                                                     MethodAttributes.SpecialName | 
                                                     MethodAttributes.RTSpecialName,
                                                     _knowTypes[TypeSymbol.Void]);
        _typeDefinition.Methods.Insert(0, staticConstructor);

        var ilProcessor = staticConstructor.Body.GetILProcessor();
        ilProcessor.Emit(OpCodes.Newobj, _randomCtorReference);
        ilProcessor.Emit(OpCodes.Stsfld, _randomFieldDefinition);
        ilProcessor.Emit(OpCodes.Ret);
    }

    private void EmitConversionExpression(ILProcessor ilProcessor, BoundConversionExpression node)
    {
        EmitExpression(ilProcessor, node.Expression);
        var needsBoxing = node.Expression.Type == TypeSymbol.Bool ||
                          node.Expression.Type == TypeSymbol.Int;
        if (needsBoxing)
        {
            ilProcessor.Emit(OpCodes.Box, _knowTypes[node.Expression.Type]);
        }

        if(node.Type == TypeSymbol.Any)
        {
            //Nothing to do.
        }
        else if(node.Type == TypeSymbol.Bool)
        {
            ilProcessor.Emit(OpCodes.Call, _convertToBooleanReference);
        }
        else if(node.Type == TypeSymbol.Int)
        {
            ilProcessor.Emit(OpCodes.Call, _convertToInt32Reference);
        }
        else if(node.Type == TypeSymbol.String)
        {
            ilProcessor.Emit(OpCodes.Call, _convertToStringReference);
        }
        else
        {
            throw new Exception($"Unexpected convertion from {node.Expression.Type} to {node.Type}.");
        }
    }
}

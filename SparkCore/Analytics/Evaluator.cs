﻿using System;
using System.Collections.Generic;
using SparkCore.Analytics.Binding;
using SparkCore.Analytics.Binding.Tree;
using SparkCore.Analytics.Binding.Tree.Expressions;
using SparkCore.Analytics.Binding.Tree.Statements;
using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics;

internal class Evaluator
{
    private readonly BoundBlockStatement _root;
    private readonly Dictionary<VariableSymbol, object> _variables;

    private object _lastValue;

    public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
    {
        _root = root;
        _variables = variables;
    }
    public object Evaluate()
    {
        var labelToIndex = new Dictionary<BoundLabel, int>();

        for (var i = 0; i < _root.Statements.Length; i++)
        {
            if (_root.Statements[i] is BoundLabelStatement l)
            {
                labelToIndex.Add(l.Label, i + 1);
            }
        }
        var index = 0;
        while (index < _root.Statements.Length)
        {
            var s = _root.Statements[index];
            switch (s.Kind)
            {
                case BoundNodeKind.VariableDeclaration:
                    EvaluateVariableDeclaration((BoundVariableDeclaration)s);
                    index++;
                    break;
                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement)s);
                    index++;
                    break;
                case BoundNodeKind.GotoStatement:
                    var gs = (BoundGotoStatement)s;
                    index = labelToIndex[gs.Label];
                    break;
                case BoundNodeKind.ConditionalGotoStatement:
                    var cgs = (BoundConditionalGotoStatement)s;
                    var condition = (bool)EvaluateExpression(cgs.Condition)!;
                    if (condition == cgs.JumpIfTrue)
                        index = labelToIndex[cgs.Label];
                    else
                        index++;
                    break;
                case BoundNodeKind.LabelStatement:
                    index++;
                    break;
                default:
                    throw new Exception($"Unexpected node operator {s.Kind}");
            }
        }

        return _lastValue;
    }

    private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
    {
        var value = EvaluateExpression(node.Initializer);
        _variables[node.Variable] = value;
        _lastValue = value;
    }

    private void EvaluateExpressionStatement(BoundExpressionStatement node)
    {
        _lastValue = EvaluateExpression(node.Expression);
    }

    private object EvaluateExpression(BoundExpression node)
    {
        switch (node.Kind)
        {
            case BoundNodeKind.LiteralExpression:
                return EvaluateLiteralExpression((BoundLiteralExpression)node);
            case BoundNodeKind.VariableExpression:
                return EvaluateVariableExprression((BoundVariableExpression)node);
            case BoundNodeKind.AssignmentExpression:
                return EvaluateAssigmentExpression((BoundAssignmentExpression)node);
            case BoundNodeKind.UnaryExpression:
                return EvaluateUnaryExpression((BoundUnaryExpression)node);
            case BoundNodeKind.BinaryExpression:
                return EvaluateBinaryExpression((BoundBinaryExpression)node);
            case BoundNodeKind.CallExpression:
                return EvaluateCallExpression((BoundCallExpression)node);
            default:
                throw new Exception($"Unexpected node operator {node.Type}");
        }
    }

    private static object EvaluateLiteralExpression(BoundLiteralExpression n)
    {
        return n.Value;
    }

    private object EvaluateVariableExprression(BoundVariableExpression v)
    {
        return _variables[v.Variable];
    }

    private object EvaluateAssigmentExpression(BoundAssignmentExpression a)
    {
        var value = EvaluateExpression(a.Expression);
        _variables[a.Variable] = value;

        return value;
    }

    private object EvaluateUnaryExpression(BoundUnaryExpression u)
    {
        var operand = EvaluateExpression(u.Operand);
        switch (u.Op.Kind)
        {
            case BoundUnaryOperatorKind.Negation:
                return -(int)operand;
            case BoundUnaryOperatorKind.Identity:
                return (int)operand;
            case BoundUnaryOperatorKind.LogicalNegation:
                return !(bool)operand;
            case BoundUnaryOperatorKind.OnesComplement:
                return ~(int)operand;
            default:
                throw new Exception($"Unexpected unary operator {u.Op}");
        }
    }

    private object EvaluateBinaryExpression(BoundBinaryExpression b)
    {
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);
        switch (b.Op.Kind)
        {
            case BoundBinaryOperatorKind.Addition:
                if (b.Type == TypeSymbol.Int)
                    return (int)left + (int)right;
                else
                    return (string)left + (string)right;
            case BoundBinaryOperatorKind.Substraction:
                return (int)left - (int)right;
            case BoundBinaryOperatorKind.Multiplication:
                return (int)left * (int)right;
            case BoundBinaryOperatorKind.Division:
                return (int)left / (int)right;
            case BoundBinaryOperatorKind.BitwiseAnd:
                if (b.Type == TypeSymbol.Int)
                    return (int)left & (int)right;
                else
                    return (bool)left & (bool)right;
            case BoundBinaryOperatorKind.BitwiseOr:
                if (b.Type == TypeSymbol.Int)
                    return (int)left | (int)right;
                else
                    return (bool)left | (bool)right;
            case BoundBinaryOperatorKind.BitwiseXor:
                if (b.Type == TypeSymbol.Int)
                    return (int)left ^ (int)right;
                else
                    return (bool)left ^ (bool)right;
            case BoundBinaryOperatorKind.LogicalAnd:
                return (bool)left && (bool)right;
            case BoundBinaryOperatorKind.LogicalOr:
                return (bool)left || (bool)right;
            case BoundBinaryOperatorKind.Equals:
                return Equals(left, right);
            case BoundBinaryOperatorKind.NotEquals:
                return !Equals(left, right);
            case BoundBinaryOperatorKind.Less:
                return (int)left < (int)right;
            case BoundBinaryOperatorKind.LessOrEquals:
                return (int)left <= (int)right;
            case BoundBinaryOperatorKind.Greater:
                return (int)left > (int)right;
            case BoundBinaryOperatorKind.GreaterOrEquals:
                return (int)left >= (int)right;
            default:
                throw new Exception($"Unexpected binary operator {b.Op}");
        }
    }

    private object EvaluateCallExpression(BoundCallExpression node)
    {
        if (node.Function == BuiltinFunctions.Input)
        {
            return Console.ReadLine();
        }else if (node.Function == BuiltinFunctions.Print)
        {
            var message = (string)EvaluateExpression(node.Arguments[0]);
            Console.WriteLine(message);
            return null;
        }
        else
        {
            throw new Exception($"Unexpected function {node.Function}.");
        }
    }

}
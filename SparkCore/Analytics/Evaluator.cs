﻿using SparkCore.Analytics.Binding;
using SparkCore.Analytics.Binding.Scope;
using SparkCore.Analytics.Binding.Scope.Expressions;
using SparkCore.Analytics.Binding.Scope.Statements;
using System;
using System.Collections.Generic;

namespace SparkCore.Analytics
{
    internal class Evaluator
    {
        private readonly BoundStatement _root;
        private readonly Dictionary<VariableSymbol, object> _variables;

        private object _lastValue;

        public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }
        public object Evaluate()
        {
            EvaluateStatement(_root);
            return _lastValue;
        }
        private void EvaluateStatement(BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BlockStatement:
                    EvaluateBlockStatement((BoundBlockStatement)node);
                    break;
                case BoundNodeKind.VariableDeclaration:
                    EvaluateVariableDeclaration((BoundVariableDeclaration)node);
                    break;
                case BoundNodeKind.IfStatement:
                    EvaluateIfStatement((BoundIfStatement)node);
                    break;
                case BoundNodeKind.WhileStatement:
                    EvaluateWhileStatement((BoundWhileStatement)node);
                    break;
                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement)node);
                    break;
                default:
                    throw new Exception($"Unexpected node operator {node.Kind}");
            }
        }

        private void EvaluateBlockStatement(BoundBlockStatement node)
        {
            foreach(var statement in node.Statements)
                EvaluateStatement(statement);
        }
        private void EvaluateIfStatement(BoundIfStatement node)
        {
            var condition = (bool)EvaluateExpression(node.Condition);
            if (condition)
            {
                EvaluateStatement(node.ThenStatement);
            }else if(node.ElseStatement != null)
            {
                EvaluateStatement(node.ElseStatement);
            }
        }
        private void EvaluateWhileStatement(BoundWhileStatement node)
        {
            while ((bool)EvaluateExpression(node.Condition))
                EvaluateStatement(node.Body);
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
                    return (int)left + (int)right;
                case BoundBinaryOperatorKind.Substraction:
                    return (int)left - (int)right;
                case BoundBinaryOperatorKind.Multiplication:
                    return (int)left * (int)right;
                case BoundBinaryOperatorKind.Division:
                    return (int)left / (int)right;
                case BoundBinaryOperatorKind.BitwiseAnd:
                    if (b.Type == typeof(int))
                        return (int)left & (int)right;
                    else
                        return (bool)left & (bool)right;
                case BoundBinaryOperatorKind.BitwiseOr:
                    if (b.Type == typeof(int))
                        return (int)left | (int)right;
                    else
                        return (bool)left | (bool)right;
                case BoundBinaryOperatorKind.BitwiseXor:
                    if (b.Type == typeof(int))
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
    }
}
﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SparkCore.Analytics.Binding;
using SparkCore.Analytics.Binding.Tree;
using SparkCore.Analytics.Binding.Tree.Expressions;
using SparkCore.Analytics.Binding.Tree.Statements;
using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
    private int _labelCount;
    private Lowerer()
    {
    }
    private BoundLabel GenerateLabel()
    {
        var name = $"Label{++_labelCount}";
        return new BoundLabel(name);
    }
    /// <summary>
    /// Reduce a given statement tree to its minimun.
    /// </summary>
    /// <param name="statement"></param>
    /// <returns></returns>
    public static BoundBlockStatement Lower(BoundStatement statement)
    {
        var lowerer = new Lowerer();
        var result = lowerer.RewriteStatement(statement);
        return Flatten(result);
    }
    /// <summary>
    /// Flatten a statement tree to a secuence of statements.
    /// </summary>
    /// <param name="statement">the statement tree to flatten.</param>
    /// <returns></returns>
    private static BoundBlockStatement Flatten(BoundStatement statement)
    {
        var builder = ImmutableArray.CreateBuilder<BoundStatement>();
        var stack = new Stack<BoundStatement>();
        stack.Push(statement);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (current is BoundBlockStatement block)
            {
                foreach (var s in block.Statements.Reverse())
                    stack.Push(s);
            }
            else
            {
                builder.Add(current);
            }
        }
        return new BoundBlockStatement(builder.ToImmutable());
    }
    protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
    {
        if (node.ElseStatement == null)
        {
            // if <condition>
            //      <then>
            //
            //--->
            // gotoIfFalse <condition> end
            // <then>
            // end;
            var endLabel = GenerateLabel();
            var gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, false);
            var endLabelStatement = new BoundLabelStatement(endLabel);
            var result = new BoundBlockStatement(ImmutableArray.Create(
                gotoFalse,
                node.ThenStatement,
                endLabelStatement
            ));
            return RewriteStatement(result);
        }
        else
        {
            //if <condition>
            //      <then>
            //else
            //      <else>.
            //--->
            // gotoIfFalse <condition> else
            // <then>
            // goto end
            // else:
            // <else>
            // end:
            var elseLabel = GenerateLabel();
            var endLabel = GenerateLabel();

            var gotoFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, false);
            var gotoEndStatement = new BoundGotoStatement(endLabel);
            var elseLabelStatement = new BoundLabelStatement(elseLabel);
            var endLabelStatement = new BoundLabelStatement(endLabel);
            var result = new BoundBlockStatement(ImmutableArray.Create(
                gotoFalse,
                node.ThenStatement,
                gotoEndStatement,
                elseLabelStatement,
                node.ElseStatement,
                endLabelStatement
            ));
            return RewriteStatement(result);
        }
    }

    protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
    {
        // while <condition>
        //      <body>
        //
        //------>
        //
        //goto check
        //continue:
        //<body>
        //check:
        //gotoTrue <condition> continue:
        //

        var continueLabel = GenerateLabel();
        var checkLabel = GenerateLabel();

        var gotoCheck = new BoundGotoStatement(checkLabel);
        var continueLabelStatement = new BoundLabelStatement(continueLabel);
        var checkLabelStatement = new BoundLabelStatement(checkLabel);
        var gotoTrue = new BoundConditionalGotoStatement(continueLabel, node.Condition);

        var result = new BoundBlockStatement(ImmutableArray.Create(
                gotoCheck,
                continueLabelStatement,
                node.Body,
                checkLabelStatement,
                gotoTrue
            ));

        return RewriteStatement(result);
    }
    
    protected override BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
    {
        // do
        //      <body>
        // while <condition>
        //------>
        //
        // 
        // continue:
        // <body>
        // gotoTrue <condition> continue:
        //

        var continueLabel = GenerateLabel();

        var continueLabelStatement = new BoundLabelStatement(continueLabel);
        var gotoTrue = new BoundConditionalGotoStatement(continueLabel, node.Condition);

        var result = new BoundBlockStatement(ImmutableArray.Create(
                continueLabelStatement,
                node.Body,
                gotoTrue
            ));

        return RewriteStatement(result);
    }

    protected override BoundStatement RewriteForStatement(BoundForStatement node)
    {
        // for <var> = <lower> to <upper>
        //      <body>
        //
        // ------->
        //
        // {
        //   var <var> = <lower>
        //   while(<var> <= <upper>)
        //   {
        //       <body>
        //       <var> = <var> + 1
        //   }
        // }
        var variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);
        var variableExpression = new BoundVariableExpression(node.Variable);
        var upperBoundSymbol = new LocalVariableSymbol("upperBound", true, TypeSymbol.Int);
        var upperBoundDeclaration = new BoundVariableDeclaration(upperBoundSymbol, node.UpperBound);
        var condition = new BoundBinaryExpression(
            variableExpression,
            BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, TypeSymbol.Int, TypeSymbol.Int),
            new BoundVariableExpression(upperBoundSymbol)
        );
        var increment = new BoundExpressionStatement(
            new BoundAssignmentExpression(
                node.Variable,
                new BoundBinaryExpression(
                    variableExpression,
                    BoundBinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Int, TypeSymbol.Int),
                    new BoundLiteralExpression(1)
                )
            )
        ); ;

        var whileBody = new BoundBlockStatement(ImmutableArray.Create(node.Body, increment));
        var whileStatement = new BoundWhileStatement(condition, whileBody);
        var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
            variableDeclaration,
            upperBoundDeclaration,
            whileStatement
        ));

        return RewriteStatement(result);
    }

}
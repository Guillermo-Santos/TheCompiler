using System;
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
    // A counter for all the generated labels.
    private int _labelCount;
    // A counter for all the generate temporal variables.
    private int _tVariableCount;
    private Lowerer()
    {
    }
    private BoundLabel GenerateLabel()
    {
        var name = $"Label{++_labelCount}";
        return new BoundLabel(name);
    }
    private LocalVariableSymbol GenerateTemporalVariable(TypeSymbol type)
    {
        var name = $"T{++_tVariableCount}";
        return new LocalVariableSymbol(name, isReadOnly: true, type);
    }
    /// <summary>
    /// Reduce a given statement tree to its minimun.
    /// </summary>
    /// <param name="statement"></param>
    /// <returns></returns>
    public static BoundBlockStatement Lower(FunctionSymbol function, BoundStatement statement)
    {
        var lowerer = new Lowerer();
        var result = lowerer.RewriteStatement(statement);
        return Flatten(function, result);
    }
    /// <summary>
    /// Flatten a statement tree to a secuence of statements.
    /// </summary>
    /// <param name="statement">the statement tree to flatten.</param>
    /// <returns></returns>
    private static BoundBlockStatement Flatten(FunctionSymbol function, BoundStatement statement)
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
        if(function.Type == TypeSymbol.Void)
        {
            if(builder.Count == 0 || CanFallThrough(builder.Last())) 
            {
                builder.Add(new BoundReturnStatement(null));
            }
        }

        return new BoundBlockStatement(builder.ToImmutable());
    }

    private static bool CanFallThrough(BoundStatement boundStatement)
    {
        // TODO: we don't rewrite conditional gotos where the condition is always true.
        //       we shouldn't handle this here, because we should realy rewrite those
        //       to unconditional gotos in the first place.
        return boundStatement.Kind != BoundNodeKind.ReturnStatement &&
               boundStatement.Kind != BoundNodeKind.GotoStatement;
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
            // end:
            var endLabel = GenerateLabel();
            var gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, jumpIfTrue: false);
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

            var gotoFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, jumpIfTrue: false);
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
        //break:
        //

        var checkLabel = GenerateLabel();

        var gotoCheck = new BoundGotoStatement(checkLabel);
        var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);
        var checkLabelStatement = new BoundLabelStatement(checkLabel);
        var gotoTrue = new BoundConditionalGotoStatement(node.ContinueLabel, node.Condition);
        var breakLabelStatement = new BoundLabelStatement(node.BreakLabel);

        var result = new BoundBlockStatement(ImmutableArray.Create(
                gotoCheck,
                continueLabelStatement,
                node.Body,
                checkLabelStatement,
                gotoTrue,
                breakLabelStatement
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
        // break:
        //


        var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);
        var gotoTrue = new BoundConditionalGotoStatement(node.ContinueLabel, node.Condition);
        var breakLabelStatement = new BoundLabelStatement(node.BreakLabel);

        var result = new BoundBlockStatement(ImmutableArray.Create(
                continueLabelStatement,
                node.Body,
                gotoTrue,
                breakLabelStatement
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
        //

        var variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);
        var variableExpression = new BoundVariableExpression(node.Variable);
        var upperBoundSymbol = new LocalVariableSymbol("upperBound", true, TypeSymbol.Int);
        var upperBoundDeclaration = new BoundVariableDeclaration(upperBoundSymbol, node.UpperBound);
        var condition = new BoundBinaryExpression(
            variableExpression,
            BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, TypeSymbol.Int, TypeSymbol.Int),
            new BoundVariableExpression(upperBoundSymbol)
        );
        var continueLabelStatemnt = new BoundLabelStatement(node.ContinueLabel);
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

        var whileBody = new BoundBlockStatement(ImmutableArray.Create(
            node.Body,
            continueLabelStatemnt, 
            increment
        ));
        var whileStatement = new BoundWhileStatement(condition, whileBody, node.BreakLabel, GenerateLabel());
        var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
            variableDeclaration,
            upperBoundDeclaration,
            whileStatement
        ));

        return RewriteStatement(result);
    }

    //Comprobar si esto es realmente necesario.
    //TODO: #1 Agregar el out VariableSymbol temporalVariables to all functions and implement logic for declaration.
    //protected override BoundStatement RewriteVariableDeclaration(BoundVariableDeclaration node)
    //{

    //    if (!NeedRewriteExpression(node.Initializer, out BoundBlockStatement result))
    //    {
    //        return node;
    //    }

    //    var expressionStatement = result.Statements.Last();
    //    var initializer = ((BoundExpressionStatement)expressionStatement).Expression;

    //    // statements equals the lowering of the expression without the expression statement used in the initializer.
    //    var statements = result.Statements.Remove(expressionStatement);
    //    // then we take this initializer and create a variable declaration for the original variable and the new initializer.
    //    var variableDeclaration = new BoundVariableDeclaration(node.Variable, initializer);
    //    // and add it to the statements
    //    statements = statements.Add(variableDeclaration);

    //    return RewriteStatement(new BoundBlockStatement(statements));
    //}
    //protected override BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
    //{
    //    if (!NeedRewriteExpression(node.Expression, out var result))
    //    {
    //        return base.RewriteExpressionStatement(node);
    //    }

    //    return RewriteStatement(result);
    //}
    //protected override BoundStatement RewriteReturnStatement(BoundReturnStatement node)
    //{
    //    if (!NeedRewriteExpression(node.Expression, out BoundBlockStatement result))
    //    {
    //        return node;
    //    }

    //    var expressionStatement = result.Statements.Last();
    //    var initializer = ((BoundExpressionStatement)expressionStatement).Expression;

    //    // statements equals the lowering of the expression without the expression statement used to in the initializer.
    //    var statements = result.Statements.Remove(expressionStatement);
    //    // we create a temporal variable with the initializer.
    //    var temp = GenerateTemporalVariable(initializer.Type);
    //    var tempDeclaration = new BoundVariableDeclaration(temp, initializer);
    //    // We add the temporal variable declaration to the statements.
    //    statements = statements.Add(tempDeclaration);
    //    var returnStatement = new BoundReturnStatement(new BoundVariableExpression(temp));
    //    statements = statements.Add(returnStatement);

    //    return RewriteStatement(new BoundBlockStatement(statements));
    //}
    //private bool NeedRewriteExpression(BoundExpression expressionToEvaluate, out BoundBlockStatement result)
    //{
    //    var statements = ImmutableArray.CreateBuilder<BoundStatement>();
    //    BoundExpression GetNewChild(BoundExpression old)
    //    {
    //        // If is a variable expression or a literal expression,
    //        if (old is BoundVariableExpression || old is BoundLiteralExpression)
    //        {
    //            // We parse it as it is.
    //            return old;
    //        }
    //        else
    //        {
    //            // else, we create a temporal variable with the expression.
    //            var temp = GenerateTemporalVariable(old.Type);
    //            var tempDeclaration = new BoundVariableDeclaration(temp, old);
    //            var tempVarExpression = new BoundVariableExpression(temp);
    //            statements.Add(tempDeclaration);
    //            return tempVarExpression;
    //        }
    //    }
    //    switch (expressionToEvaluate)
    //    {
    //        case BoundBinaryExpression binary:
    //            {
    //                //
    //                // expression1 <op> expression2
    //                //
    //                //---->
    //                //
    //                // let T1 = expression1
    //                // let T2 = expression2
    //                // var a = T1 + T2
    //                //.
    //                var left = GetNewChild(binary.Left);
    //                var right = GetNewChild(binary.Right);
    //                if (left == binary.Left && right == binary.Right)
    //                {
    //                    break;
    //                }

    //                var initializer = new BoundBinaryExpression(left, binary.Op, right);
    //                statements.Add(new BoundExpressionStatement(initializer));
    //                result = new BoundBlockStatement(statements.ToImmutable());
    //                return true;
    //            }
    //        case BoundUnaryExpression unary:
    //            {
    //                //
    //                // var a = <op>expression
    //                //
    //                //---->
    //                //
    //                // let T1 = expression
    //                // <op>T1
    //                //

    //                var operand = GetNewChild(unary.Operand);
    //                if (operand == unary.Operand)
    //                {
    //                    break;
    //                }

    //                var initializer = new BoundUnaryExpression(unary.Op, operand);
    //                statements.Add(new BoundExpressionStatement(initializer));
    //                result = new BoundBlockStatement(statements.ToImmutable());
    //                return true;
    //            }
    //        case BoundAssignmentExpression assigment:
    //            {
    //                var expression = GetNewChild(assigment.Expression);
    //                if (expression == assigment.Expression)
    //                {
    //                    result = null;
    //                    return false;
    //                }

    //                var assign = new BoundAssignmentExpression(assigment.Variable, expression);
    //                statements.Add(new BoundExpressionStatement(assign));
    //                result = new BoundBlockStatement(statements.ToImmutable());
    //                return true;

    //            }
    //        case BoundConversionExpression conversion:
    //            {
    //                //
    //                // type(expression)
    //                //
    //                //---->
    //                //
    //                // let T1 = expression
    //                // type(T1)
    //                //
    //                var expression = GetNewChild(conversion.Expression);

    //                if (expression == conversion.Expression)
    //                {
    //                    break;
    //                }

    //                var conversionStatement = new BoundConversionExpression(conversion.Type, expression);
    //                statements.Add(new BoundExpressionStatement(conversionStatement));
    //                result = new BoundBlockStatement(statements.ToImmutable());
    //                return true;
    //            }
    //        case BoundCallExpression call:
    //            {
    //                //
    //                // call(expression)
    //                //
    //                //---->
    //                //
    //                // let T1 = expression
    //                // a = call(T1)
    //                //

    //                var args = ImmutableArray.CreateBuilder<BoundExpression>();
    //                foreach (var arg in call.Arguments)
    //                {
    //                    args.Add(GetNewChild(arg));
    //                }
    //                var arguments = args.ToImmutable();

    //                var isEquals = true;

    //                for (var i = 0; i < arguments.Length; i++)
    //                {
    //                    if (arguments[i] == call.Arguments[i])
    //                        continue;
    //                    else
    //                        isEquals = false;
    //                }

    //                if (isEquals)
    //                {
    //                    break;
    //                }

    //                var initializer = new BoundCallExpression(call.Function, arguments);
    //                statements.Add(new BoundExpressionStatement(initializer));
    //                result = new BoundBlockStatement(statements.ToImmutable());
    //                return true;
    //            }
    //    }
    //    result = null;
    //    return false;
    //}
}
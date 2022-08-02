﻿using System;
using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Binding.Tree.Expressions;

internal sealed class BoundLiteralExpression : BoundExpression
{
    public BoundLiteralExpression(object value)
    {
        if (value is bool)
            Type = TypeSymbol.Bool;
        else if (value is int)
            Type = TypeSymbol.Int;
        else if (value is string)
            Type = TypeSymbol.String;
        else
            throw new Exception($"Unexpected literal '{value}' of type '{value.GetType()}'");

        ConstantValue = new BoundConstant(value);
    }

    public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    public override TypeSymbol Type { get; }
    public object Value => ConstantValue.Value;
    public override BoundConstant ConstantValue
    {
        get;
    }
}


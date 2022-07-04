using System;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax;

namespace SparkCore.Analytics.Binding.Tree.Expressions
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(SyntaxKind syntaxtype, BoundBinaryOperatorKind kind, TypeSymbol alTy) :
            this(syntaxtype, kind, alTy, alTy, alTy)
        {
        }
        private BoundBinaryOperator(SyntaxKind syntaxtype, BoundBinaryOperatorKind kind, TypeSymbol operantType, TypeSymbol resultType) :
            this(syntaxtype, kind, operantType, operantType, resultType)
        {
        }
        private BoundBinaryOperator(SyntaxKind syntaxtype, BoundBinaryOperatorKind kind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol resultType)
        {
            Syntaxtype = syntaxtype;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            Type = resultType;
        }

        public SyntaxKind Syntaxtype
        {
            get;
        }
        public BoundBinaryOperatorKind Kind
        {
            get;
        }
        public TypeSymbol LeftType
        {
            get;
        }
        public TypeSymbol RightType
        {
            get;
        }
        public TypeSymbol Type
        {
            get;
        }

        private static BoundBinaryOperator[] _operators =
        {
            new BoundBinaryOperator(SyntaxKind.PlusToken,  BoundBinaryOperatorKind.Addition, TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.MinusToken,  BoundBinaryOperatorKind.Substraction, TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.PibeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Int, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Int, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.LessToken, BoundBinaryOperatorKind.Less, TypeSymbol.Int, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.LessOrEqualsToken, BoundBinaryOperatorKind.LessOrEquals, TypeSymbol.Int, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.Greater, TypeSymbol.Int, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.GreaterOrEqualsToken, BoundBinaryOperatorKind.GreaterOrEquals, TypeSymbol.Int, TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.PibeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.PibePibeToken, BoundBinaryOperatorKind.LogicalOr, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Bool),
        };

        public static BoundBinaryOperator Bind(SyntaxKind SyntaxType, TypeSymbol lefttype, TypeSymbol righttype)
        {
            foreach (var op in _operators)
            {
                if (op.Syntaxtype == SyntaxType && op.LeftType == lefttype && op.RightType == righttype)
                    return op;
            }
            return null;
        }
    }

}

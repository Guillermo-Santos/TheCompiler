using SparkCore.Analytics.Syntax;
using System;

namespace SparkCore.Analytics.Binding.Scope.Expressions
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(SyntaxKind syntaxtype, BoundBinaryOperatorKind kind, Type alTy) :
            this(syntaxtype, kind, alTy, alTy, alTy)
        {
        }
        private BoundBinaryOperator(SyntaxKind syntaxtype, BoundBinaryOperatorKind kind, Type operantType, Type resultType) :
            this(syntaxtype, kind, operantType, operantType, resultType)
        {
        }
        private BoundBinaryOperator(SyntaxKind syntaxtype, BoundBinaryOperatorKind kind, Type leftType, Type rightType, Type resultType)
        {
            Syntaxtype = syntaxtype;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            Type = resultType;
        }

        public SyntaxKind Syntaxtype { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public Type LeftType { get; }
        public Type RightType { get; }
        public Type Type { get; }

        private static BoundBinaryOperator[] _operators =
        {
            new BoundBinaryOperator(SyntaxKind.PlusToken,  BoundBinaryOperatorKind.Addition, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.MinusToken,  BoundBinaryOperatorKind.Substraction, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.PibeToken, BoundBinaryOperatorKind.BitwiseOr, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, typeof(int)),
            new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, typeof(int), typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, typeof(int), typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.LessToken, BoundBinaryOperatorKind.Less, typeof(int), typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.LessOrEqualsToken, BoundBinaryOperatorKind.LessOrEquals, typeof(int), typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.Greater, typeof(int), typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.GreaterOrEqualsToken, BoundBinaryOperatorKind.GreaterOrEquals, typeof(int), typeof(bool)),

            new BoundBinaryOperator(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.PibeToken, BoundBinaryOperatorKind.BitwiseOr, typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.PibePibeToken, BoundBinaryOperatorKind.LogicalOr, typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, typeof(bool)),
        };

        public static BoundBinaryOperator Bind(SyntaxKind SyntaxType, Type lefttype, Type righttype)
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

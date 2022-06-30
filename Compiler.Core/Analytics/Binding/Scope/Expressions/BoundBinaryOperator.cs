using SparkCore.Analytics.Syntax;
using System;

namespace SparkCore.Analytics.Binding.Scope.Expressions
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(SyntaxType syntaxtype, BoundBinaryOperatorType type, Type alTy) :
            this(syntaxtype, type, alTy, alTy, alTy)
        {
        }
        private BoundBinaryOperator(SyntaxType syntaxtype, BoundBinaryOperatorType type, Type operantType, Type resultType) :
            this(syntaxtype, type, operantType, operantType, resultType)
        {
        }
        private BoundBinaryOperator(SyntaxType syntaxtype, BoundBinaryOperatorType type, Type leftType, Type rightType, Type resultType)
        {
            Syntaxtype = syntaxtype;
            BoundType = type;
            LeftType = leftType;
            RightType = rightType;
            Type = resultType;
        }

        public SyntaxType Syntaxtype { get; }
        public BoundBinaryOperatorType BoundType { get; }
        public Type LeftType { get; }
        public Type RightType { get; }
        public Type Type { get; }

        private static BoundBinaryOperator[] _operators =
        {
            new BoundBinaryOperator(SyntaxType.PlusToken,  BoundBinaryOperatorType.Addition, typeof(int)),
            new BoundBinaryOperator(SyntaxType.MinusToken,  BoundBinaryOperatorType.Substraction, typeof(int)),
            new BoundBinaryOperator(SyntaxType.StarToken, BoundBinaryOperatorType.Multiplication, typeof(int)),
            new BoundBinaryOperator(SyntaxType.SlashToken, BoundBinaryOperatorType.Divicion, typeof(int)),

            new BoundBinaryOperator(SyntaxType.EqualsEqualsToken, BoundBinaryOperatorType.Equals, typeof(int), typeof(bool)),
            new BoundBinaryOperator(SyntaxType.BangEqualsToken, BoundBinaryOperatorType.NotEquals, typeof(int), typeof(bool)),

            new BoundBinaryOperator(SyntaxType.AmpersandAmpersandToken, BoundBinaryOperatorType.LogicalAnd, typeof(bool)),
            new BoundBinaryOperator(SyntaxType.PibePibeToken, BoundBinaryOperatorType.LogicalOr, typeof(bool)),
            new BoundBinaryOperator(SyntaxType.EqualsEqualsToken, BoundBinaryOperatorType.Equals, typeof(bool)),
            new BoundBinaryOperator(SyntaxType.BangEqualsToken, BoundBinaryOperatorType.NotEquals, typeof(bool)),
        };

        public static BoundBinaryOperator Bind(SyntaxType SyntaxType, Type lefttype, Type righttype)
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

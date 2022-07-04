using System;
using SparkCore.Analytics.Syntax;

namespace SparkCore.Analytics.Binding.Scope.Expressions
{

    internal sealed class BoundUnaryOperator
    {
        private BoundUnaryOperator(SyntaxKind syntaxtype, BoundUnaryOperatorKind kind, Type operandType) :
            this(syntaxtype, kind, operandType, operandType)
        {
        }
        private BoundUnaryOperator(SyntaxKind syntaxtype, BoundUnaryOperatorKind kind, Type operandType, Type resultType)
        {
            Syntaxtype = syntaxtype;
            Kind = kind;
            OperandType = operandType;
            Type = resultType;
        }

        public SyntaxKind Syntaxtype
        {
            get;
        }
        public BoundUnaryOperatorKind Kind
        {
            get;
        }
        public Type OperandType
        {
            get;
        }
        public Type Type
        {
            get;
        }

        private static BoundUnaryOperator[] _operators =
        {
            new BoundUnaryOperator(SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNegation, typeof(bool)),
            new BoundUnaryOperator(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, typeof(int)),
            new BoundUnaryOperator(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, typeof(int)),
            new BoundUnaryOperator(SyntaxKind.TildeToken, BoundUnaryOperatorKind.OnesComplement, typeof(int)),
        };

        public static BoundUnaryOperator Bind(SyntaxKind SyntaxType, Type operandtype)
        {
            foreach (var op in _operators)
            {
                if (op.Syntaxtype == SyntaxType && op.OperandType == operandtype)
                    return op;
            }
            return null;
        }
    }

}

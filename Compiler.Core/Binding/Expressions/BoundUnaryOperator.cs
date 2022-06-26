using Compiler.Core.Syntax;
using System;

namespace Compiler.Core.Binding.Expressions
{
    internal sealed class BoundUnaryOperator
    {
        private BoundUnaryOperator(SyntaxType syntaxtype, BoundUnaryOperatorType type, Type operandType):
            this(syntaxtype, type, operandType, operandType)
        {
        }
        private BoundUnaryOperator(SyntaxType syntaxtype, BoundUnaryOperatorType type, Type operandType, Type resultType)
        {
            Syntaxtype = syntaxtype;
            BoundType = type;
            OperandType = operandType;
            Type = resultType;
        }

        public SyntaxType Syntaxtype { get; }
        public BoundUnaryOperatorType BoundType { get; }
        public Type OperandType { get; }
        public Type Type { get; }

        private static BoundUnaryOperator[] _operators =
        {
            new BoundUnaryOperator(SyntaxType.BangToken, BoundUnaryOperatorType.LogicalNegation, typeof(bool)),
            new BoundUnaryOperator(SyntaxType.PlusToken, BoundUnaryOperatorType.Identity, typeof(int)),
            new BoundUnaryOperator(SyntaxType.MinusToken, BoundUnaryOperatorType.Negation, typeof(int)),
        };

        public static BoundUnaryOperator Bind(SyntaxType SyntaxType, Type operandtype)
        {
            foreach(var op in _operators)
            {
                if(op.Syntaxtype == SyntaxType && op.OperandType == operandtype)
                    return op;
            }
            return null;
        }
    }

}

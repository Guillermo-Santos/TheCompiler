using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.Analytics.Syntax.Tree.Expressions;
using SparkCore.Analytics.Syntax.Tree.Statements;

namespace SparkCore.Tests.Analytics.Syntax
{
    /// <summary>
    /// This class represent a series of test to ensure that we honor de syntax of our expressions.
    /// </summary>
    public class ParserTest
    {
        [Theory]
        [MemberData(nameof(GetBinaryOperatorPairsData))]
        public void Parser_BinaryExpression_HonorsProcedures(SyntaxType op1, SyntaxType op2)
        {
            var op1Precedence = op1.GetBinaryOperatorPrecedence();
            var op2Precedence = op2.GetBinaryOperatorPrecedence();
            var op1Text = SyntaxFacts.GetText(op1);
            var op2Text = SyntaxFacts.GetText(op2);
            var text = $"a {op1Text} b {op2Text} c";
            var expression = ParseExpression(text);

            //Si la precedencia del operador 1 es mayor a la del operador 2.
            if (op1Precedence >= op2Precedence)
            {
                //Aseguramos que el arbol sintactico tiene la siguiente estructura
                //      op2
                //     /   \
                //   op1    c
                //  /   \
                // a     b

                using (var e = new AssertingEnumerator(expression))
                {
                    e.AssertNode(SyntaxType.BinaryExpression);
                    e.AssertNode(SyntaxType.BinaryExpression);
                    e.AssertNode(SyntaxType.NameExpression);
                    e.AssertToken(SyntaxType.IdentifierToken, "a");
                    e.AssertToken(op1, op1Text);
                    e.AssertNode(SyntaxType.NameExpression);
                    e.AssertToken(SyntaxType.IdentifierToken, "b");
                    e.AssertToken(op2, op2Text);
                    e.AssertNode(SyntaxType.NameExpression);
                    e.AssertToken(SyntaxType.IdentifierToken, "c");
                }
            }
            else//Si no entonces
            {
                //Aseguramos que el arbol sintactico tiene la siguiente estructura
                //   op1
                //  /   \
                // a    op2
                //     /   \
                //    b     c

                using (var e = new AssertingEnumerator(expression))
                {
                    e.AssertNode(SyntaxType.BinaryExpression);
                    e.AssertNode(SyntaxType.NameExpression);
                    e.AssertToken(SyntaxType.IdentifierToken, "a");
                    e.AssertToken(op1, op1Text);
                    e.AssertNode(SyntaxType.BinaryExpression);
                    e.AssertNode(SyntaxType.NameExpression);
                    e.AssertToken(SyntaxType.IdentifierToken, "b");
                    e.AssertToken(op2, op2Text);
                    e.AssertNode(SyntaxType.NameExpression);
                    e.AssertToken(SyntaxType.IdentifierToken, "c");
                }
            }
        }


        [Theory]
        [MemberData(nameof(GetUnaryOperatorPairsData))]
        public void Parser_UnaryExpression_HonorsProcedures(SyntaxType unaryType, SyntaxType binaryType)
        {
            var unaryPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(unaryType);
            var binaryPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(binaryType);
            var unaryText = SyntaxFacts.GetText(unaryType);
            var binaryText = SyntaxFacts.GetText(binaryType);
            var text = $"{unaryText} a {binaryText} b";
            var expression = ParseExpression(text);

            if (unaryPrecedence >= binaryPrecedence)
            {
                //   binary
                //   /   \
                // unary   b
                //  |
                //  a
                using (var e = new AssertingEnumerator(expression))
                {
                    e.AssertNode(SyntaxType.BinaryExpression);
                    e.AssertNode(SyntaxType.UnaryExpression);
                    e.AssertToken(unaryType, unaryText);
                    e.AssertNode(SyntaxType.NameExpression);
                    e.AssertToken(SyntaxType.IdentifierToken, "a");
                    e.AssertToken(binaryType, binaryText);
                    e.AssertNode(SyntaxType.NameExpression);
                    e.AssertToken(SyntaxType.IdentifierToken, "b");
                }
            }
            else
            {
                //   unary
                //     |
                //   binary
                //   /    \
                //  a      b

                using (var e = new AssertingEnumerator(expression))
                {
                    e.AssertNode(SyntaxType.UnaryExpression);
                    e.AssertToken(unaryType, unaryText);
                    e.AssertNode(SyntaxType.BinaryExpression);
                    e.AssertNode(SyntaxType.NameExpression);
                    e.AssertToken(SyntaxType.IdentifierToken, "a");
                    e.AssertToken(binaryType, binaryText);
                    e.AssertNode(SyntaxType.NameExpression);
                    e.AssertToken(SyntaxType.IdentifierToken, "b");
                }
            }
        }
        
        public static IEnumerable<object[]> GetBinaryOperatorPairsData()
        {
            foreach (SyntaxType op1 in SyntaxFacts.GetBinaryOperatorTypes())
            {
                foreach (SyntaxType op2 in SyntaxFacts.GetBinaryOperatorTypes())
                {
                    yield return new object[] { op1, op2 };
                }
            }
        }
        public static IEnumerable<object[]> GetUnaryOperatorPairsData()
        {
            foreach (SyntaxType unary in SyntaxFacts.GetUnaryOperatorTypes())
            {
                foreach (SyntaxType binary in SyntaxFacts.GetBinaryOperatorTypes())
                {
                    yield return new object[] { unary, binary };
                }
            }
        }
        private static SyntaxExpression ParseExpression(string text)
        {
            SyntaxTree syntaxTree = SyntaxTree.Parse(text);
            var root = syntaxTree.Root;
            var statement = root.Statement;
            return Assert.IsType<ExpressionSyntaxStatement>(statement).Expression;
        }
    }
}
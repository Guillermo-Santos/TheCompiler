using SparkCore.Analytics.Syntax;

namespace SparkCore.Tests.Analytics
{
    public class EvaluationTests
    {
        [Theory]
        [InlineData("1",1)]
        [InlineData("+1",1)]
        [InlineData("-1",-1)]
        [InlineData("14 + 12",26)]
        [InlineData("1 + 2",3)]
        [InlineData("1 - 2",-1)]
        [InlineData("4 * 2",8)]
        [InlineData("9 / 3",3)]
        [InlineData("(10)",10)]
        [InlineData("12 == 3",false)]
        [InlineData("3 == 3",true)]
        [InlineData("12 != 3",true)]
        [InlineData("3 != 3",false)]
        [InlineData("false == false",true)]
        [InlineData("true == false",false)]
        [InlineData("false != false", false)]
        [InlineData("true != false",true)]
        [InlineData("true",true)]
        [InlineData("false",false)]
        [InlineData("!true",false)]
        [InlineData("!false",true)]
        [InlineData("(a = 10) * a",100)]
        public void SyntaxFact_GetText_RoundTrips(string text, object expectedResult)
        {
            var expression = SyntaxTree.Parse(text);
            var compilation = new Compilation(expression);
            var variables = new Dictionary<VariableSymbol, object>();
            var result = compilation.Evaluate(variables);

            //Aseguramos que no hubo errores en la compilacion.
            Assert.Empty(result.Diagnostics);
            //Aseguramos que el resultado esperado sea el mismo que el resultado obtenido.
            Assert.Equal(expectedResult, result.Value);
        }
    }
}

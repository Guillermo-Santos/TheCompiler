using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax.Tree;

namespace SparkCore.Tests.Analytics;

public class EvaluationTests
{
    [Theory]
    #region In line data
    [InlineData("1", 1)]
    [InlineData("+1", 1)]
    [InlineData("-1", -1)]
    [InlineData("~1", -2)]
    [InlineData("14 + 12", 26)]
    [InlineData("1 + 2", 3)]
    [InlineData("1 - 2", -1)]
    [InlineData("4 * 2", 8)]
    [InlineData("9 / 3", 3)]
    [InlineData("(10)", 10)]
    [InlineData("12 == 3", false)]
    [InlineData("3 == 3", true)]
    [InlineData("12 != 3", true)]
    [InlineData("3 != 3", false)]
    [InlineData("3 < 4", true)]
    [InlineData("5 < 3", false)]
    [InlineData("3 <= 3", true)]
    [InlineData("3 <= 4", true)]
    [InlineData("4 <= 3", false)]
    [InlineData("3 > 4", false)]
    [InlineData("5 > 3", true)]
    [InlineData("3 >= 3", true)]
    [InlineData("3 >= 4", false)]
    [InlineData("4 >= 3", true)]
    [InlineData("1 | 2", 3)]
    [InlineData("1 | 0", 1)]
    [InlineData("1 & 3", 1)]
    [InlineData("1 & 0", 0)]
    [InlineData("1 ^ 0", 1)]
    [InlineData("0 ^ 1", 1)]
    [InlineData("1 ^ 3", 2)]
    [InlineData("false == false", true)]
    [InlineData("true == false", false)]
    [InlineData("false != false", false)]
    [InlineData("true != false", true)]
    [InlineData("true && true", true)]
    [InlineData("false || false", false)]
    [InlineData("false | false", false)]
    [InlineData("false | true", true)]
    [InlineData("true | false", true)]
    [InlineData("true | true", true)]
    [InlineData("false & false", false)]
    [InlineData("false & true", false)]
    [InlineData("true & false", false)]
    [InlineData("true & true", true)]
    [InlineData("false ^ false", false)]
    [InlineData("true ^ false", true)]
    [InlineData("false ^ true", true)]
    [InlineData("true ^ true", false)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("!true", false)]
    [InlineData("!false", true)]
    [InlineData("var a = 10", 10)]
    [InlineData("\"test\"", "test")]
    [InlineData("\"te\"\"st\"", "te\"st")]
    [InlineData("\"test\" == \"test\"", true)]
    [InlineData("\"test\" != \"test\"", false)]
    [InlineData("\"test\" == \"asd\"", false)]
    [InlineData("\"test\" != \"asd\"", true)]
    [InlineData("{var a = 10 (a * a)}", 100)]
    [InlineData("{ var a = 0 (a = 10) * a}", 100)]
    [InlineData("{ var a = 0 if a == 0 a = 10 a }", 10)]
    [InlineData("{ var a = 0 if a == 4 a = 10 a }", 0)]
    [InlineData("{ var a = 0 if a == 0 a = 10 else a = 5 a}", 10)]
    [InlineData("{ var a = 0 if a == 4 a = 10 else a = 5 a }", 5)]
    [InlineData("{ var i = 10 var result = 0 while i > 0 { result = result + 1 i = i - 1} result}", 10)]
    [InlineData("{ var result = 0 for i = 1 to 100 { result = result + i} result}", 5050)]
    [InlineData("{ var result = 0 var e = 0 for i = 1 to 10 {result = result + 1 if i==5 e = 10} result = result * e}", 100)]
    [InlineData("{ var a = 10 for i = 1 to (a = a - 1) {} a}", 9)]
    [InlineData("{ var a = 0 do{ a = a + 1 }while a < 10 a}", 10)]
    #endregion
    public void SyntaxFact_GetText_RoundTrips(string text, object expectedResult)
    {
        AssertValue(text, expectedResult);
    }
    [Fact]
    public void Evaluate_VariableDeclaration_Reports_Redeclaration()
    {
        var text = @"
                {
                    var x = 10
                    var y = 100
                    {
                        var x = 10
                    }
                    var [x] = 5
                }
            ";

        var diagnostics = @"
                'x' is already declared.
            ";

        AssertDiagnostics(text, diagnostics);

    }
    [Fact]
    public void Evaluate_BloackStatement_NoInfiniteLoop()
    {
        var text = @"
                {
                [)][]
            ";

        var diagnostics = @"
                Unexpected token <CloseParentesisToken>, expected <IdentifierToken>.
                Unexpected token <EndOfFileToken>, expected <CloseBraceToken>.
            ";

        AssertDiagnostics(text, diagnostics);

    }

    [Fact]
    public void Evaluator_InvokeFunctionArguments_NoInfiniteLoop()
    {
        var text = @"
                print(""Hi""[[=]][)]
            ";

        var diagnostics = @"
                Unexpected token <EqualsToken>, expected <CloseParentesisToken>.
                Unexpected token <EqualsToken>, expected <IdentifierToken>.
                Unexpected token <CloseParentesisToken>, expected <IdentifierToken>.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_FunctionParameters_NoInfiniteLoop()
    {
        var text = @"
                function hi(name: string[[[=]]][)]
                {
                    print(""Hi "" + name + ""!"" )
                }[]
            ";

        var diagnostics = @"
                Unexpected token <EqualsToken>, expected <CloseParentesisToken>.
                Unexpected token <EqualsToken>, expected <OpenBraceToken>.
                Unexpected token <EqualsToken>, expected <IdentifierToken>.
                Unexpected token <CloseParentesisToken>, expected <IdentifierToken>.
                Unexpected token <EndOfFileToken>, expected <CloseBraceToken>.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluate_IfStatement_Reports_CannotConvert()
    {
        var text = @"
                {
                    var x = 0
                    if [10]                    
                        x = 10
                }
            ";

        var diagnostics = @"
                Cannot convert type 'int' to 'bool'.
            ";

        AssertDiagnostics(text, diagnostics);

    }
    [Fact]
    public void Evaluate_WhileStatement_Reports_CannotConvert()
    {
        var text = @"
                {
                    var x = 0
                    while [10]                    
                        x = 10
                }
            ";

        var diagnostics = @"
                Cannot convert type 'int' to 'bool'.
            ";
        AssertDiagnostics(text, diagnostics);

    }
    [Fact]
    public void Evaluate_ForStatement_Reports_CannotConvert_LowerBound()
    {
        var text = @"
                {
                    var result = 0
                    for i = [true] to 10                    
                        result = result + i
                }
            ";

        var diagnostics = @"
                Cannot convert type 'bool' to 'int'.
            ";

        AssertDiagnostics(text, diagnostics);

    }
    [Fact]
    public void Evaluate_DoWhileStatement_Reports_CannotConvert_LowerBound()
    {
        var text = @"
                {
                    var x = 0
                    do
                        x = 10
                    while [10]
                }
            ";

        var diagnostics = @"
                Cannot convert type 'int' to 'bool'.
            ";

        AssertDiagnostics(text, diagnostics);

    }
    [Fact]
    public void Evaluate_ForStatement_Reports_CannotConvert_UpperBound()
    {
        var text = @"
                {
                    var result = 0
                    for i = 1 to [true]                    
                        result = result + i
                }
            ";

        var diagnostics = @"
                Cannot convert type 'bool' to 'int'.
            ";

        AssertDiagnostics(text, diagnostics);

    }
    [Fact]
    public void Evaluate_NameExpression_Reports_Undefined()
    {
        var text = @"[x] * 10";

        var diagnostics = @"
                Variable 'x' doesn't exist.
            ";

        AssertDiagnostics(text, diagnostics);

    }
    [Fact]
    public void Evaluate_NameExpression_Reports_NoErrorForInsertedToken()
    {
        var text = @"1 + []";

        var diagnostics = @"
                Unexpected token <EndOfFileToken>, expected <IdentifierToken>.
            ";

        AssertDiagnostics(text, diagnostics);

    }
    [Fact]
    public void Evaluate_UnaryExpression_Reports_Undefined()
    {
        var text = @"[+]true";

        var diagnostics = @"
                Unary operator '+' is not defined for type 'bool'.
            ";

        AssertDiagnostics(text, diagnostics);

    }
    [Fact]
    public void Evaluate_BinaryExpression_Reports_Undefined()
    {
        var text = @"10 [+] true";

        var diagnostics = @"
                Binary operator '+' is not definded for types 'int' and 'bool'.
            ";

        AssertDiagnostics(text, diagnostics);

    }
    [Fact]
    public void Evaluate_AssigmentExpression_Reports_Undefined()
    {
        var text = @"[x] = 10";

        var diagnostics = @"
                Variable 'x' doesn't exist.
            ";

        AssertDiagnostics(text, diagnostics);

    }
    [Fact]
    public void Evaluate_AssigmentExpression_Reports_CannotAssign()
    {
        var text = @"
                {
                    let x = 10
                    x [=] 0
                }
            ";

        var diagnostics = @"
                Variable 'x' is read only and cannot be assigned to.
            ";

        AssertDiagnostics(text, diagnostics);

    }
    [Fact]
    public void Evaluate_AssigmentExpression_Reports_CannotConvert()
    {
        var text = @"
                {
                    var x = 10
                    x = [true]
                }
            ";

        var diagnostics = @"
                Cannot convert type 'bool' to 'int'.
            ";

        AssertDiagnostics(text, diagnostics);

    }
    [Fact]
    public void Evaluator_Variables_Can_Shadow_Functions()
    {
        var text = @"
                {
                    let print = 42
                    [print](""test"")
                }
            ";

        var diagnostics = @"
                Function 'print' doesn't exist.
            ";

        AssertDiagnostics(text, diagnostics);
    }
    private static void AssertValue(string text, object expectedResult)
    {
        var syntaxTree = SyntaxTree.Parse(text);
        var compilation = new Compilation(syntaxTree);
        var variables = new Dictionary<VariableSymbol, object>();
        var result = compilation.Evaluate(variables);

        //Aseguramos que no hubo errores en la compilacion.
        Assert.Empty(result.Diagnostics);
        //Aseguramos que el resultado esperado sea el mismo que el resultado obtenido.
        Assert.Equal(expectedResult, result.Value);
    }
    private void AssertDiagnostics(string text, string diagnosticsText)
    {
        var annotatedText = AnnotatedText.Parse(text);
        var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
        var compilation = new Compilation(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

        var expectedDiagnostics = AnnotatedText.UnindentLines(diagnosticsText);

        if (annotatedText.Spans.Length != expectedDiagnostics.Length)
            throw new Exception("ERROR: Must mark as many spans as there are diagnostics.");

        var diagnostics = result.Diagnostics;
        Assert.Equal(expectedDiagnostics.Length, diagnostics.Length);

        for (var i = 0; i < expectedDiagnostics.Length; i++)
        {
            var expectedMessage = expectedDiagnostics[i];
            var actualMessage = diagnostics[i].Message;
            Assert.Equal(expectedMessage, actualMessage);

            var expectedSpan = annotatedText.Spans[i];
            var actualSpan = diagnostics[i].Span;
            Assert.Equal(expectedSpan, actualSpan);
        }

    }

}

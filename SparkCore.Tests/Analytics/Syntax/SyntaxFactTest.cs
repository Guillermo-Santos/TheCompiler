using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Syntax.Tree;

namespace SparkCore.Tests.Analytics.Syntax
{
    public class SyntaxFactTest
    {
        [Theory]
        [MemberData(nameof(GetSyntaxTypeData))]
        public void SyntaxFact_GetText_RoundTrips(SyntaxKind type)
        {
            var text = SyntaxFacts.GetText(type);
            if (text == null)
                return;
            var tokens = SyntaxTree.ParseTokens(text);
            var token = Assert.Single(tokens);

            Assert.Equal(type, token.Kind);
            Assert.Equal(text, token.Text);
        }
        public static IEnumerable<object[]> GetSyntaxTypeData()
        {
            var types = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (var type in types)
                yield return new object[] { type };
        }
    }
}
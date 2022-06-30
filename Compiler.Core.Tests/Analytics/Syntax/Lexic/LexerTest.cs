using SparkCore.Analytics.Syntax;
using SparkCore.Analytics.Syntax.Tree;

namespace SparkCore.Tests.Analytics.Syntax.Lexic
{
    /// <summary>
    /// Class to do test over the tokens and their combinations.
    /// </summary>
    public class LexerTest
    {
        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Lexer_Tests_AllTokens()
        {
            //Obtener todos los tokens tipo palabra reservada o token (diferenciacion de expressiones).
            var tokentypes = Enum.GetValues(typeof(SyntaxType))
                                  .Cast<SyntaxType>()
                                  .Where(t => t.ToString().EndsWith("Keyword") ||
                                              t.ToString().EndsWith("Token"))
                                  .ToList();
            //Probamos los tokens.
            var testedTokenTypes = GetTokens().Concat(GetSeparators()).Select(t => t.type);
            //Ordenamos la lista de tokens.
            var untestedTokenTypes = new SortedSet<SyntaxType>(tokentypes);
            //Removemos los tipos de tokens no testeables.
            untestedTokenTypes.Remove(SyntaxType.BadToken);
            untestedTokenTypes.Remove(SyntaxType.EndOfFileToken);
            //Eliminamos los tokens probados de la lista de tokens.
            untestedTokenTypes.ExceptWith(testedTokenTypes);
            //Asegurarse de que todos los tokens fueron probados correctamente.
            Assert.Empty(untestedTokenTypes);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="text"></param>
        [Theory]
        [MemberData(nameof(GetTokensData))]
        public void Lexer_Lexes_Token(SyntaxType type, string text)
        {
            var tokens = SyntaxTree.ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(type, token.Type);
            Assert.Equal(text, token.Text);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t1Type"></param>
        /// <param name="t1Text"></param>
        /// <param name="t2Type"></param>
        /// <param name="t2Text"></param>
        [Theory]
        [MemberData(nameof(GetTokenPairsData))]
        public void Lexer_Lexes_TokenPairs(SyntaxType t1Type, string t1Text, SyntaxType t2Type, string t2Text)
        {
            var text = t1Text + t2Text;
            var tokens = SyntaxTree.ParseTokens(text).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.Equal(t1Type, tokens[0].Type);
            Assert.Equal(t1Text, tokens[0].Text);
            Assert.Equal(t2Type, tokens[1].Type);
            Assert.Equal(t2Text, tokens[1].Text);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t1Type"></param>
        /// <param name="t1Text"></param>
        /// <param name="separatorType"></param>
        /// <param name="separatorText"></param>
        /// <param name="t2Type"></param>
        /// <param name="t2Text"></param>
        [Theory]
        [MemberData(nameof(GetTokenPairsWithSeparatorData))]
        public void Lexer_Lexes_TokenPairsWithSeparator(SyntaxType t1Type, string t1Text, SyntaxType separatorType, string separatorText, SyntaxType t2Type, string t2Text)
        {
            var text = t1Text + separatorText + t2Text;
            var tokens = SyntaxTree.ParseTokens(text).ToArray();

            Assert.Equal(3, tokens.Length);
            Assert.Equal(t1Type, tokens[0].Type);
            Assert.Equal(t1Text, tokens[0].Text);
            Assert.Equal(separatorType, tokens[1].Type);
            Assert.Equal(separatorText, tokens[1].Text);
            Assert.Equal(t2Type, tokens[2].Type);
            Assert.Equal(t2Text, tokens[2].Text);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> GetTokensData()
        {
            foreach (var t in GetTokens().Concat(GetSeparators()))
                yield return new object[] { t.type, t.text };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> GetTokenPairsData()
        {
            foreach (var t in GetTokenPairs())
                yield return new object[] { t.t1Type, t.t1Text, t.t2Type, t.t2Text };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> GetTokenPairsWithSeparatorData()
        {
            foreach (var t in GetTokenPairsWithSeparator())
                yield return new object[] { t.t1Type, t.t1Text, t.separatorType, t.separatorText, t.t2Type, t.t2Text };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<(SyntaxType type, string text)> GetTokens()
        {
            //Lista de tokens inmutables, representa los tokens que tienen un valor fijo (operadores y palabras reservadas)
            //Se obtiene del metodo gettext de la clase syntaxfacts la cual define a el texto de estos tokens
            //y filtrandolos de aquellos cuyo texto no este definido(expresiones, variables, etc).
            var fixedTokens = Enum.GetValues(typeof(SyntaxType))
                                  .Cast<SyntaxType>()
                                  .Select(t => (type: t, text: SyntaxFacts.GetText(t)))
                                  .Where(t => t.text != null);
            //Lista de tokens con valores dinamicos utilizados para este test.
            var dynamicTokens = new[]
            {
                (SyntaxType.NumberToken,"1"),
                (SyntaxType.NumberToken,"123"),
                (SyntaxType.IdentifierToken,"a"),
                (SyntaxType.IdentifierToken, "abc"),
            };
            return fixedTokens.Concat(dynamicTokens);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<(SyntaxType type, string text)> GetSeparators()
        {
            return new[]
            {
                (SyntaxType.WhiteSpaceToken," "),
                (SyntaxType.WhiteSpaceToken,"  "),
                (SyntaxType.WhiteSpaceToken,"\r"),
                (SyntaxType.WhiteSpaceToken,"\n"),
                (SyntaxType.WhiteSpaceToken,"\r\n"),
            };
        }
        /// <summary>
        /// Procedure to confirm if a pair of tokens require a separator between them.
        /// </summary>
        /// <param name="t1Type">First token</param>
        /// <param name="t2Type">Second token</param>
        /// <returns>
        ///     Returns true if it require a separator and false if it do not.
        /// </returns>
        private static bool RequiresSeparator(SyntaxType t1Type, SyntaxType t2Type)
        {
            var t1IsKeyword = t1Type.ToString().EndsWith("Keyword");
            var t2IsKeyword = t2Type.ToString().EndsWith("Keyword");
            if (t1Type == SyntaxType.IdentifierToken && t2Type == SyntaxType.IdentifierToken)
                return true;
            if (t1IsKeyword && t2IsKeyword)
                return true;
            if (t1IsKeyword && t2Type == SyntaxType.IdentifierToken)
                return true;
            if (t1Type == SyntaxType.IdentifierToken && t2IsKeyword)
                return true;
            if (t1Type == SyntaxType.NumberToken && t2Type == SyntaxType.NumberToken)
                return true;
            if (t1Type == SyntaxType.BangToken && t2Type == SyntaxType.EqualsToken)
                return true;
            if (t1Type == SyntaxType.BangToken && t2Type == SyntaxType.EqualsEqualsToken)
                return true;
            if (t1Type == SyntaxType.EqualsToken && t2Type == SyntaxType.EqualsToken)
                return true;
            if (t1Type == SyntaxType.EqualsToken && t2Type == SyntaxType.EqualsEqualsToken)
                return true;
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<(SyntaxType t1Type, string t1Text, SyntaxType t2Type, string t2Text)> GetTokenPairs()
        {
            foreach (var t1 in GetTokens())
            {
                foreach (var t2 in GetTokens())
                {
                    if (!RequiresSeparator(t1.type, t2.type))
                        yield return (t1.type, t1.text, t2.type, t2.text);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<(SyntaxType t1Type, string t1Text, SyntaxType separatorType, string separatorText, SyntaxType t2Type, string t2Text)> GetTokenPairsWithSeparator()
        {
            foreach (var t1 in GetTokens())
            {
                foreach (var t2 in GetTokens())
                {
                    if (RequiresSeparator(t1.type, t2.type))
                    {
                        foreach (var s in GetSeparators())
                            yield return (t1.type, t1.text, s.type, s.text, t2.type, t2.text);
                    }
                }
            }
        }

    }
}
using Compiler.Core.Syntax;
using System.Collections.Generic;

namespace Compiler.Core.Analyzers
{


    internal sealed class LexicAnalyzer
    {
        private readonly string _text;
        private int _position;
        private List<string> _diagnostics = new List<string>();
        public LexicAnalyzer(string text)
        {
            _text = text;
        }
        public IEnumerable<string> Diagnostics => _diagnostics;
        private char Current => Peek(0);
        private char LookaHead => Peek(1);
        private char Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _text.Length)
                return '\0';
            return _text[index];
        }

        private void Next()
        {
            _position++;
        }
        public SyntaxToken NextToken()
        {
            if (_position >= _text.Length)
            {
                return new SyntaxToken(SyntaxType.EndOfFileToken, _position, "\0", null);
            }
            if (char.IsDigit(Current))
            {
                var start = _position;
                while (char.IsDigit(Current))
                    Next();
                var length = _position - start;
                var text = _text.Substring(start, length);
                if (!int.TryParse(text, out var value))
                {
                    _diagnostics.Add($"The number {_text} cannot be represented by an Int32.");
                }
                return new SyntaxToken(SyntaxType.NumberToken, start, text, value);
            }

            if (char.IsWhiteSpace(Current))
            {
                var start = _position;
                while (char.IsWhiteSpace(Current))
                    Next();
                var length = _position - start;
                var text = _text.Substring(start, length);
                return new SyntaxToken(SyntaxType.WhiteSpaceToken, start, text, null);
            }

            if (char.IsLetter(Current))
            {
                var start = _position;
                while (char.IsLetter(Current))
                    Next();
                var length = _position - start;
                var text = _text.Substring(start, length);
                var type = SyntaxFacts.GetKeywordType(text);
                return new SyntaxToken(type, start, text, null);
            }
            switch (Current)
            {
                case '+':
                    return new SyntaxToken(SyntaxType.PlusToken, _position, "+", null);
                case '-':
                    return new SyntaxToken(SyntaxType.MinusToken, _position, "-", null);
                case '*':
                    return new SyntaxToken(SyntaxType.StarToken, _position, "*", null);
                case '/':
                    return new SyntaxToken(SyntaxType.SlashToken, _position, "/", null);
                case '(':
                    return new SyntaxToken(SyntaxType.OpenParentesisToken, _position, "(", null);
                case ')':
                    return new SyntaxToken(SyntaxType.CloseParentesisToken, _position, ")", null);
                case '!':
                    if (LookaHead == '=')
                        return new SyntaxToken(SyntaxType.BangEqualsToken, _position += 2, "!=", null);
                    else
                        return new SyntaxToken(SyntaxType.BangToken, _position, "!", null);
                case '&':
                    if(LookaHead == '&') 
                        return new SyntaxToken(SyntaxType.AmpersandAmpersandToken, _position+=2, "&&", null);
                    break;
                case '|':
                    if (LookaHead == '|')
                        return new SyntaxToken(SyntaxType.PibePibeToken, _position += 2, "||", null);
                    break;
                case '=':
                    if (LookaHead == '=')
                        return new SyntaxToken(SyntaxType.EqualsEqualsToken, _position += 2, "==", null);
                    break;
            }

            _diagnostics.Add($"ERROR: bad character input: '{Current}'");
            return new SyntaxToken(SyntaxType.BadToken, _position++, Current.ToString(), null);
        }
    }
}

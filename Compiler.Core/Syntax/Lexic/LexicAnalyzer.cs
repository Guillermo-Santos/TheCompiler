using Compiler.Core.Diagnostics;
using System.Collections.Generic;

namespace Compiler.Core.Syntax.Lexic
{


    internal sealed class LexicAnalyzer
    {
        private readonly string _text;
        private int _position;
        private DiagnosticBag _diagnostics = new DiagnosticBag();
        public LexicAnalyzer(string text)
        {
            _text = text;
        }
        public DiagnosticBag Diagnostics => _diagnostics;
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

            var start = _position;

            if (char.IsDigit(Current))
            {
                while (char.IsDigit(Current))
                    Next();
                var length = _position - start;
                var text = _text.Substring(start, length);
                if (!int.TryParse(text, out var value))
                {
                    _diagnostics.ReportInvalidNumber(new TextSpan(start, length), _text, typeof(int));
                }
                return new SyntaxToken(SyntaxType.NumberToken, start, text, value);
            }

            if (char.IsWhiteSpace(Current))
            {
                while (char.IsWhiteSpace(Current))
                    Next();
                var length = _position - start;
                var text = _text.Substring(start, length);
                return new SyntaxToken(SyntaxType.WhiteSpaceToken, start, text, null);
            }

            if (char.IsLetter(Current))
            {
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
                    return new SyntaxToken(SyntaxType.PlusToken, _position++, "+", null);
                case '-':
                    return new SyntaxToken(SyntaxType.MinusToken, _position++, "-", null);
                case '*':
                    return new SyntaxToken(SyntaxType.StarToken, _position++, "*", null);
                case '/':
                    return new SyntaxToken(SyntaxType.SlashToken, _position++, "/", null);
                case '(':
                    return new SyntaxToken(SyntaxType.OpenParentesisToken, _position++, "(", null);
                case ')':
                    return new SyntaxToken(SyntaxType.CloseParentesisToken, _position++, ")", null);
                case '!':
                    if (LookaHead == '=')
                    {
                        _position += 2;
                        return new SyntaxToken(SyntaxType.BangEqualsToken, start, "!=", null);
                    }
                    else
                    {
                        _position++;
                        return new SyntaxToken(SyntaxType.BangToken, start, "!", null);
                    }
                case '&':
                    if (LookaHead == '&')
                    {
                        _position += 2;
                        return new SyntaxToken(SyntaxType.AmpersandAmpersandToken, start, "&&", null);
                    }
                    break;
                case '|':
                    if (LookaHead == '|')
                    {
                        _position += 2;
                        return new SyntaxToken(SyntaxType.PibePibeToken, start, "||", null);
                    }
                    break;
                case '=':
                    if (LookaHead == '=')
                    {
                        _position += 2;
                        return new SyntaxToken(SyntaxType.EqualsEqualsToken, start, "==", null);
                    }
                    else
                    {
                        _position++;
                        return new SyntaxToken(SyntaxType.EqualsToken, start, "=", null);
                    }
                    break;
            }

            _diagnostics.ReportBadCharacter(_position, Current);
            return new SyntaxToken(SyntaxType.BadToken, _position++, Current.ToString(), null);
        }
    }
}

using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.Analytics.Text;

namespace SparkCore.Analytics.Syntax.Lexic
{


    internal sealed class LexicAnalyzer
    {
        private readonly SourceText _text;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();

        private int _position;

        private int _start;
        private SyntaxType _type;
        private object _value;

        public LexicAnalyzer(SourceText text)
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

        public SyntaxToken Lex()
        {
            _start = _position;
            _type = SyntaxType.BadToken;
            _value = null;


            switch (Current)
            {
                case '\0':
                    _type = SyntaxType.EndOfFileToken;
                    break;
                case '+':
                    _type = SyntaxType.PlusToken;
                    _position++;
                    break;
                case '-':
                    _type = SyntaxType.MinusToken;
                    _position++;
                    break;
                case '*':
                    _type = SyntaxType.StarToken;
                    _position++;
                    break;
                case '/':
                    _type = SyntaxType.SlashToken;
                    _position++;
                    break;
                case '(':
                    _type = SyntaxType.OpenParentesisToken;
                    _position++;
                    break;
                case ')':
                    _type = SyntaxType.CloseParentesisToken;
                    _position++;
                    break;
                case '{':
                    _type = SyntaxType.OpenBraceToken;
                    _position++;
                    break;
                case '}':
                    _type = SyntaxType.CloseBraceToken;
                    _position++;
                    break;
                case '!':
                    _position++;
                    if (Current != '=')
                    {
                        _type = SyntaxType.BangToken;
                    }
                    else
                    {
                        _position++;
                        _type = SyntaxType.BangEqualsToken;
                    }
                    break;
                case '&':
                    if (LookaHead == '&')
                    {
                        _position += 2;
                        _type = SyntaxType.AmpersandAmpersandToken;
                    }
                    break;
                case '|':
                    if (LookaHead == '|')
                    {
                        _position += 2;
                        _type = SyntaxType.PibePibeToken;
                    }
                    break;
                case '=':
                    _position++;
                    if (Current != '=')
                    {
                        _type = SyntaxType.EqualsToken;
                    }
                    else
                    {
                        _position++;
                        _type = SyntaxType.EqualsEqualsToken;
                    }
                    break;
                case '0': case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
                    ReadNumberToken();
                    break;
                case ' ': case '\t':  case '\n': case '\r':
                    ReadWhiteSpaceToken();
                    break;
                default:
                    if (char.IsLetter(Current))
                    {
                        ReadIdentifierOrKeywordToken();
                    }
                    else if (char.IsWhiteSpace(Current))
                    {
                        ReadWhiteSpaceToken();
                    }
                    else
                    {
                        _diagnostics.ReportBadCharacter(_position, Current);
                        _position++;
                    }
                    break;
            }

            var length = _position - _start;
            var text = SyntaxFacts.GetText(_type);
            if (text == null)
                text = _text.ToString(_start, length);
            return new SyntaxToken(_type, _start, text, _value);
        }
        private void ReadWhiteSpaceToken()
        {
            while (char.IsWhiteSpace(Current))
                _position++;
            _type = SyntaxType.WhiteSpaceToken;
        }

        private void ReadNumberToken()
        {
            while (char.IsDigit(Current))
                _position++;

            var length = _position - _start;
            var text = _text.ToString(_start, length);
            if (!int.TryParse(text, out var value))
            {
                _diagnostics.ReportInvalidNumber(new TextSpan(_start, length), text, typeof(int));
            }
            _value = value;
            _type = SyntaxType.NumberToken;
        }
        private void ReadIdentifierOrKeywordToken()
        {
            while (char.IsLetter(Current))
                _position++;
            var length = _position - _start;
            var text = _text.ToString(_start, length);
            _type = SyntaxFacts.GetKeywordType(text);
        }
    }
}

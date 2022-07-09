using System.Text;
using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.Analytics.Text;

namespace SparkCore.Analytics.Syntax.Lexic;

//TODO: Agregar ; como token y colocarlo como terminador para las expresiones.
internal sealed class LexicAnalyzer
{
    private readonly SourceText _text;
    private readonly DiagnosticBag _diagnostics = new();

    private int _position;

    private int _start;
    private SyntaxKind _kind;
    private object _value;

    public LexicAnalyzer(SourceText text)
    {
        _text = text;
    }
    public DiagnosticBag Diagnostics => _diagnostics;
    private char Current => Peek(0);
    private char Lookahead => Peek(1);
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
        _kind = SyntaxKind.BadToken;
        _value = null;


        switch (Current)
        {
            case '\0':
                _kind = SyntaxKind.EndOfFileToken;
                break;
            case '+':
                _kind = SyntaxKind.PlusToken;
                _position++;
                break;
            case '-':
                _kind = SyntaxKind.MinusToken;
                _position++;
                break;
            case '*':
                _kind = SyntaxKind.StarToken;
                _position++;
                break;
            case '/':
                _kind = SyntaxKind.SlashToken;
                _position++;
                break;
            case '(':
                _kind = SyntaxKind.OpenParentesisToken;
                _position++;
                break;
            case ')':
                _kind = SyntaxKind.CloseParentesisToken;
                _position++;
                break;
            case '{':
                _kind = SyntaxKind.OpenBraceToken;
                _position++;
                break;
            case '}':
                _kind = SyntaxKind.CloseBraceToken;
                _position++;
                break;
            case ':':
                _kind = SyntaxKind.ColonToken;
                _position++;
                break;
            case ',':
                _kind = SyntaxKind.CommaToken;
                _position++;
                break;
            case '~':
                _kind = SyntaxKind.TildeToken;
                _position++;
                break;
            case '^':
                _kind = SyntaxKind.HatToken;
                _position++;
                break;
            case '!':
                _position++;
                if (Current != '=')
                {
                    _kind = SyntaxKind.BangToken;
                }
                else
                {
                    _position++;
                    _kind = SyntaxKind.BangEqualsToken;
                }
                break;
            case '&':
                _position++;
                if (Current != '&')
                {
                    _kind = SyntaxKind.AmpersandToken;
                }
                else
                {
                    _position++;
                    _kind = SyntaxKind.AmpersandAmpersandToken;
                }
                break;
            case '|':
                _position++;
                if (Current != '|')
                {
                    _kind = SyntaxKind.PibeToken;
                }
                else
                {
                    _position++;
                    _kind = SyntaxKind.PibePibeToken;
                }
                break;
            case '=':
                _position++;
                if (Current != '=')
                {
                    _kind = SyntaxKind.EqualsToken;
                }
                else
                {
                    _position++;
                    _kind = SyntaxKind.EqualsEqualsToken;
                }
                break;
            case '<':
                _position++;
                if (Current != '=')
                {
                    _kind = SyntaxKind.LessToken;
                }
                else
                {
                    _position++;
                    _kind = SyntaxKind.LessOrEqualsToken;
                }
                break;
            case '>':
                _position++;
                if (Current != '=')
                {
                    _kind = SyntaxKind.GreaterToken;
                }
                else
                {
                    _position++;
                    _kind = SyntaxKind.GreaterOrEqualsToken;
                }
                break;
            case '"':
                ReadString();
                break;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                ReadNumberToken();
                break;
            case ' ':
            case '\t':
            case '\n':
            case '\r':
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
        var text = SyntaxFacts.GetText(_kind);
        if (text == null)
            text = _text.ToString(_start, length);
        return new SyntaxToken(_kind, _start, text, _value);
    }

    private void ReadString()
    {
        // Saltar la posicion actual.
        _position++;
        var sb = new StringBuilder();
        var done = false;
        while (!done)
        {
            switch (Current)
            {
                case '\0':
                case '\r':
                case '\n':
                    var span = new TextSpan(_start, 1);
                    _diagnostics.ReportUnterminedString(span);
                    done = true;
                    break;
                case '"':
                    if (Lookahead == '"')
                    {
                        sb.Append(Current);
                        _position += 2;
                    }
                    else
                    {
                        _position++;
                        done = true;
                    }
                    break;
                default:
                    sb.Append(Current);
                    _position++;
                    break;
            }
        }

        _kind = SyntaxKind.StringToken;
        _value = sb.ToString();
    }
    private void ReadWhiteSpaceToken()
    {
        while (char.IsWhiteSpace(Current))
            _position++;
        _kind = SyntaxKind.WhiteSpaceToken;
    }

    private void ReadNumberToken()
    {
        while (char.IsDigit(Current))
            _position++;

        var length = _position - _start;
        var text = _text.ToString(_start, length);
        if (!int.TryParse(text, out var value))
        {
            _diagnostics.ReportInvalidNumber(new TextSpan(_start, length), text, TypeSymbol.Int);
        }
        _value = value;
        _kind = SyntaxKind.NumberToken;
    }
    private void ReadIdentifierOrKeywordToken()
    {
        while (char.IsLetter(Current))
            _position++;
        var length = _position - _start;
        var text = _text.ToString(_start, length);
        _kind = SyntaxFacts.GetKeywordType(text);
    }
}

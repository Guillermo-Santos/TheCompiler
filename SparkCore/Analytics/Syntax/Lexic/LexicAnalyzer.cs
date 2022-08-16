using System.Collections.Immutable;
using System.Text;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.IO.Diagnostics;
using SparkCore.IO.Text;

namespace SparkCore.Analytics.Syntax.Lexic;

internal sealed class LexicAnalyzer
{
    private readonly DiagnosticBag _diagnostics = new();
    private readonly SyntaxTree _syntaxTree;
    private readonly SourceText _text;
    private int _position;

    private int _start;
    private SyntaxKind _kind;
    private object? _value;
    private readonly ImmutableArray<SyntaxTrivia>.Builder _triviaBuilder = ImmutableArray.CreateBuilder<SyntaxTrivia>();

    public LexicAnalyzer(SyntaxTree syntaxTree)
    {
        _syntaxTree = syntaxTree;
        _text = syntaxTree.Text;
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
        ReadTrivia(leading: true);

        var leadingTrivia = _triviaBuilder.ToImmutable();
        var tokenStart = _position;

        ReadToken();
        var tokenKind = _kind;
        var tokenValue = _value;
        var tokenLength = _position - _start;

        ReadTrivia(leading: false);
        var trailingTrivia = _triviaBuilder.ToImmutable();

        var tokenText = SyntaxFacts.GetText(tokenKind);
        if (tokenText == null)
            tokenText = _text.ToString(tokenStart, tokenLength);
        return new SyntaxToken(_syntaxTree, tokenKind, tokenStart, tokenText, tokenValue, leadingTrivia, trailingTrivia);
    }

    private void ReadTrivia(bool leading)
    {
        _triviaBuilder.Clear();
        var done = false;

        while (!done)
        {
            _start = _position;
            _kind = SyntaxKind.BadToken;
            _value = null;

            switch (Current)
            {
                case '\0':
                    done = true;
                    break;
                case '/':
                    if (Lookahead == '/')
                    {
                        ReadSingleLineComment();
                    }
                    else if (Lookahead == '*')
                    {
                        ReadMultiLineComment();
                    }
                    else
                    {
                        done = true;
                    }
                    break;
                case '\n':
                case '\r':
                    if (!leading)
                        done = true;
                    ReadLineBreak();
                    break;
                case ' ':
                case '\t':
                    ReadWhiteSpace();
                    break;
                default:
                    if (char.IsWhiteSpace(Current))
                    {
                        ReadWhiteSpace();
                    }
                    else
                    {
                        done = true;
                    }
                    break;
            }
            var length = _position - _start;
            if (length > 0)
            {
                var text = _text.ToString(_start, length);
                var trivia = new SyntaxTrivia(_syntaxTree, _kind, _start, text);
                _triviaBuilder.Add(trivia);
            }
        }
    }
    private void ReadLineBreak()
    {
        if (Current == '\r' && Lookahead == '\n')
        {
            _position += 2;
        }
        else
        {
            _position++;
        }

        _kind = SyntaxKind.LineBreakTrivia;
    }
    private void ReadWhiteSpace()
    {
        var done = false;
        while (!done)
        {
            switch (Current)
            {
                case '\0':
                case '\r':
                case '\n':
                    done = true;
                    break;
                default:
                    if (!char.IsWhiteSpace(Current))
                        done = true;
                    else
                        _position++;
                    break;
            }
        }

        _kind = SyntaxKind.WhiteSpaceTrivia;
    }
    private void ReadSingleLineComment()
    {
        // Skip '//'
        _position += 2;
        var done = false;

        while (!done)
        {
            switch (Current)
            {
                case '\0':
                case '\r':
                case '\n':
                    done = true;
                    break;
                default:
                    _position++;
                    break;
            }
        }

        _kind = SyntaxKind.SingleLineCommentTrivia;
    }
    private void ReadMultiLineComment()
    {
        // Skip '/*'
        _position += 2;
        var done = false;
        while (!done)
        {
            switch (Current)
            {
                case '\0':
                    var span = new TextSpan(_start, 2);
                    var location = new TextLocation(_text, span);
                    _diagnostics.ReportUnterminedMultiLineComment(location);
                    done = true;
                    break;
                case '*':
                    if (Lookahead == '/')
                    {
                        _position++;
                        done = true;
                    }
                    _position++;
                    break;
                default:
                    _position++;
                    break;
            }
        }
        _kind = SyntaxKind.MultiLineCommentTrivia;
    }
    private void ReadToken()
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
            case '_':
                ReadIdentifierOrKeywordToken();
                break;
            default:
                if (char.IsLetter(Current))
                {
                    ReadIdentifierOrKeywordToken();
                }
                else
                {
                    var span = new TextSpan(_position, 1);
                    var location = new TextLocation(_text, span);
                    _diagnostics.ReportBadCharacter(location, Current);
                    _position++;
                }
                break;
        }
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
                    var location = new TextLocation(_text, span);
                    _diagnostics.ReportUnterminedString(location);
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
    private void ReadNumberToken()
    {
        while (char.IsDigit(Current))
            _position++;

        var length = _position - _start;
        var text = _text.ToString(_start, length);
        if (!int.TryParse(text, out var value))
        {
            var span = new TextSpan(_start, length);
            var location = new TextLocation(_text, span);
            _diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Int);
        }
        _value = value;
        _kind = SyntaxKind.NumberToken;
    }
    private void ReadIdentifierOrKeywordToken()
    {
        while (char.IsLetter(Current) || char.Equals(Current, '_') || char.IsDigit(Current))
            _position++;
        var length = _position - _start;
        var text = _text.ToString(_start, length);
        _kind = SyntaxFacts.GetKeywordType(text);
    }
}

using System;
using System.Collections.Immutable;

namespace SparkCore.Analytics.Text
{
    public sealed class SourceText
    {

        private readonly string _text;
        public SourceText(string text)
        {
            Lines = ParseLines(this, text);
            _text = text;
        }
        public ImmutableArray<TextLine> Lines { get; }
        public char this[int index] => _text[index];
        public int Length => _text.Length;
        public int GetLineIndex(int position)
        {
            var lower = 0;
            var upper = Lines.Length - 1;
            while(lower <= upper)
            {
                var index = lower + (upper - lower) / 2;
                var start = Lines[index].Start;
                
                if(position == start)
                    return index;
                
                
                if(start > position)
                {
                    upper = index + 1;
                }
                else
                {
                    lower = index + 1;
                }
            }
            return lower - 1;
        }
        private ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
        {
            var result = ImmutableArray.CreateBuilder<TextLine>();
            var position = 0;
            var lineStart = 0;
            for(int i = 0; i < text.Length; i++)
            {
                var lineBreakWidth = GetLineBreakWidth(text, i);
                if(lineBreakWidth == 0)
                {
                    position++;
                }
                else
                {
                    AddLine(result, sourceText, position, lineStart, lineBreakWidth);

                    position += lineBreakWidth;
                    lineStart = position;
                }
            }
            if (position > lineStart)
                AddLine(result, sourceText, position, lineStart, 0);
            return result.ToImmutableArray();
        }

        private static void AddLine(ImmutableArray<TextLine>.Builder result,SourceText sourceText, int position, int lineStart, int lineBreakWidth)
        {
            var lineLenght = position - lineStart;
            var lineLengthIncludingLinewBreak = lineLenght + lineBreakWidth;
            var line = new TextLine(sourceText, lineStart, lineLenght, lineLengthIncludingLinewBreak);
            result.Add(line);
        }
        

        private int GetLineBreakWidth(string text, int i)
        {
            var c = text[i];
            var l = i + 1 >= text.Length ? '\0' : text[i + 1];
            if (c == '\r' && l == '\n')
                return 2;
            else if (c == '\r' || c == '\n')
                return 1;
            return 0;
        }

        public static SourceText From(string text)
        {
            return new SourceText(text);
        }

        public override string ToString() => _text;
        public string ToString(int start, int length) => _text.Substring(start, length);
        public string ToString(TextSpan span) => _text.Substring(span.Start, span.Length);

    }

}

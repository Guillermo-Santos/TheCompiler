namespace SparkCore.Analytics.Text
{
    public sealed class TextLine
    {
        public TextLine(SourceText text, int start, int lenght, int lengthIncludingLineBreak)
        {
            Text = text;
            Start = start;
            Lenght = lenght;
            LengthIncludingLineBreak = lengthIncludingLineBreak;
        }

        public SourceText Text
        {
            get;
        }
        public int Start
        {
            get;
        }
        public int Lenght
        {
            get;
        }
        public int End => Start + Lenght;
        public int LengthIncludingLineBreak
        {
            get;
        }
        public TextSpan Span => new TextSpan(Start, Lenght);
        public TextSpan SpanIncludinLineBreak => new TextSpan(Start, LengthIncludingLineBreak);
        public override string ToString() => Text.ToString(Span);
    }

}

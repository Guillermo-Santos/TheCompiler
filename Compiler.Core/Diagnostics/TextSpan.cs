namespace Compiler.Core.Diagnostics
{
    public struct TextSpan
    {
        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }
        public int Start;
        public int Length;
        public int End => Start + Length;
    }
}

using System;
using System.IO;

namespace SparkCore.Analytics.Text
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

        public static TextSpan FromBounds(int start, int end)
        {
            var lenght = end - start;
            return new TextSpan(start, lenght);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SparkCore.Analytics.Text;

namespace Compiler.spi.Replies;
internal class TextSpanComparer : IComparer<TextSpan>
{
    public int Compare(TextSpan x, TextSpan y)
    {
        var cmp = x.Start - y.Start;
        if (cmp == 0)
            cmp = x.Length - y.Length;
        return cmp;
    }
}

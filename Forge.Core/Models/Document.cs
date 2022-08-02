using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SparkCore.IO.Text;

namespace Forge.Core.Models;
public sealed class Document
{
    public string FileName
    {
        get;
        set;
    }
    public string Text
    {
        get;
        set;
    }

    public SourceText GetSourceText() => new(Text, FileName);
}

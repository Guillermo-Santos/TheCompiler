using System.Collections.Generic;

namespace Compiler.Core.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxType Type { get; }
        public abstract IEnumerable<SyntaxNode> GetChildren();
    }
}

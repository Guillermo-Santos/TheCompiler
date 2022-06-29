using SparkCore.Analytics.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SparkCore.Analytics.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxType Type { get; }
        public virtual TextSpan Span
        {
            get
            {
                var first = GetChildren().First().Span;
                var last = GetChildren().Last().Span;
                return TextSpan.FromBounds(first.Start, last.End);
            }
        }
        public IEnumerable<SyntaxNode> GetChildren()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
                {
                    var child = (SyntaxNode)property.GetValue(this);
                    yield return child;
                }else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
                {
                    var children = (IEnumerable<SyntaxNode>)property.GetValue(this);
                    foreach(var child in children)
                        yield return child;
                }
            }
        }
        public void WriteTo(TextWriter writter)
        {
            PrettyPrint(writter, this);
        }
        private static void PrettyPrint(TextWriter writter,SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";
            writter.Write(indent);
            writter.Write(marker);
            writter.Write(node.Type);
            if (node is SyntaxToken t && t.Value != null)
            {
                writter.Write(" ");
                writter.Write(t.Value);
            }

            writter.WriteLine();

            indent += isLast ? "   " : "│  ";
            var lastChild = node.GetChildren().LastOrDefault();
            foreach (var child in node.GetChildren())
                PrettyPrint(writter,child, indent, child == lastChild);
        }

        public override string ToString()
        {
            using (var writter = new StringWriter())
            {
                WriteTo(writter);
                return writter.ToString();
            }
        }

    }
}

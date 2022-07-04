using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SparkCore.Analytics.Binding.Scope.Expressions;
using SparkCore.Analytics.Binding.Scope.Statements;

namespace SparkCore.Analytics.Binding.Scope
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind
        {
            get;
        }


        public IEnumerable<BoundNode> GetChildren()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType))
                {
                    var child = (BoundNode)property.GetValue(this);
                    if (child != null)
                        yield return child;
                }
                else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                {
                    var children = (IEnumerable<BoundNode>)property.GetValue(this);
                    foreach (var child in children)
                    {
                        if (child != null)
                            yield return child;
                    }
                }
            }
        }
        private IEnumerable<(string Name, object Value)> GetProperties()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.Name == nameof(Kind) ||
                    property.Name == nameof(BoundBinaryExpression.Op))
                    continue;
                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType) ||
                    typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                    continue;
                var value = property.GetValue(this);
                if (value != null)
                    yield return (property.Name, value);
            }
        }

        public void WriteTo(TextWriter writter)
        {
            PrettyPrint(writter, this);
        }

        private static void PrettyPrint(TextWriter writter, BoundNode node, string indent = "", bool isLast = true)
        {
            var isToConsole = writter == Console.Out;
            var marker = isLast ? "└──" : "├──";

            if (isToConsole)
                Console.ForegroundColor = ConsoleColor.DarkGray;
            writter.Write(indent);
            writter.Write(marker);


            if (isToConsole)
                Console.ForegroundColor = GetColor(node);

            var text = GetText(node);
            writter.Write(text);

            var isFirstProperty = true;

            foreach (var p in node.GetProperties())
            {
                if (isFirstProperty)
                {
                    isFirstProperty = false;
                }
                else
                {
                    if (isToConsole)
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    writter.Write(",");
                }
                writter.Write(" ");

                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.Yellow;

                writter.Write(p.Name);

                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                writter.Write(" = ");

                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.DarkYellow;

                writter.Write(p.Value);
            }

            if (isToConsole)
                Console.ResetColor();


            writter.WriteLine();

            indent += isLast ? "   " : "│  ";
            var lastChild = node.GetChildren().LastOrDefault();
            foreach (var child in node.GetChildren())
                PrettyPrint(writter, child, indent, child == lastChild);
        }

        private static ConsoleColor GetColor(BoundNode node)
        {
            switch (node)
            {
                case BoundExpression:
                    return ConsoleColor.Blue;
                case BoundStatement:
                    return ConsoleColor.Cyan;
                default:
                    return ConsoleColor.Yellow;
            }
        }

        private static string GetText(BoundNode node)
        {
            if (node is BoundBinaryExpression b)
                return b.Op.Kind.ToString() + "Expression";
            if (node is BoundUnaryExpression u)
                return u.Op.Kind.ToString() + "Expression";
            return node.Kind.ToString();
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

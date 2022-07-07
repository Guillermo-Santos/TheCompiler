using Compiler.Cnsl.Replies;

namespace Compiler.Cnsl;

internal static class Program
{
    private static void Main()
    {
        var repl = new CompilerRepl();
        repl.Run();
    }

}
using Compiler.spi.Replies;

namespace Compiler.spi;

internal static class Program
{
    private static void Main()
    {
        var repl = new CompilerRepl();
        repl.Run();
    }

}
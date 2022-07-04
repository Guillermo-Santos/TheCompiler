namespace Compiler.Conle;

internal static class Program
{
    private static void Main()
    {
        var repl = new CompilerRepl();
        repl.Run();
    }

}
using spi.Replies;

namespace spi;

internal static class Program
{
    private static void Main()
    {
        var repl = new SparkRepl();
        repl.Run();
    }

}
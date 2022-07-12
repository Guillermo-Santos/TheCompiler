namespace SparkCore.Analytics.Symbols;

public sealed class TypeSymbol : Symbol
{
    public static readonly TypeSymbol Error = new("?");
    public static readonly TypeSymbol Bool = new("bool");
    public static readonly TypeSymbol Int = new("int");
    // TODO: Agregar logica para poder usar float.
    // TODO?: Quizas agregar double y char.
    public static readonly TypeSymbol Float = new("float");
    public static readonly TypeSymbol String = new("string");
    public static readonly TypeSymbol Void = new("void");
    internal TypeSymbol(string name) : base(name)
    {
    }

    public override SymbolKind Kind => SymbolKind.Type;
}

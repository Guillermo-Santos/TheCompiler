namespace SparkCore.Analytics.Symbols;

public class LocalVariableSymbol : VariableSymbol
{
    internal LocalVariableSymbol(string name, bool isReadOnly, TypeSymbol type, Binding.BoundConstant? constant)
        : base(name, isReadOnly, type, constant)
    {
    }

    public override SymbolKind Kind => SymbolKind.LocalVariable;
}

namespace SparkCore.Analytics.Symbols;

public sealed class GlobalVariableSymbol : VariableSymbol
{
    internal GlobalVariableSymbol(string name, bool isReadOnly, TypeSymbol type, Binding.BoundConstant constant)
        : base(name, isReadOnly, type, constant)
    {
    }

    public override SymbolKind Kind => SymbolKind.GlobalVariable;
}

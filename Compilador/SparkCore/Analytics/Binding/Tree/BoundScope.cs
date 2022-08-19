using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Binding.Tree;

internal sealed class BoundScope
{
    private Dictionary<string, Symbol>? _symbols;
    public BoundScope? Parent
    {
        get;
    }

    public BoundScope(BoundScope? parent)
    {
        Parent = parent;
    }
    public bool TryDeclareVariable(VariableSymbol variable) => TryDeclareSymbol(variable);
    public bool TryDeclareFunction(FunctionSymbol function) => TryDeclareSymbol(function);

    private bool TryDeclareSymbol<TSymbol>(TSymbol symbol)
          where TSymbol : Symbol
    {
        if (_symbols == null)
            _symbols = new();
        if (_symbols.ContainsKey(symbol.Name))
            return false;
        _symbols.Add(symbol.Name, symbol);
        return true;
    }
    public Symbol? TryLookupSymbol(string name)
    {
        if (_symbols != null && _symbols.TryGetValue(name, out var symbol))
            return symbol;
        return Parent?.TryLookupSymbol(name);
    }

    public ImmutableArray<VariableSymbol> GetDeclaredVariales() => GetDeclaredSymbols<VariableSymbol>();
    public ImmutableArray<FunctionSymbol> GetDeclaredFunctions() => GetDeclaredSymbols<FunctionSymbol>();
    public ImmutableArray<TSymbol> GetDeclaredSymbols<TSymbol>()
        where TSymbol : Symbol
    {
        if (_symbols == null)
            return ImmutableArray<TSymbol>.Empty;
        return _symbols.Values.OfType<TSymbol>().ToImmutableArray();
    }
}

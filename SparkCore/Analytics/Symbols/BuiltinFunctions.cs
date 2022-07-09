using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace SparkCore.Analytics.Symbols;

internal static class BuiltinFunctions
{
    public static FunctionSymbol Print = new("print",
                                             ImmutableArray.Create(new ParameterSymbol("text", TypeSymbol.String)), 
                                             TypeSymbol.Void);
    public static FunctionSymbol Input = new("input",
                                             ImmutableArray<ParameterSymbol>.Empty, 
                                             TypeSymbol.String);
    public static FunctionSymbol Random = new("random",
                                             ImmutableArray.Create(new ParameterSymbol("max", TypeSymbol.Int)),
                                             TypeSymbol.Int);
    internal static IEnumerable<FunctionSymbol> GetAll()
        => typeof(BuiltinFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
                                   .Where(f => f.FieldType == typeof(FunctionSymbol))
                                   .Select(f => (FunctionSymbol)f.GetValue(null));
}

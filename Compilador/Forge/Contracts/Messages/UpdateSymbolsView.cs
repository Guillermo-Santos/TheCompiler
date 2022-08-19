using System.Collections.Immutable;
using CommunityToolkit.Mvvm.Messaging.Messages;
using SparkCore.Analytics.Symbols;

namespace Forge.Contracts.Messages;

internal class UpdateSymbolsView : ValueChangedMessage<ImmutableArray<Symbol>>
{
    public UpdateSymbolsView(ImmutableArray<Symbol> symbols) : base(symbols)
    {
    }
}

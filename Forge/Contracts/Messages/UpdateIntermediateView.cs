using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Forge.Contracts.Messages;

internal class UpdateIntermediateView : ValueChangedMessage<string>
{
    public UpdateIntermediateView(string intermediate) : base(intermediate)
    {
    }
}

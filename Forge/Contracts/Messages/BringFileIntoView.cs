using CommunityToolkit.Mvvm.Messaging.Messages;
using Forge.Models;
using SparkCore.IO.Diagnostics;

namespace Forge.Contracts.Messages;

internal class BringFileIntoView : ValueChangedMessage<(Document document, Diagnostic diagnostic)>
{
    public BringFileIntoView((Document document, Diagnostic diagnostic) value) : base(value)
    {
    }
}

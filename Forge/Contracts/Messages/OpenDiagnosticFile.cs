using CommunityToolkit.Mvvm.Messaging.Messages;
using SparkCore.IO.Diagnostics;

namespace Forge.Contracts.Messages;

internal class OpenDiagnosticFile : ValueChangedMessage<Diagnostic>
{
    public OpenDiagnosticFile(Diagnostic value) : base(value)
    {
    }
}

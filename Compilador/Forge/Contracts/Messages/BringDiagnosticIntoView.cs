using CommunityToolkit.Mvvm.Messaging.Messages;
using SparkCore.IO.Diagnostics;
using SparkCore.IO.Text;

namespace Forge.Contracts.Messages;

internal class BringDiagnosticIntoView : ValueChangedMessage<Diagnostic>
{
    public BringDiagnosticIntoView(Diagnostic value) : base(value)
    {
    }
}
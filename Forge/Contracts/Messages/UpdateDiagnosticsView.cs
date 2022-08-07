using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;
using SparkCore.IO.Diagnostics;

namespace Forge.Contracts.Messages;
internal class UpdateDiagnosticsView : ValueChangedMessage<ImmutableArray<Diagnostic>>
{
    public UpdateDiagnosticsView(ImmutableArray<Diagnostic> diagnostics) : base(diagnostics)
    {
    }
}

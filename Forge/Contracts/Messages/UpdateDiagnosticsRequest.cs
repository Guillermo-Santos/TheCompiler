using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;
using SparkCore.IO.Diagnostics;

namespace Forge.Contracts.Messages;
internal class UpdateDiagnosticsRequest : ValueChangedMessage<Diagnostic>
{
    public UpdateDiagnosticsRequest(Diagnostic value) : base(value)
    {
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;

using Compiler.Core.Models;

namespace Compiler.Core.Contracts.Services;

// Remove this class once your pages/features are using your data.
public interface ISampleDataService
{
    Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();
}

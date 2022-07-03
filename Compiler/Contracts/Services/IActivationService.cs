using System.Threading.Tasks;

namespace Compiler.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}

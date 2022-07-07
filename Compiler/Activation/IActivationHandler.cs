using System.Threading.Tasks;

namespace Compiler.Activation;

public interface IActivationHandler
{
    bool CanHandle(object args);

    Task HandleAsync(object args);
}

using System.Threading;
using System.Threading.Tasks;

namespace ApiClient.Services;

public interface IInternalApiClientNetStandard
{
    Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken);
}
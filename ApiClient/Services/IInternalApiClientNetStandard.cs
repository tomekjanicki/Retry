using System.Threading;
using System.Threading.Tasks;

namespace ApiClient.Services;

public interface IInternalApiClientNetStandard
{
    Task<string> GetTimeAsString(bool fail, int? delayInMilliseconds, CancellationToken cancellationToken = default);
}
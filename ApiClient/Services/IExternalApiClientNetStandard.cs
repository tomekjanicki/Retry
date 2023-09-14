using System.Threading;
using System.Threading.Tasks;

namespace ApiClient.Services;

public interface IExternalApiClientNetStandard
{
    Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken = default);
}
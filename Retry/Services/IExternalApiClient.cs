using OneOf;
using OneOf.Types;

namespace Retry.Services;

public interface IExternalApiClient
{
    Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetItems(bool fail, CancellationToken cancellationToken = default);

    Task<OneOf<string, NotFound, ApiError>> GetUserFullNameById(int id, bool fail, CancellationToken cancellationToken = default);
}
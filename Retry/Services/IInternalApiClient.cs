using OneOf.Types;
using OneOf;

namespace Retry.Services;

public interface IInternalApiClient
{
    Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken = default);

    Task<OneOf<string, NotFound, ApiError>> GetUserFullNameById(int id, bool fail, CancellationToken cancellationToken = default);
}
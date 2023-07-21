using OneOf;
using OneOf.Types;

namespace Retry.Services;

public interface IExternalApiClient
{
    Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> GetItems(bool fail, CancellationToken cancellationToken);

    Task<OneOf<string, NotFound, Error>> GetUserFullNameById(int id, bool fail, CancellationToken cancellationToken);
}
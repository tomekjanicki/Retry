namespace Retry.Services;

public interface IInternalApiClient
{
    Task<string> GetTimeAsString(bool fail, CancellationToken cancellationToken);
}
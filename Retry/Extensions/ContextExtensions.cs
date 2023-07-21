using Polly;

namespace Retry.Extensions;

public static class ContextExtensions
{
    public static T GetValue<T>(this Context context, string key) => (T)context[key];
}
using Polly;

namespace Retry.Extensions;

public static class ContextExtensions
{
    public static T GetValue<T>(this Context context, string key) => (T)context[key];

    public static T? TryGetValue<T>(this Context context, string key)
        where T : class =>
        context.TryGetValue(key, out var value) ? (T)value : null;
}
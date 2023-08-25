using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApiClient.Extensions;

public static class HttpResponseMessageExtensions
{
    internal static async Task EnsureSuccessStatusCodeWithContentInfo(this HttpResponseMessage httpResponseMessage)
    {
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            var content = await httpResponseMessage.GetContent().ConfigureAwait(false);
            var messageBuilder = new StringBuilder();
            messageBuilder.Append($"Request failed. Status code: {httpResponseMessage.StatusCode}, Reason: {httpResponseMessage.ReasonPhrase}");
            if (content != string.Empty)
            {
                messageBuilder.Append($", Additional info: {content}");
            }
            var exception = new HttpRequestException(messageBuilder.ToString(), null);
            exception.SetStatusCode(httpResponseMessage.StatusCode);

            throw exception;
        }
    }

    private static async Task<string> GetContent(this HttpResponseMessage httpResponseMessage)
    {
        try
        {
            return await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        catch
        {
            return string.Empty;
        }
    }
}
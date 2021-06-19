using System.Threading.Tasks;
using System.Net.Http;

namespace backend
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task EnsureSuccessStatusCodeAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.Content != null)
                response.Content.Dispose();

            throw new SimpleHttpResponseException(response.StatusCode, content);
        }
    }
}

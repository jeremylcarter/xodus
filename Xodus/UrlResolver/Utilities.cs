using System.Net.Http;

namespace UrlResolver
{
    internal class Utilities
    {
        public static HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-us,en;q=0.8");
            return httpClient;
        }
    }
}
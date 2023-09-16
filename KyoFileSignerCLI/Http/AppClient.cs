using System.Net.Http.Headers;
using KyoFileSignerCLI.Interfaces;

namespace KyoFileSignerCLI.Http
{
    public class AppClient : IAppClient
    {
        public HttpClient Client { get; }

        public AppClient(HttpClient httpClient)
        {
            Client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public static HttpRequestMessage GetRequest(string apiPath)
        {
            return new HttpRequestMessage(HttpMethod.Get, apiPath);
        }

        public static HttpRequestMessage PostRequest(string apiPath)
        {
            return new HttpRequestMessage(HttpMethod.Post, apiPath);
        }

        public static HttpRequestMessage GetRequest(string apiPath, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, apiPath);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }

        public static HttpRequestMessage PostRequest(string apiPath, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, apiPath);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }

        public static HttpRequestMessage PatchRequest(string apiPath, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, apiPath);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }
    }
}
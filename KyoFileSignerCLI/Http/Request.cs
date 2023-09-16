using System.Net;
using System.Text.Json;

namespace KyoFileSignerCLI.Http
{
    public class Request
    {
        public async Task ReadOnly<T>(HttpResponseMessage? response, Action<IResponse> successHandler, CancellationToken cancellationToken)
        {
            await CheckFailedResponse(response, cancellationToken);
            using (var contentStream = await response.Content.ReadAsStreamAsync())
            {
                IResponse.Only<T>? responseModel = null;
                // Check if StatusCode is one from Unauthorized, Forbidden, BadRequest, etc. this will always return a single object
                try
                {
                    responseModel = await JsonSerializer.DeserializeAsync<IResponse.Only<T>>(contentStream, DefaultJsonSerializerOptions.Options, cancellationToken);
                }
                catch (Exception)
                {
                    throw;
                }
                if (responseModel != null && responseModel.GetType() == typeof(IResponse.Only<T>))
                {
                    successHandler(responseModel);
                }
            }
        }

        public async Task ReadMany<T>(HttpResponseMessage? response, Action<IResponse> successHandler, CancellationToken cancellationToken)
        {
            await CheckFailedResponse(response, cancellationToken);
            using (var contentStream = await response.Content.ReadAsStreamAsync())
            {
                IResponse.Many<T>? responseModel = null;
                try
                {
                    responseModel = await JsonSerializer.DeserializeAsync<IResponse.Many<T>>(contentStream, DefaultJsonSerializerOptions.Options, cancellationToken);
                }
                catch (Exception)
                {
                    throw;
                }
                if (responseModel != null && responseModel.GetType() == typeof(IResponse.Many<T>))
                {
                    successHandler(responseModel);
                }
            }
        }

        private async Task CheckFailedResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            IResponse? responseModel = null;
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            // We assume that if errors occur, a nullable result is returned, we provide nullable object (object?) as type parameter
            using (var contentStream = await response.Content.ReadAsStreamAsync())
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    responseModel = await JsonSerializer.DeserializeAsync<IResponse.Only<object?>>(contentStream, DefaultJsonSerializerOptions.Options, cancellationToken);
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    responseModel = await JsonSerializer.DeserializeAsync<IResponse.Only<object?>>(contentStream, DefaultJsonSerializerOptions.Options, cancellationToken);
                }
                else if (!response.IsSuccessStatusCode)
                {
                    responseModel = await JsonSerializer.DeserializeAsync<IResponse.Only<object?>>(contentStream, DefaultJsonSerializerOptions.Options, cancellationToken);
                }
            }
            
            throw new HttpRequestException($"HttpRequestException {response.StatusCode}\n{responseModel.GetErrors()}\n");
        }
    }
}
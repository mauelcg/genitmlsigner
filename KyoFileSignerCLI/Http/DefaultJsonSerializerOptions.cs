using System.Text.Json;

namespace KyoFileSignerCLI.Http
{
    public static class DefaultJsonSerializerOptions
    {
        public static JsonSerializerOptions Options => new JsonSerializerOptions { PropertyNameCaseInsensitive = true};
    }
}
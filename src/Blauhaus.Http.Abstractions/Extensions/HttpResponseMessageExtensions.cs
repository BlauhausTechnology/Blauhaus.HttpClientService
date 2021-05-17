using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blauhaus.Http.Abstractions.Errors;
using Newtonsoft.Json;

namespace Blauhaus.Http.Abstractions.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<string> EnsureSuccessStatusCodeAsync(this HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return content;
            }

            if (response.Content != null)
                response.Content.Dispose();

            var method = response.RequestMessage == null 
                ? "Unknown" 
                : response.RequestMessage.Method.Method;

            var uri = response.RequestMessage == null || response.RequestMessage.RequestUri == null
                ? "Unknown" 
                : response.RequestMessage.RequestUri.AbsoluteUri;

            var error = JsonConvert.DeserializeObject<HttpError>(content);
            if (error != null && !string.IsNullOrEmpty(error.Message))
            {
                content = error.Message;
            }

            throw new HttpResponseException(response.StatusCode, method, uri, content);
        }

        public static async Task<TDto> UnwrapResponseAsync<TDto>(this HttpResponseMessage response)
        {
            var content = await response.EnsureSuccessStatusCodeAsync();

            var dto = JsonConvert.DeserializeObject<TDto>(content);
            if (dto == null)
            {
                throw new Exception($"Value for {typeof(TDto).Name} was null");
            }

            return dto; 
        }

}
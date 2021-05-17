using System.Net.Http;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;

namespace Blauhaus.Http.Abstractions.Extensions
{
    public static class ObjectExtensions
    {
        public static HttpContent ToHttpContent<T>(this T objectToSerialize)
        {
            var json = JsonConvert.SerializeObject(objectToSerialize);
            return new StringContent(json, new UTF8Encoding(), "application/json");
        }
    }
}
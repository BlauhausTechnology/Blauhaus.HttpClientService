using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.HttpClientService.Request;
using Blauhaus.HttpClientService.Tests.Mocks;
using Blauhaus.HttpClientService.Tests.Tests._Base;
using Newtonsoft.Json.Linq;

namespace Blauhaus.HttpClientService.Tests.Tests
{
    public class PatchWrapperAsyncTests : BaseWrappedRequestTest<HttpRequestWrapper<JObject>>
    {
        protected override Task<TestDto> ExecuteAsync(HttpRequestWrapper<JObject> wrapper, CancellationToken token)
        {
            return Sut.PatchAsync<TestDto>(wrapper, token);
        }

        protected override HttpRequestWrapper<JObject> GetWrapper()
        {
            return new HttpRequestWrapper<JObject>("http://baseaddress.com/testroute", new JObject {["TestDtoProperty"] = "hello world"});
        }

        protected override HttpMethod GetHttpMethod()
        {
            return HttpMethod.Patch;
        }
    }
}
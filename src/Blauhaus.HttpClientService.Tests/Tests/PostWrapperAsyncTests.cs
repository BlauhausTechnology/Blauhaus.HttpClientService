using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.HttpClientService.Request;
using Blauhaus.HttpClientService.Tests.Mocks;
using Blauhaus.HttpClientService.Tests.Tests._Base;

namespace Blauhaus.HttpClientService.Tests.Tests
{
    public class PostWrapperAsyncTests : BaseWrappedRequestTest<HttpRequestWrapper<TestDto>>
    {
        protected override Task<TestDto> ExecuteAsync(HttpRequestWrapper<TestDto> wrapper, CancellationToken token)
        {
            return Sut.PostAsync<TestDto, TestDto>(wrapper, token);
        }

        protected override HttpRequestWrapper<TestDto> GetWrapper()
        {
            return new HttpRequestWrapper<TestDto>("http://baseaddress.com/testroute", new TestDto {TestDtoProperty = "hello world"});
        }

        protected override HttpMethod GetHttpMethod()
        {
            return HttpMethod.Post;
        }
    }
}
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.HttpClientService.Request;
using Blauhaus.HttpClientService.Tests.Mocks;
using Blauhaus.HttpClientService.Tests.Tests._Base;

namespace Blauhaus.HttpClientService.Tests.Tests
{
    public class DeleteWrapperAsyncTests : BaseWrappedRequestTest<HttpRequestWrapper>
    {

        protected override Task<TestDto> ExecuteAsync(HttpRequestWrapper wrapper, CancellationToken token)
        {
            return Sut.DeleteAsync<TestDto>(wrapper, CancellationToken.None);
        }

        protected override HttpRequestWrapper GetWrapper()
        {
            return new HttpRequestWrapper("http://baseaddress.com/testroute");
        }
        
        protected override HttpMethod GetHttpMethod()
        {
            return HttpMethod.Delete;
        }
    }
}
using System.Threading;
using Blauhaus.HttpClientService.Abstractions;
using Blauhaus.TestHelpers.MockBuilders;
using Moq;

namespace Blauhaus.HttpClientService.TestHelpers.MockBuilders
{
    public class HttpClientServiceMockBuilder : BaseMockBuilder<HttpClientServiceMockBuilder, IHttpClientService>
    {
        public HttpClientServiceMockBuilder Where_PostAsync_returns<TRequest, TResponse>(TResponse response)
        {
            Mock.Setup(x => x.PostAsync<TRequest, TResponse>(It.IsAny<IHttpRequestWrapper<TRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            return this;
        }
         
    }
}
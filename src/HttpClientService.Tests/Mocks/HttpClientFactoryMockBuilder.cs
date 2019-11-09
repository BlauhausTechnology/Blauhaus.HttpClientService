using System.Net.Http;
using Blauhaus.Tests.Helpers;
using Moq;

namespace HttpClient.Tests.Mocks
{
    public class HttpClientFactoryMockBuilder : BaseMockBuilder<HttpClientFactoryMockBuilder, IHttpClientFactory>
    {

        public HttpClientFactoryMockBuilder Where_CreateClient_returns(System.Net.Http.HttpClient client)
        {
            Mock.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(client);
            return this;
        }
    }
}
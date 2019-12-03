using Blauhaus.Common.TestHelpers;
using Blauhaus.HttpClientService.Abstractions;
using Blauhaus.HttpClientService.Config;

namespace Blauhaus.HttpClientService.Tests.Mocks
{
    public class HttpClientServiceConfigMockBuilder : BaseMockBuilder<HttpClientServiceConfigMockBuilder, IHttpClientServiceConfig>
    {
        public HttpClientServiceConfigMockBuilder()
        {
        }
    }
}
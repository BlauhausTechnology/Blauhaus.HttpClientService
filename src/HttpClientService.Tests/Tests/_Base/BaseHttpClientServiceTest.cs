using Blauhaus.Tests.Helpers;
using HttpClient.Tests.Mocks;
using NUnit.Framework;

namespace HttpClient.Tests.Tests._Base
{
    public class BaseHttpClientServiceTest : BaseUnitTest<Core.Service.HttpClientService>
    {
        protected HttpClientFactoryMockBuilder MockHttpClientFactory;
        protected MockMessageHandlerBuilder MockMessageHandler;
        protected HttpClientServiceConfigMockBuilder MockClientServiceConfig;

        protected class TestDto 
        {
            public string TestDtoProperty { get; set; }
        }

        protected override Core.Service.HttpClientService ConstructSut()
        {
            return new Core.Service.HttpClientService(MockClientServiceConfig.Object, MockHttpClientFactory.Object);
        }

        [SetUp]
        protected virtual void OnSetup()
        {
            MockHttpClientFactory = new HttpClientFactoryMockBuilder();
            MockMessageHandler = new MockMessageHandlerBuilder();
            MockClientServiceConfig = new HttpClientServiceConfigMockBuilder();
            Cleanup();
        }
    }
}
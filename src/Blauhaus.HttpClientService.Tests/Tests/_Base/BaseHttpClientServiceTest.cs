using Blauhaus.HttpClientService.Tests.Mocks;
using Blauhaus.Loggers.Common.Abstractions;
using Blauhaus.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace Blauhaus.HttpClientService.Tests.Tests._Base
{
    public class BaseHttpClientServiceTest : BaseUnitTest<Service.HttpClientService>
    {
        protected HttpClientFactoryMockBuilder MockHttpClientFactory;
        protected MockMessageHandlerBuilder MockMessageHandler;
        protected HttpClientServiceConfigMockBuilder MockClientServiceConfig;

       

        protected override Service.HttpClientService ConstructSut()
        {
            return new Service.HttpClientService(MockClientServiceConfig.Object, MockHttpClientFactory.Object, Mock.Of<ILogService>());
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
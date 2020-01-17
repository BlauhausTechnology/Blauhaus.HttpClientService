using System.Collections.Generic;
using Blauhaus.Auth.Abstractions.ClientAuthenticationHandlers;
using Blauhaus.Common.TestHelpers;
using Blauhaus.HttpClientService.Tests.Mocks;
using Blauhaus.Loggers.Common.Abstractions;
using Moq;
using NUnit.Framework;

namespace Blauhaus.HttpClientService.Tests.Tests._Base
{
    public class BaseHttpClientServiceTest : BaseUnitTest<Service.HttpClientService>
    {
        protected HttpClientFactoryMockBuilder MockHttpClientFactory;
        protected MockMessageHandlerBuilder MockMessageHandler;
        protected HttpClientServiceConfigMockBuilder MockClientServiceConfig;
        protected MockBuilder<IAuthenticatedAccessToken> MockAccessToken;

       

        protected override Service.HttpClientService ConstructSut()
        {
            return new Service.HttpClientService(
                MockClientServiceConfig.Object, 
                MockHttpClientFactory.Object, 
                Mock.Of<ILogService>(), 
                MockAccessToken.Object);
        }

        [SetUp]
        protected virtual void OnSetup()
        {
            MockAccessToken = new MockBuilder<IAuthenticatedAccessToken>();
            MockHttpClientFactory = new HttpClientFactoryMockBuilder();
            MockMessageHandler = new MockMessageHandlerBuilder();
            MockClientServiceConfig = new HttpClientServiceConfigMockBuilder();
            MockAccessToken = new MockBuilder<IAuthenticatedAccessToken>()
                .With(x => x.AdditionalHeaders, new Dictionary<string, string>());
            Cleanup();
        }
    }
}
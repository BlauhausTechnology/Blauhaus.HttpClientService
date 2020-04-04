using System.Collections.Generic;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Auth.Abstractions.AccessToken;
using Blauhaus.HttpClientService.Tests.Mocks;
using Blauhaus.TestHelpers;
using Blauhaus.TestHelpers.BaseTests;
using Blauhaus.TestHelpers.Http.MockBuilders;
using Blauhaus.TestHelpers.MockBuilders;
using NUnit.Framework;

namespace Blauhaus.HttpClientService.Tests.Tests._Base
{
    public class BaseHttpClientServiceTest : BaseUnitTest<Service.HttpClientService>
    {
        protected HttpClientFactoryMockBuilder MockHttpClientFactory;
        protected HttpMessageHandlerMockBuilder MockMessageHandler;
        protected HttpClientServiceConfigMockBuilder MockClientServiceConfig;
        protected MockBuilder<IAuthenticatedAccessToken> MockAccessToken;
        protected MockBuilder<IAnalyticsService> MockAnalyticsService;

       

        protected override Service.HttpClientService ConstructSut()
        {
            return new Service.HttpClientService(
                MockClientServiceConfig.Object, 
                MockHttpClientFactory.Object, 
                MockAnalyticsService.Object, 
                MockAccessToken.Object);
        }

        [SetUp]
        protected virtual void OnSetup()
        {
            Cleanup();
            MockAccessToken = new MockBuilder<IAuthenticatedAccessToken>();
            MockHttpClientFactory = new HttpClientFactoryMockBuilder();
            MockMessageHandler = new HttpMessageHandlerMockBuilder();
            MockClientServiceConfig = new HttpClientServiceConfigMockBuilder();
            MockAccessToken = new MockBuilder<IAuthenticatedAccessToken>()
                .With(x => x.AdditionalHeaders, new Dictionary<string, string>());
            MockAnalyticsService = new MockBuilder<IAnalyticsService>()
                .With(x => x.AnalyticsOperationHeaders, new Dictionary<string, string>());
        }
    }
}
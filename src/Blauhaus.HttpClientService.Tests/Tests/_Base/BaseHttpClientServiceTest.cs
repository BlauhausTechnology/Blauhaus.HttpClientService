﻿using System.Collections.Generic;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Auth.Abstractions.AccessToken;
using Blauhaus.Common.TestHelpers;
using Blauhaus.HttpClientService.Tests.Mocks;
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
            MockAccessToken = new MockBuilder<IAuthenticatedAccessToken>();
            MockHttpClientFactory = new HttpClientFactoryMockBuilder();
            MockMessageHandler = new MockMessageHandlerBuilder();
            MockClientServiceConfig = new HttpClientServiceConfigMockBuilder();
            MockAccessToken = new MockBuilder<IAuthenticatedAccessToken>()
                .With(x => x.AdditionalHeaders, new Dictionary<string, string>());
            MockAnalyticsService = new MockBuilder<IAnalyticsService>()
                .With(x => x.AnalyticsOperationHeaders, new Dictionary<string, string>());
            Cleanup();
        }
    }
}
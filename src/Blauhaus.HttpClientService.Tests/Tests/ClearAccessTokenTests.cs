using System.Threading;
using System.Threading.Tasks;
using Blauhaus.HttpClientService.Tests.Tests._Base;
using NUnit.Framework;

namespace Blauhaus.HttpClientService.Tests.Tests
{
    public class ClearAccessTokenTests : BaseHttpClientServiceTest
    {
        [Test]
        public async Task SHOULD_remove_bearer_token_from_requests()
        {
            //Arrange
            MockHttpClientFactory.Where_CreateClient_returns(new System.Net.Http.HttpClient(MockMessageHandler.Build().Object));
            Sut.HandleAccessToken("authToken");

            //Act
            Sut.ClearAccessToken();

            //Assert
            await Sut.PostAsync("http://www.google.com/", new TestDto(), CancellationToken.None);
            MockMessageHandler.VerifyAuthHeader("Bearer", "authToken", 0);
        }
    }
}
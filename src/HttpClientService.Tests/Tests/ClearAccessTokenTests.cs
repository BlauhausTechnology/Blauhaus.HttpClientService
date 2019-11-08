using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpClientService.Tests.Tests._Base;
using NUnit.Framework;

namespace HttpClientService.Tests.Tests
{
    public class ClearAccessTokenTests : BaseHttpClientServiceTest
    {
        [Test]
        public async Task SHOULD_remove_bearer_token_from_requests()
        {
            //Arrange
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler.Build().Object));
            Sut.HandleAccessToken("authToken");

            //Act
            Sut.ClearAccessToken();

            //Assert
            await Sut.PostAsync("http://www.google.com/", new TestDto(), CancellationToken.None);
            MockMessageHandler.VerifyHeader("bearer", "authToken", 0);
        }
    }
}
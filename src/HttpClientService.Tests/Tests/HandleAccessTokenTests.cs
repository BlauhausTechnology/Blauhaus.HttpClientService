using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpClientService.Tests.Tests._Base;
using NUnit.Framework;

namespace HttpClientService.Tests.Tests
{
    public class HandleAccessTokenTests : BaseHttpClientServiceTest
    {
        [Test]
        public async Task SHOULD_add_bearer_token_to_requests()
        {
            //Arrange
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler.Build().Object));

            //Act
            Sut.HandleAccessToken("authToken");

            //Assert
            await Sut.PostAsync("http://www.google.com/", new TestDto(), CancellationToken.None);
            MockMessageHandler.VerifyHeader("bearer", "authToken");
        }
    }
}
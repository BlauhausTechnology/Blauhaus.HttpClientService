using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.HttpClientService.Exceptions;
using Blauhaus.HttpClientService.Request;
using Blauhaus.HttpClientService.Service;
using Blauhaus.HttpClientService.Tests.Tests._Base;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Blauhaus.HttpClientService.Tests.Tests
{
    public class GetAsyncTests : BaseHttpClientServiceTest
    {
        [Test]
        public async Task SHOULD_post_content_to_correct_endpoint()
        {
            //Arrange
            var wrapper = new HttpRequestWrapper("http://baseaddress.com/testroute");
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler.Build().Object));

            //Act
            await Sut.GetAsync<TestDto>(wrapper, CancellationToken.None);

            //Assert
            MockMessageHandler.VerifyUri("http://baseaddress.com/testroute");
            MockMessageHandler.VerifyMethod(HttpMethod.Get);
        }

        [Test]
        public async Task SHOULD_append_query_string_parameters()
        {
            //Arrange
            var wrapper = new HttpRequestWrapper("http://baseaddress.com/testroute")
                .WithQueryStringParameter("userId", "123")
                .WithQueryStringParameter("name", "Bob");
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler.Build().Object));

            //Act
            await Sut.GetAsync<TestDto>(wrapper, CancellationToken.None);

            //Assert
            MockMessageHandler.VerifyUri("http://baseaddress.com/testroute?userId=123&name=Bob");
        }

        [Test]
        public async Task SHOULD_append_headers()
        {
            //Arrange
            var wrapper = new HttpRequestWrapper("http://baseaddress.com/testroute")
                .WithRequestHeader("userId", "123")
                .WithRequestHeader("name", "Bob");
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler.Build().Object));

            //Act
            await Sut.GetAsync<TestDto>(wrapper, CancellationToken.None);

            //Assert
            MockMessageHandler.VerifyHeader("userId","123");
            MockMessageHandler.VerifyHeader("name","Bob");
        }

        [Test]
        public async Task SHOULD_return_deserialized_dto()
        {
            //Arrange
            var wrapper = new HttpRequestWrapper("http://baseaddress.com/testroute");
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                .Where_SendAsync_returns_Content(JsonConvert.SerializeObject(new TestDto {TestDtoProperty = "Hello back"}))
                .Build().Object));

            //Act
            var result = await Sut.GetAsync<TestDto>(wrapper, CancellationToken.None);

            //Assert
            Assert.That(result.TestDtoProperty, Is.EqualTo("Hello back"));
        }

        [Test]
        public void WHEN_post_fails_with_error_message_SHOULD_throw()
        {
            //Arrange
            var wrapper = new HttpRequestWrapper("http://baseaddress.com/testroute");
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                .Where_SendAsync_returns_StatusCode(HttpStatusCode.BadRequest)
                .Where_SendAsync_returns_Content(JsonConvert.SerializeObject(new HttpError
                {
                    Message = "bad luck"
                }))
                .Build().Object));

            //Assert
            Assert.ThrowsAsync<HttpClientServiceException>(async () =>
                await Sut.GetAsync<TestDto>(wrapper, CancellationToken.None), "bad luck");
        }

        [Test]
        public void WHEN_post_fails_without_error_message_SHOULD_throw()
        {
            //Arrange
            var wrapper = new HttpRequestWrapper("http://baseaddress.com/testroute");
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                .Where_SendAsync_returns_StatusCode(HttpStatusCode.FailedDependency)
                .Where_SendAsync_returns_ReasonPhrase("Bad luck")
                .Build().Object));

            //Assert
            Assert.ThrowsAsync<HttpClientServiceException>(async () =>
                await Sut.GetAsync<TestDto>(wrapper, CancellationToken.None));

        }

        [Test]
        public void WHEN_post_returns_401_not_authorized_SHOULD_throw_UnauthorizedAccessException()
        {
            //Arrange
            var wrapper = new HttpRequestWrapper("http://baseaddress.com/testroute");
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                .Where_SendAsync_returns_StatusCode(HttpStatusCode.Unauthorized)
                .Where_SendAsync_returns_ReasonPhrase("Bad luck")
                .Build().Object));

            //Assert
            Assert.ThrowsAsync<HttpClientServiceAuthorizationException>(async () =>
                await Sut.GetAsync<TestDto>(wrapper, CancellationToken.None));
        }
    }
}
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.HttpClientService.Exceptions;
using Blauhaus.HttpClientService.Request;
using Blauhaus.HttpClientService.Request._Base;
using Blauhaus.HttpClientService.Service;
using Blauhaus.HttpClientService.Tests.Mocks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Blauhaus.HttpClientService.Tests.Tests._Base
{
    public abstract class BaseWrappedRequestTest<TWrapper> : BaseHttpClientServiceTest where TWrapper : BaseHttpRequestWrapper<TWrapper>
    {

        [Test]
        public async Task SHOULD_send_to_correct_endpoint()
        {
            //Arrange
            var wrapper = GetWrapper();
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler.Build().Object));

            //Act
            await ExecuteAsync(wrapper, CancellationToken.None);

            //Assert
            MockMessageHandler.VerifyUri("http://baseaddress.com/testroute");
            MockMessageHandler.VerifyMethod(GetHttpMethod());
        }

        [Test]
        public async Task SHOULD_append_query_string_parameters()
        {
            //Arrange
            var wrapper = GetWrapper()
                .WithQueryStringParameter("userId", "123")
                .WithQueryStringParameter("name", "Bob");
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler.Build().Object));

            //Act
            await ExecuteAsync(wrapper, CancellationToken.None);

            //Assert
            MockMessageHandler.VerifyUri("http://baseaddress.com/testroute?userId=123&name=Bob");
        }

        [Test]
        public async Task SHOULD_append_headers_if_provided()
        {
            //Arrange
            var wrapper = GetWrapper()
                .WithRequestHeader("userId", "123")
                .WithRequestHeader("name", "Bob")
                .WithAuthorizationHeader("Bearer", "bearerToken");
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler.Build().Object));

            //Act
            await ExecuteAsync(wrapper, CancellationToken.None);

            //Assert
            MockMessageHandler.VerifyHeader("userId","123");
            MockMessageHandler.VerifyHeader("name","Bob");
            MockMessageHandler.VerifyAuthHeader("Bearer","bearerToken");
        }

        [Test]
        public async Task SHOULD_append_default_headers_if_set()
        {
            //Arrange
            var wrapper = GetWrapper();
            Sut.SetDefaultRequestHeader("userId", "123");
            Sut.SetDefaultRequestHeader("name", "Bob");
            MockAccessToken
                .With(x => x.Scheme, "Bearer")
                .With(x => x.Token, "bearerToken");
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler.Build().Object));

            //Act
            await ExecuteAsync(wrapper, CancellationToken.None);

            //Assert
            MockMessageHandler.VerifyHeader("userId","123");
            MockMessageHandler.VerifyHeader("name","Bob");
            MockMessageHandler.VerifyAuthHeader("Bearer","bearerToken");
        }
        
        [Test]
        public async Task SHOULD_not_append_default_headers_if_cleared()
        {
            //Arrange
            var wrapper = GetWrapper();
            Sut.SetDefaultRequestHeader("userId", "123");
            Sut.SetDefaultRequestHeader("name", "Bob");
            MockAccessToken
                .With(x => x.Scheme, "Bearer")
                .With(x => x.Token, "bearerToken");
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler.Build().Object));

            //Act
            MockAccessToken
                .With(x => x.Scheme, "")
                .With(x => x.Token, "");
            Sut.ClearDefaultRequestHeaders();
            await ExecuteAsync(wrapper, CancellationToken.None);

            //Assert
            MockMessageHandler.VerifyHeader("userId","123", 0);
            MockMessageHandler.VerifyHeader("name","Bob", 0);
            MockMessageHandler.VerifyNoAuthHeader();
        }
        
        [Test]
        public async Task SHOULD_return_deserialized_dto()
        {
            //Arrange
            var wrapper = GetWrapper();
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                .Where_SendAsync_returns_Content(JsonConvert.SerializeObject(new TestDto {TestDtoProperty = "Hello back"}))
                .Build().Object));

            //Act
            var result = await ExecuteAsync(wrapper, CancellationToken.None);

            //Assert
            Assert.That(result.TestDtoProperty, Is.EqualTo("Hello back"));
        }

        [Test]
        public void WHEN_post_fails_with_error_message_SHOULD_throw()
        {
            //Arrange
            var wrapper = GetWrapper();
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                .Where_SendAsync_returns_StatusCode(HttpStatusCode.BadRequest)
                .Where_SendAsync_returns_Content(JsonConvert.SerializeObject(new HttpError
                {
                    Message = "bad luck"
                }))
                .Build().Object));

            //Assert
            Assert.ThrowsAsync<HttpClientServiceServerError>(async () =>
                await ExecuteAsync(wrapper, CancellationToken.None), "bad luck");
        }

        [Test]
        public void WHEN_post_fails_without_error_message_SHOULD_throw()
        {
            //Arrange
            var wrapper = GetWrapper();
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                .Where_SendAsync_returns_StatusCode(HttpStatusCode.FailedDependency)
                .Where_SendAsync_returns_ReasonPhrase("Bad luck")
                .Build().Object));

            //Assert
            Assert.ThrowsAsync<HttpClientServiceException>(async () =>
                await ExecuteAsync(wrapper, CancellationToken.None));

        }

        [Test]
        public void WHEN_post_returns_401_not_authorized_SHOULD_throw_UnauthorizedAccessException()
        {
            //Arrange
            var wrapper = GetWrapper();
            MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                .Where_SendAsync_returns_StatusCode(HttpStatusCode.Unauthorized)
                .Where_SendAsync_returns_ReasonPhrase("Bad luck")
                .Build().Object));

            //Assert
            Assert.ThrowsAsync<HttpClientServiceAuthorizationException>(async () =>
                await ExecuteAsync(wrapper, CancellationToken.None));
        }

        protected abstract Task<TestDto> ExecuteAsync(TWrapper wrapper, CancellationToken token);

        protected abstract TWrapper GetWrapper();
        protected abstract HttpMethod GetHttpMethod();
    }
}
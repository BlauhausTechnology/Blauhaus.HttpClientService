using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.HttpClientService.Abstractions.Exceptions;
using Blauhaus.HttpClientService.Service;
using Blauhaus.HttpClientService.Tests.Mocks;
using Blauhaus.HttpClientService.Tests.Tests._Base;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Blauhaus.HttpClientService.Tests.Tests
{
    public class PostAsyncTests : BaseHttpClientServiceTest
    {
        public class TypeOfResponseIsNotProvided : PostAsyncTests
        {
            [Test]
            public async Task SHOULD_post_content_to_correct_endpoint()
            {
                //Arrange
                MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler.Build().Object));

                //Act
                await Sut.PostAsync("http://baseaddress.com/testroute", new TestDto {TestDtoProperty = "hello world"}, CancellationToken.None);

                //Assert
                MockMessageHandler.VerifyUri("http://baseaddress.com/testroute");
                MockMessageHandler.VerifyContent(x => x.Contains("hello world"));
            }

            [Test]
            public async Task SHOULD_post_using_Default_RequestHeaders()
            {
                //Arrange
                MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler.Build().Object));
                Sut.SetDefaultRequestHeader("frogs", "2");
                Sut.SetDefaultRequestHeader("pudding", "yes please");

                //Act
                await Sut.PostAsync("http://baseaddress.com/testroute", new TestDto {TestDtoProperty = "hello world"}, CancellationToken.None);

                //Assert
                MockMessageHandler.VerifyHeader("frogs", "2");
                MockMessageHandler.VerifyHeader("pudding", "yes please");
            }

            [Test]
            public void WHEN_post_fails_with_error_message_SHOULD_throw_message()
            {
                //Arrange
                MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                    .Where_SendAsync_returns_StatusCode(HttpStatusCode.BadRequest)
                    .Where_SendAsync_returns_Content(JsonConvert.SerializeObject(new HttpError
                    {
                        Message = "bad luck"
                    }))
                    .Build().Object));

                //Assert
                Assert.ThrowsAsync<HttpClientServiceServerError>(async () =>
                    await Sut.PostAsync("http://baseaddress.com/testroute", new TestDto {TestDtoProperty = "hello world"}, CancellationToken.None), "bad luck");
            }

            [Test]
            public void WHEN_post_fails_without_error_message_SHOULD_throw()
            {
                //Arrange
                MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                    .Where_SendAsync_returns_StatusCode(HttpStatusCode.FailedDependency)
                    .Where_SendAsync_returns_ReasonPhrase("Bad luck")
                    .Build().Object));

                //Assert
                Assert.ThrowsAsync<HttpClientServiceException>(async () =>
                    await Sut.PostAsync("http://baseaddress.com/testroute", new TestDto {TestDtoProperty = "hello world"}, CancellationToken.None));
            }

            [Test]
            public void WHEN_post_returns_401_not_authorized_SHOULD_throw_UnauthorizedAccessException()
            {
                //Arrange
                MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                    .Where_SendAsync_returns_StatusCode(HttpStatusCode.Unauthorized)
                    .Where_SendAsync_returns_ReasonPhrase("Bad luck")
                    .Build().Object));

                //Assert
                Assert.ThrowsAsync<HttpClientServiceAuthorizationException>(async () =>
                    await Sut.PostAsync("http://baseaddress.com/testroute", new TestDto {TestDtoProperty = "hello world"}, CancellationToken.None));
            }
            
        }

        public class TypeOfResponseIsProvided : PostAsyncTests
        {
            [Test]
            public async Task WHEN_TResponse_is_provided_SHOULD_post_content_to_correct_endpoint()
            {
                //Arrange
                MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler.Build().Object));

                //Act
                await Sut.PostAsync<TestDto, TestDto>("http://baseaddress.com/testroute", new TestDto {TestDtoProperty = "hello world"}, CancellationToken.None);

                //Assert
                MockMessageHandler.VerifyUri("http://baseaddress.com/testroute");
                MockMessageHandler.VerifyContent(x => x.Contains("hello world"));
                MockMessageHandler.VerifyMethod(HttpMethod.Post);
            }

            [Test]
            public async Task WHEN_TResponse_is_provided_SHOULD_return_deserialized_dto()
            {
                //Arrange
                MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                    .Where_SendAsync_returns_Content(JsonConvert.SerializeObject(new TestDto {TestDtoProperty = "Hello back"}))
                    .Build().Object));

                //Act
                var result = await Sut.PostAsync<TestDto, TestDto>("http://baseaddress.com/testroute", new TestDto {TestDtoProperty = "hello world"}, CancellationToken.None);

                //Assert
                Assert.That(result.TestDtoProperty, Is.EqualTo("Hello back"));
            }

            [Test]
            public void WHEN_TResponse_is_provided_WHEN_post_fails_SHOULD_throw()
            {
                //Arrange
                MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                    .Where_SendAsync_returns_StatusCode(HttpStatusCode.BadRequest)
                    .Where_SendAsync_returns_Content(JsonConvert.SerializeObject(new HttpError
                    {
                        Message = "bad luck"
                    }))
                    .Build().Object));

                //Assert
                Assert.ThrowsAsync<HttpClientServiceServerError>(async () =>
                    await Sut.PostAsync<TestDto, TestDto>("http://baseaddress.com/testroute", new TestDto {TestDtoProperty = "hello world"},
                        CancellationToken.None), "bad luck");
            }

            [Test]
            public void WHEN_post_fails_without_error_message_SHOULD_throw()
            {
                //Arrange
                MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                    .Where_SendAsync_returns_StatusCode(HttpStatusCode.FailedDependency)
                    .Where_SendAsync_returns_ReasonPhrase("Bad luck")
                    .Build().Object));

                //Assert
                Assert.ThrowsAsync<HttpClientServiceException>(async () =>
                    await Sut.PostAsync<TestDto, TestDto>("http://baseaddress.com/testroute", new TestDto {TestDtoProperty = "hello world"}, CancellationToken.None));
            }

            [Test]
            public void WHEN_TResponse_is_provided_and_post_returns_401_not_authorized_SHOULD_throw_UnauthorizedAccessException()
            {
                //Arrange
                MockHttpClientFactory.Where_CreateClient_returns(new HttpClient(MockMessageHandler
                    .Where_SendAsync_returns_StatusCode(HttpStatusCode.Unauthorized)
                    .Where_SendAsync_returns_ReasonPhrase("Bad luck")
                    .Build().Object));

                //Assert
                Assert.ThrowsAsync<HttpClientServiceAuthorizationException>(async () =>
                    await Sut.PostAsync<TestDto, TestDto>("http://baseaddress.com/testroute", new TestDto {TestDtoProperty = "hello world"}, CancellationToken.None));
            }
        }

    }
}
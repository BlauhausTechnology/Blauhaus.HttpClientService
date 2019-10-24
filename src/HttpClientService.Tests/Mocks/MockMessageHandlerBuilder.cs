using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace HttpClientService.Tests.Mocks
{
    public class MockMessageHandlerBuilder : Mock<HttpMessageHandler>
    {
        private HttpStatusCode _code = HttpStatusCode.Accepted;
        private string _content = string.Empty;
        private string _reasonPhrase;

        public MockMessageHandlerBuilder()
        {
            this.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Content = new StringContent(JsonConvert.SerializeObject(""))
                })
                .Verifiable();
        }

        public MockMessageHandlerBuilder Where_SendAsync_returns_StatusCode(HttpStatusCode code)
        {
            _code = code;
            return this;
        }

        public MockMessageHandlerBuilder Where_SendAsync_returns_Content(string content)
        {
            _content = content;
            return this;
        }

        public MockMessageHandlerBuilder Where_SendAsync_returns_ReasonPhrase(string value)
        {
            _reasonPhrase = value;
            return this;
        }

        public MockMessageHandlerBuilder Build()
        {
            this.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    ReasonPhrase = _reasonPhrase,
                    StatusCode = _code,
                    Content = new StringContent(_content)
                })
                .Verifiable();

            return this;
        }

        public void VerifyMethod(HttpMethod method)
        {
            this.Protected()
                .Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(y => y.Method == method),
                    ItExpr.IsAny<CancellationToken>());
        }
        public void VerifyUri(string uri)
        {
            this.Protected()
                .Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(y => y.RequestUri == new Uri(uri)),
                    ItExpr.IsAny<CancellationToken>());
        }

        public void VerifyContent(string content)
        {
            this.Protected()
                .Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(y => y.Content.ReadAsStringAsync().Result.Contains(content)),
                    ItExpr.IsAny<CancellationToken>());
        }

        public void VerifyHeader(string key, string value)
        {
            this.Protected()
                .Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(y => y.Headers.Any(x => x.Key == key && x.Value.First() == value)),
                    ItExpr.IsAny<CancellationToken>());
        }


    }
}
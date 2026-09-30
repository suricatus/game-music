using System;
using System.Collections;
using Eco.Adapters.Http;

namespace Eco.Adapters.Tests
{
    public class FakeHttpClient : IHttpClient
    {
        private readonly HttpResult _result;
        public string LastUrl { get; private set; }
        public string LastBody { get; private set; }

        private FakeHttpClient(HttpResult result) => _result = result;

        public static FakeHttpClient ReturningSuccess(long statusCode, string body) =>
            new(new HttpResult(true, statusCode, body, null));

        public static FakeHttpClient ReturningFailure(long statusCode, string error) =>
            new(new HttpResult(false, statusCode, null, error));

        public IEnumerator Get(string url, Action<HttpResult> onComplete)
        {
            LastUrl = url;
            onComplete(_result);
            yield break;
        }

        public IEnumerator Post(string url, string jsonBody, Action<HttpResult> onComplete)
        {
            LastUrl = url;
            LastBody = jsonBody;
            onComplete(_result);
            yield break;
        }
    }
}

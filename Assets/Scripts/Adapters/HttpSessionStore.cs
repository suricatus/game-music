using System;
using System.Collections;
using Eco.Adapters.Http;
using Eco.Core.Sessions;
using Newtonsoft.Json;

namespace Eco.Adapters
{
    // Contract expected of the Session API:
    //   POST {baseUrl}/sessions          body: MatchSession JSON        -> 2xx on success
    //   GET  {baseUrl}/sessions/{token}                                 -> 200 + MatchSession JSON, or 404 if unknown
    public class HttpSessionStore : ISessionStore
    {
        private readonly string _baseUrl;
        private readonly IHttpClient _http;

        public HttpSessionStore(string baseUrl, IHttpClient http = null)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _http = http ?? new UnityHttpClient();
        }

        public IEnumerator Save(MatchSession session, Action onSaved, Action<string> onError)
        {
            var body = JsonConvert.SerializeObject(session);
            yield return _http.Post($"{_baseUrl}/sessions", body, result =>
            {
                if (result.Success) onSaved();
                else onError(result.Error ?? $"HTTP {result.StatusCode}");
            });
        }

        public IEnumerator TryLoad(string token, Action<MatchSession> onFound, Action onNotFound, Action<string> onError)
        {
            yield return _http.Get($"{_baseUrl}/sessions/{Uri.EscapeDataString(token)}", result =>
            {
                if (result.Success) onFound(JsonConvert.DeserializeObject<MatchSession>(result.Body));
                else if (result.StatusCode == 404) onNotFound();
                else onError(result.Error ?? $"HTTP {result.StatusCode}");
            });
        }
    }
}

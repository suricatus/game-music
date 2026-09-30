using System;
using System.Collections;
using System.Collections.Generic;
using Eco.Adapters.Http;
using Eco.Core.Sessions;
using Newtonsoft.Json;

namespace Eco.Adapters
{
    // Contract expected of the Lead API:
    //   POST {baseUrl}/leads   body: { "campaignId": "...", "leads": [ { ...fields }, ... ] }   -> 2xx on success
    public class HttpLeadUploader : ILeadUploader
    {
        private readonly string _baseUrl;
        private readonly IHttpClient _http;

        public HttpLeadUploader(string baseUrl, IHttpClient http = null)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _http = http ?? new UnityHttpClient();
        }

        public IEnumerator Upload(string campaignId, IReadOnlyList<IReadOnlyDictionary<string, string>> leads,
            Action onSuccess, Action<string> onError)
        {
            var body = JsonConvert.SerializeObject(new { campaignId, leads });
            yield return _http.Post($"{_baseUrl}/leads", body, result =>
            {
                if (result.Success) onSuccess();
                else onError(result.Error ?? $"HTTP {result.StatusCode}");
            });
        }
    }
}

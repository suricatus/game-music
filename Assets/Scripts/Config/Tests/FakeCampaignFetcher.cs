using System;
using System.Collections;

namespace Eco.Config.Tests
{
    public class FakeCampaignFetcher : ICampaignFetcher
    {
        private readonly string _json;
        private readonly string _error;

        private FakeCampaignFetcher(string json, string error)
        {
            _json = json;
            _error = error;
        }

        public static FakeCampaignFetcher ReturningSuccess(string json) => new(json, null);
        public static FakeCampaignFetcher ReturningError(string error) => new(null, error);

        public IEnumerator Fetch(string uri, int timeoutSeconds, Action<string> onSuccess, Action<string> onError)
        {
            if (_error != null) onError(_error);
            else onSuccess(_json);
            yield break;
        }
    }
}

using System;
using System.Collections;
using Eco.Config;

namespace Eco.Bootstrap.Tests
{
    public class FakeCampaignFetcher : ICampaignFetcher
    {
        private readonly string _error;

        private FakeCampaignFetcher(string error) => _error = error;

        public static FakeCampaignFetcher ReturningError(string error) => new(error);

        public IEnumerator Fetch(string uri, int timeoutSeconds, Action<string> onSuccess, Action<string> onError)
        {
            onError(_error);
            yield break;
        }
    }
}

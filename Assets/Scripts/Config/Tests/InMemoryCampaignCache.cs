using System.Collections.Generic;

namespace Eco.Config.Tests
{
    public class InMemoryCampaignCache : ICampaignCache
    {
        private readonly Dictionary<string, string> _store = new();

        public bool TryRead(string campaignId, out string json) => _store.TryGetValue(campaignId, out json);

        public void Write(string campaignId, string json) => _store[campaignId] = json;
    }
}

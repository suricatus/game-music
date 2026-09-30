using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Eco.Core.Sessions
{
    public class MatchSession
    {
        [JsonProperty("token")] public string Token;
        [JsonProperty("campaignId")] public string CampaignId;
        [JsonProperty("trackId")] public string TrackId;
        [JsonProperty("displayPercent")] public int DisplayPercent;
        [JsonProperty("lead")] public Dictionary<string, string> Lead;
        [JsonProperty("createdAtUtc")] public DateTime CreatedAtUtc;

        public MatchSession() { }

        public MatchSession(string token, string campaignId, string trackId, int displayPercent,
            IReadOnlyDictionary<string, string> lead, DateTime createdAtUtc)
        {
            Token = token;
            CampaignId = campaignId;
            TrackId = trackId;
            DisplayPercent = displayPercent;
            Lead = new Dictionary<string, string>(lead);
            CreatedAtUtc = createdAtUtc;
        }
    }
}

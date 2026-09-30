using System;
using System.Collections;
using System.IO;
using Eco.Core.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace Eco.Config
{
    public enum CampaignSource
    {
        Remote,
        Cache,
        Bundled
    }

    public readonly struct CampaignLoadResult
    {
        public readonly Campaign Campaign;
        public readonly CampaignSource Source;

        public CampaignLoadResult(Campaign campaign, CampaignSource source)
        {
            Campaign = campaign;
            Source = source;
        }
    }

    public class CampaignConfigService
    {
        public const int SupportedSchemaVersion = 1;

        private readonly string _campaignId;
        private readonly string _remoteUrl;
        private readonly ICampaignCache _cache;
        private readonly ICampaignFetcher _remoteFetcher;
        private readonly ICampaignFetcher _bundledFetcher;
        private readonly int _timeoutSeconds;

        public CampaignConfigService(string campaignId, string remoteUrl, ICampaignCache cache,
            ICampaignFetcher remoteFetcher = null, int timeoutSeconds = 5)
        {
            _campaignId = campaignId;
            _remoteUrl = remoteUrl;
            _cache = cache;
            _remoteFetcher = remoteFetcher ?? new UnityWebRequestCampaignFetcher();
            _bundledFetcher = new UnityWebRequestCampaignFetcher();
            _timeoutSeconds = timeoutSeconds;
        }

        public IEnumerator Load(Action<CampaignLoadResult> onLoaded, Action<string> onFailed)
        {
            string remoteError;

            if (string.IsNullOrEmpty(_remoteUrl))
            {
                remoteError = "No remote URL configured.";
            }
            else
            {
                remoteError = null;
                yield return _remoteFetcher.Fetch(_remoteUrl, _timeoutSeconds,
                    json =>
                    {
                        if (TryParse(json, out var campaign))
                        {
                            _cache.Write(_campaignId, json);
                            onLoaded(new CampaignLoadResult(campaign, CampaignSource.Remote));
                        }
                        else
                        {
                            remoteError = "Remote config failed validation.";
                        }
                    },
                    error => remoteError = error);
            }

            if (remoteError == null) yield break;

            if (_cache.TryRead(_campaignId, out var cachedJson) && TryParse(cachedJson, out var cachedCampaign))
            {
                onLoaded(new CampaignLoadResult(cachedCampaign, CampaignSource.Cache));
                yield break;
            }

            var bundledUri = ToRequestUri(Path.Combine(Application.streamingAssetsPath, "Campaigns", _campaignId + ".json"));
            string bundledError = null;
            yield return _bundledFetcher.Fetch(bundledUri, _timeoutSeconds,
                json =>
                {
                    if (TryParse(json, out var bundledCampaign))
                        onLoaded(new CampaignLoadResult(bundledCampaign, CampaignSource.Bundled));
                    else
                        bundledError = "Bundled config failed validation.";
                },
                error => bundledError = error);

            if (bundledError != null)
                onFailed($"Could not load campaign '{_campaignId}'. Remote: {remoteError}. No valid cache. Bundled: {bundledError}.");
        }

        private static bool TryParse(string json, out Campaign campaign)
        {
            try
            {
                campaign = JsonConvert.DeserializeObject<Campaign>(json);
                return campaign != null && campaign.SchemaVersion == SupportedSchemaVersion;
            }
            catch (JsonException)
            {
                campaign = null;
                return false;
            }
        }

        private static string ToRequestUri(string path) => path.Contains("://") ? path : "file://" + path;
    }
}

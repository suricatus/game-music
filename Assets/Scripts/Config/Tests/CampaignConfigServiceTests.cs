using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Eco.Config.Tests
{
    public class CampaignConfigServiceTests
    {
        private const string ValidJson =
            "{\"campaignId\":\"test\",\"schemaVersion\":1,\"axes\":[],\"questions\":[],\"tracks\":[]}";

        private const string MismatchedSchemaJson =
            "{\"campaignId\":\"test\",\"schemaVersion\":99,\"axes\":[],\"questions\":[],\"tracks\":[]}";

        private const string PlaceholderRemoteUrl = "https://example.invalid/campaign.json";

        [UnityTest]
        public IEnumerator Load_RemoteAvailable_UsesRemoteAndWritesCache()
        {
            var cache = new InMemoryCampaignCache();
            var service = new CampaignConfigService("test", PlaceholderRemoteUrl, cache,
                FakeCampaignFetcher.ReturningSuccess(ValidJson));

            CampaignLoadResult? result = null;
            yield return service.Load(r => result = r, error => Assert.Fail(error));

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(CampaignSource.Remote, result.Value.Source);
            Assert.IsTrue(cache.TryRead("test", out _));
        }

        [UnityTest]
        public IEnumerator Load_RemoteUnavailable_FallsBackToCache()
        {
            var cache = new InMemoryCampaignCache();
            cache.Write("test", ValidJson);
            var service = new CampaignConfigService("test", PlaceholderRemoteUrl, cache,
                FakeCampaignFetcher.ReturningError("simulated network failure"));

            CampaignLoadResult? result = null;
            yield return service.Load(r => result = r, error => Assert.Fail(error));

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(CampaignSource.Cache, result.Value.Source);
        }

        [UnityTest]
        public IEnumerator Load_RemoteAndCacheUnavailable_FallsBackToBundledStreamingAssets()
        {
            var cache = new InMemoryCampaignCache();
            var service = new CampaignConfigService("eco-bmth-2026", PlaceholderRemoteUrl, cache,
                FakeCampaignFetcher.ReturningError("simulated network failure"));

            CampaignLoadResult? result = null;
            yield return service.Load(r => result = r, error => Assert.Fail(error));

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(CampaignSource.Bundled, result.Value.Source);
            Assert.AreEqual(24, result.Value.Campaign.Tracks.Count);
        }

        [UnityTest]
        public IEnumerator Load_EverythingUnavailable_ReportsFailure()
        {
            var cache = new InMemoryCampaignCache();
            var service = new CampaignConfigService("no-such-campaign", PlaceholderRemoteUrl, cache,
                FakeCampaignFetcher.ReturningError("simulated network failure"));

            string failure = null;
            yield return service.Load(_ => Assert.Fail("expected failure, got a result"), error => failure = error);

            Assert.IsNotNull(failure);
        }

        [UnityTest]
        public IEnumerator Load_RemoteSchemaVersionMismatch_FallsBackToCache()
        {
            var cache = new InMemoryCampaignCache();
            cache.Write("test", ValidJson);
            var service = new CampaignConfigService("test", PlaceholderRemoteUrl, cache,
                FakeCampaignFetcher.ReturningSuccess(MismatchedSchemaJson));

            CampaignLoadResult? result = null;
            yield return service.Load(r => result = r, error => Assert.Fail(error));

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(CampaignSource.Cache, result.Value.Source);
        }

        [UnityTest]
        public IEnumerator Load_NoRemoteUrlConfigured_SkipsStraightToCache()
        {
            var cache = new InMemoryCampaignCache();
            cache.Write("test", ValidJson);
            var service = new CampaignConfigService("test", null, cache);

            CampaignLoadResult? result = null;
            yield return service.Load(r => result = r, error => Assert.Fail(error));

            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(CampaignSource.Cache, result.Value.Source);
        }
    }
}

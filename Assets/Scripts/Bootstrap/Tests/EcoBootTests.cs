using System.Collections;
using Eco.Adapters;
using Eco.Config;
using Eco.Core.Sessions;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Eco.Bootstrap.Tests
{
    public class EcoBootTests
    {
        private const string PlaceholderRemoteUrl = "https://example.invalid/campaign.json";

        private static CampaignConfigService ServiceFor(string campaignId) =>
            new(campaignId, PlaceholderRemoteUrl, new InMemoryCampaignCache(),
                FakeCampaignFetcher.ReturningError("simulated network failure"));

        [UnityTest]
        public IEnumerator Run_TotemSurface_BuildsTotemAdapterAndQuiz()
        {
            var boot = new EcoBoot(ServiceFor("eco-bmth-2026"), RuntimeSurface.Totem,
                _ => new InMemorySessionStore(), id => new FakeLeadQueue());

            EcoBootResult result = null;
            yield return boot.Run(r => result = r, error => Assert.Fail(error));

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Totem);
            Assert.IsNull(result.Web);
            Assert.AreEqual(1, result.Quiz.QuestionNumber);
            Assert.AreEqual(8, result.Quiz.TotalQuestions);
        }

        [UnityTest]
        public IEnumerator Run_TotemSurface_WithLeadUploaderFactory_BuildsLeadSync()
        {
            var boot = new EcoBoot(ServiceFor("eco-bmth-2026"), RuntimeSurface.Totem,
                _ => new InMemorySessionStore(), id => new FakeLeadQueue(),
                _ => new FakeLeadUploader());

            EcoBootResult result = null;
            yield return boot.Run(r => result = r, error => Assert.Fail(error));

            Assert.IsNotNull(result.LeadSync);
        }

        [UnityTest]
        public IEnumerator Run_TotemSurface_WithoutLeadUploaderFactory_LeavesLeadSyncNull()
        {
            var boot = new EcoBoot(ServiceFor("eco-bmth-2026"), RuntimeSurface.Totem,
                _ => new InMemorySessionStore(), id => new FakeLeadQueue());

            EcoBootResult result = null;
            yield return boot.Run(r => result = r, error => Assert.Fail(error));

            Assert.IsNull(result.LeadSync);
        }

        [UnityTest]
        public IEnumerator Run_WebSurface_NoSessionTokenInUrl_LeavesResolvedSessionNull()
        {
            var boot = new EcoBoot(ServiceFor("eco-bmth-2026"), RuntimeSurface.Web,
                _ => new InMemorySessionStore(), currentUrl: "https://eco.suricatus.games");

            EcoBootResult result = null;
            yield return boot.Run(r => result = r, error => Assert.Fail(error));

            Assert.IsNotNull(result.Web);
            Assert.IsNull(result.Totem);
            Assert.IsNull(result.ResolvedSession);
        }

        [UnityTest]
        public IEnumerator Run_WebSurface_UrlWithKnownToken_ResolvesSession()
        {
            var sessionStore = new InMemorySessionStore();
            var session = new MatchSession("AB12CD34", "eco-bmth-2026", "t22", 94,
                new System.Collections.Generic.Dictionary<string, string>(), System.DateTime.UtcNow);
            yield return sessionStore.Save(session, () => { }, Assert.Fail);

            var boot = new EcoBoot(ServiceFor("eco-bmth-2026"), RuntimeSurface.Web,
                _ => sessionStore, currentUrl: "https://eco.suricatus.games?match=AB12CD34");

            EcoBootResult result = null;
            yield return boot.Run(r => result = r, error => Assert.Fail(error));

            Assert.IsNotNull(result.ResolvedSession);
            Assert.AreEqual("t22", result.ResolvedSession.TrackId);
        }

        [UnityTest]
        public IEnumerator Run_WebSurface_UrlWithUnknownToken_LeavesResolvedSessionNull()
        {
            var boot = new EcoBoot(ServiceFor("eco-bmth-2026"), RuntimeSurface.Web,
                _ => new InMemorySessionStore(), currentUrl: "https://eco.suricatus.games?match=DOESNOTEXIST");

            EcoBootResult result = null;
            yield return boot.Run(r => result = r, error => Assert.Fail(error));

            Assert.IsNotNull(result.Web);
            Assert.IsNull(result.ResolvedSession);
        }

        [Test]
        public void Constructor_TotemSurfaceWithoutLeadQueueFactory_Throws()
        {
            Assert.Throws<System.ArgumentException>(() =>
                new EcoBoot(ServiceFor("eco-bmth-2026"), RuntimeSurface.Totem, _ => new InMemorySessionStore()));
        }

        [UnityTest]
        public IEnumerator Run_ConfigLoadFailsCompletely_InvokesOnFailed()
        {
            var boot = new EcoBoot(ServiceFor("no-such-campaign"), RuntimeSurface.Web, _ => new InMemorySessionStore());

            string failure = null;
            yield return boot.Run(_ => Assert.Fail("expected failure, got a result"), error => failure = error);

            Assert.IsNotNull(failure);
        }
    }
}

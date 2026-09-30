using System;
using System.Collections;
using Eco.Core.Sessions;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Eco.Adapters.Tests
{
    public class WebAdapterTests
    {
        [Test]
        public void TryReadSessionToken_UrlWithMatchParam_ReturnsToken()
        {
            var found = WebAdapter.TryReadSessionToken("https://eco.suricatus.games?match=AB12CD34", out var token);

            Assert.IsTrue(found);
            Assert.AreEqual("AB12CD34", token);
        }

        [Test]
        public void TryReadSessionToken_UrlWithOtherParamsToo_StillFindsMatch()
        {
            var found = WebAdapter.TryReadSessionToken("https://eco.suricatus.games?utm_source=story&match=AB12CD34", out var token);

            Assert.IsTrue(found);
            Assert.AreEqual("AB12CD34", token);
        }

        [Test]
        public void TryReadSessionToken_NoQueryString_ReturnsFalse()
        {
            var found = WebAdapter.TryReadSessionToken("https://eco.suricatus.games", out var token);

            Assert.IsFalse(found);
            Assert.IsNull(token);
        }

        [Test]
        public void TryReadSessionToken_NoMatchParam_ReturnsFalse()
        {
            var found = WebAdapter.TryReadSessionToken("https://eco.suricatus.games?utm_source=story", out _);

            Assert.IsFalse(found);
        }

        [UnityTest]
        public IEnumerator TryResolveSession_KnownToken_ReturnsSession()
        {
            var store = new InMemorySessionStore();
            var session = new MatchSession("AB12CD34", "eco-bmth-2026", "t22", 94,
                new System.Collections.Generic.Dictionary<string, string>(), DateTime.UtcNow);
            yield return store.Save(session, () => { }, Assert.Fail);
            var adapter = new WebAdapter(store);

            MatchSession loaded = null;
            var notFoundCalled = false;
            yield return adapter.TryResolveSession("AB12CD34", s => loaded = s, () => notFoundCalled = true, Assert.Fail);

            Assert.IsFalse(notFoundCalled);
            Assert.IsNotNull(loaded);
            Assert.AreEqual("t22", loaded.TrackId);
        }

        [UnityTest]
        public IEnumerator TryResolveSession_UnknownToken_CallsOnNotFound()
        {
            var adapter = new WebAdapter(new InMemorySessionStore());

            var notFoundCalled = false;
            yield return adapter.TryResolveSession("does-not-exist", _ => Assert.Fail("expected not-found"),
                () => notFoundCalled = true, Assert.Fail);

            Assert.IsTrue(notFoundCalled);
        }
    }
}

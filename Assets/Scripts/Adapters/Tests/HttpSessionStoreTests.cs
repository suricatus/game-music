using System.Collections;
using System.Collections.Generic;
using Eco.Core.Sessions;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Eco.Adapters.Tests
{
    public class HttpSessionStoreTests
    {
        [UnityTest]
        public IEnumerator Save_SuccessResponse_CallsOnSaved()
        {
            var http = FakeHttpClient.ReturningSuccess(200, "");
            var store = new HttpSessionStore("https://api.eco.example/v1", http);
            var session = new MatchSession("AB12CD34", "eco-bmth-2026", "t22", 94,
                new Dictionary<string, string>(), System.DateTime.UtcNow);

            var saved = false;
            yield return store.Save(session, () => saved = true, Assert.Fail);

            Assert.IsTrue(saved);
            StringAssert.EndsWith("/sessions", http.LastUrl);
            StringAssert.Contains("AB12CD34", http.LastBody);
        }

        [UnityTest]
        public IEnumerator Save_ServerError_CallsOnError()
        {
            var http = FakeHttpClient.ReturningFailure(500, "server exploded");
            var store = new HttpSessionStore("https://api.eco.example/v1", http);
            var session = new MatchSession("AB12CD34", "eco-bmth-2026", "t22", 94,
                new Dictionary<string, string>(), System.DateTime.UtcNow);

            string error = null;
            yield return store.Save(session, () => Assert.Fail("expected failure"), e => error = e);

            Assert.AreEqual("server exploded", error);
        }

        [UnityTest]
        public IEnumerator TryLoad_Found_CallsOnFoundWithDeserializedSession()
        {
            const string body = "{\"token\":\"AB12CD34\",\"campaignId\":\"eco-bmth-2026\",\"trackId\":\"t22\",\"displayPercent\":94,\"lead\":{}}";
            var http = FakeHttpClient.ReturningSuccess(200, body);
            var store = new HttpSessionStore("https://api.eco.example/v1", http);

            MatchSession found = null;
            yield return store.TryLoad("AB12CD34", s => found = s, () => Assert.Fail("expected found"), Assert.Fail);

            Assert.IsNotNull(found);
            Assert.AreEqual("t22", found.TrackId);
            StringAssert.EndsWith("/sessions/AB12CD34", http.LastUrl);
        }

        [UnityTest]
        public IEnumerator TryLoad_404_CallsOnNotFound_NotOnError()
        {
            var http = FakeHttpClient.ReturningFailure(404, "not found");
            var store = new HttpSessionStore("https://api.eco.example/v1", http);

            var notFoundCalled = false;
            yield return store.TryLoad("does-not-exist", _ => Assert.Fail("expected not-found"),
                () => notFoundCalled = true, Assert.Fail);

            Assert.IsTrue(notFoundCalled);
        }

        [UnityTest]
        public IEnumerator TryLoad_OtherServerError_CallsOnError()
        {
            var http = FakeHttpClient.ReturningFailure(500, "server exploded");
            var store = new HttpSessionStore("https://api.eco.example/v1", http);

            string error = null;
            yield return store.TryLoad("AB12CD34", _ => Assert.Fail("expected error"), () => Assert.Fail("expected error"),
                e => error = e);

            Assert.AreEqual("server exploded", error);
        }
    }
}

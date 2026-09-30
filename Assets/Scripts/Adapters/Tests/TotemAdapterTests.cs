using System;
using System.Collections;
using System.Collections.Generic;
using Eco.Core.Matching;
using Eco.Core.Models;
using Eco.Core.Sessions;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Eco.Adapters.Tests
{
    public class TotemAdapterTests
    {
        private static LeadCaptureConfig LeadConfig => new()
        {
            Fields = new List<string> { "nome", "email" },
            Required = new List<string> { "nome", "email" }
        };

        private static TotemAdapter BuildAdapter(FakeLeadQueue leadQueue, InMemorySessionStore sessionStore) =>
            new(LeadConfig, leadQueue, sessionStore, "https://eco.suricatus.games", new Random(1));

        [Test]
        public void CaptureLead_ValidFields_QueuesAndReturnsOk()
        {
            var leadQueue = new FakeLeadQueue();
            var adapter = BuildAdapter(leadQueue, new InMemorySessionStore());

            var result = adapter.CaptureLead(new Dictionary<string, string> { { "nome", "Amanda" }, { "email", "a@b.com" } });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, leadQueue.Enqueued.Count);
        }

        [Test]
        public void CaptureLead_MissingRequiredField_DoesNotQueue()
        {
            var leadQueue = new FakeLeadQueue();
            var adapter = BuildAdapter(leadQueue, new InMemorySessionStore());

            var result = adapter.CaptureLead(new Dictionary<string, string> { { "nome", "Amanda" } });

            Assert.IsFalse(result.Success);
            CollectionAssert.Contains(result.MissingFields, "email");
            Assert.IsEmpty(leadQueue.Enqueued);
        }

        [UnityTest]
        public IEnumerator BeginSession_SavesSessionAndReturnsUrlWithToken()
        {
            var sessionStore = new InMemorySessionStore();
            var adapter = BuildAdapter(new FakeLeadQueue(), sessionStore);
            var track = new TrackDef { Id = "t22", Title = "Drown" };
            var topMatch = new TrackMatch(track, 0.95, 94);
            var lead = new Dictionary<string, string> { { "nome", "Amanda" }, { "email", "a@b.com" } };

            string url = null;
            yield return adapter.BeginSession("eco-bmth-2026", topMatch, lead,
                u => url = u, error => Assert.Fail(error));

            StringAssert.StartsWith("https://eco.suricatus.games?match=", url);
            var token = url.Split('=')[1];

            MatchSession session = null;
            yield return sessionStore.TryLoad(token, s => session = s, () => Assert.Fail("session not found"), Assert.Fail);

            Assert.AreEqual("t22", session.TrackId);
            Assert.AreEqual(94, session.DisplayPercent);
            Assert.AreEqual("Amanda", session.Lead["nome"]);
        }
    }
}

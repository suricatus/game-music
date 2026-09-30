using System;
using System.Collections;
using System.Collections.Generic;
using Eco.Core.Sessions;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Eco.Core.Tests
{
    public class FakeLeadQueue : ILeadQueue
    {
        private readonly List<IReadOnlyDictionary<string, string>> _items = new();
        public bool WasCleared;

        public void Enqueue(IReadOnlyDictionary<string, string> fields) => _items.Add(fields);
        public IReadOnlyList<IReadOnlyDictionary<string, string>> PeekAll() => new List<IReadOnlyDictionary<string, string>>(_items);

        public void Clear()
        {
            WasCleared = true;
            _items.Clear();
        }
    }

    public class FakeLeadUploader : ILeadUploader
    {
        private readonly string _error;
        public IReadOnlyList<IReadOnlyDictionary<string, string>> ReceivedLeads;

        private FakeLeadUploader(string error) => _error = error;

        public static FakeLeadUploader Succeeding() => new(null);
        public static FakeLeadUploader Failing(string error) => new(error);

        public IEnumerator Upload(string campaignId, IReadOnlyList<IReadOnlyDictionary<string, string>> leads,
            Action onSuccess, Action<string> onError)
        {
            ReceivedLeads = leads;
            if (_error != null) onError(_error);
            else onSuccess();
            yield break;
        }
    }

    public class LeadSyncServiceTests
    {
        [UnityTest]
        public IEnumerator SyncPending_NoLeadsQueued_ReportsZeroWithoutCallingUploader()
        {
            var queue = new FakeLeadQueue();
            var uploader = FakeLeadUploader.Succeeding();
            var service = new LeadSyncService("eco-bmth-2026", queue, uploader);

            int? synced = null;
            yield return service.SyncPending(count => synced = count, error => Assert.Fail(error));

            Assert.AreEqual(0, synced);
            Assert.IsNull(uploader.ReceivedLeads);
        }

        [UnityTest]
        public IEnumerator SyncPending_UploadSucceeds_ClearsQueue()
        {
            var queue = new FakeLeadQueue();
            queue.Enqueue(new Dictionary<string, string> { { "nome", "Amanda" } });
            var service = new LeadSyncService("eco-bmth-2026", queue, FakeLeadUploader.Succeeding());

            int? synced = null;
            yield return service.SyncPending(count => synced = count, error => Assert.Fail(error));

            Assert.AreEqual(1, synced);
            Assert.IsTrue(queue.WasCleared);
            Assert.IsEmpty(queue.PeekAll());
        }

        [UnityTest]
        public IEnumerator SyncPending_UploadFails_LeavesLeadsQueued()
        {
            var queue = new FakeLeadQueue();
            queue.Enqueue(new Dictionary<string, string> { { "nome", "Amanda" } });
            var service = new LeadSyncService("eco-bmth-2026", queue, FakeLeadUploader.Failing("network down"));

            string failure = null;
            yield return service.SyncPending(_ => Assert.Fail("expected failure"), error => failure = error);

            Assert.AreEqual("network down", failure);
            Assert.IsFalse(queue.WasCleared);
            Assert.AreEqual(1, queue.PeekAll().Count);
        }
    }
}

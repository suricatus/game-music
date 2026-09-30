using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Eco.Adapters.Tests
{
    public class FileLeadQueueTests
    {
        private string _tempRoot;

        [SetUp]
        public void SetUp()
        {
            _tempRoot = Path.Combine(Path.GetTempPath(), $"eco-leadqueue-test-{Guid.NewGuid():N}");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, recursive: true);
        }

        [Test]
        public void PeekAll_NoLeadsEnqueued_ReturnsEmpty()
        {
            var queue = new FileLeadQueue(_tempRoot, "eco-bmth-2026");

            Assert.IsEmpty(queue.PeekAll());
        }

        [Test]
        public void Enqueue_ThenPeekAll_ReturnsLeadsInOrderWithoutClearing()
        {
            var queue = new FileLeadQueue(_tempRoot, "eco-bmth-2026");
            queue.Enqueue(new Dictionary<string, string> { { "nome", "Amanda" }, { "email", "a@b.com" } });
            queue.Enqueue(new Dictionary<string, string> { { "nome", "Renan" }, { "email", "r@b.com" } });

            var leads = queue.PeekAll();

            Assert.AreEqual(2, leads.Count);
            Assert.AreEqual("Amanda", leads[0]["nome"]);
            Assert.AreEqual("Renan", leads[1]["nome"]);
            Assert.AreEqual(2, queue.PeekAll().Count, "PeekAll should not consume the queue");
        }

        [Test]
        public void Clear_RemovesEverythingPreviouslyEnqueued()
        {
            var queue = new FileLeadQueue(_tempRoot, "eco-bmth-2026");
            queue.Enqueue(new Dictionary<string, string> { { "nome", "Amanda" } });

            queue.Clear();

            Assert.IsEmpty(queue.PeekAll());
        }

        [Test]
        public void Enqueue_SurvivesAcrossQueueInstances()
        {
            new FileLeadQueue(_tempRoot, "eco-bmth-2026")
                .Enqueue(new Dictionary<string, string> { { "nome", "Amanda" } });

            var reopened = new FileLeadQueue(_tempRoot, "eco-bmth-2026");

            Assert.AreEqual(1, reopened.PeekAll().Count);
        }
    }
}

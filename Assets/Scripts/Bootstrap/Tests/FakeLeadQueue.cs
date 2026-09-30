using System.Collections.Generic;
using Eco.Core.Sessions;

namespace Eco.Bootstrap.Tests
{
    public class FakeLeadQueue : ILeadQueue
    {
        private readonly List<IReadOnlyDictionary<string, string>> _enqueued = new();

        public void Enqueue(IReadOnlyDictionary<string, string> fields) => _enqueued.Add(fields);

        public IReadOnlyList<IReadOnlyDictionary<string, string>> PeekAll() =>
            new List<IReadOnlyDictionary<string, string>>(_enqueued);

        public void Clear() => _enqueued.Clear();
    }
}

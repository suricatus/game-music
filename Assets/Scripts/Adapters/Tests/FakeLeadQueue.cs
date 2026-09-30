using System.Collections.Generic;
using Eco.Core.Sessions;

namespace Eco.Adapters.Tests
{
    public class FakeLeadQueue : ILeadQueue
    {
        public readonly List<IReadOnlyDictionary<string, string>> Enqueued = new();

        public void Enqueue(IReadOnlyDictionary<string, string> fields) => Enqueued.Add(fields);

        public IReadOnlyList<IReadOnlyDictionary<string, string>> PeekAll() =>
            new List<IReadOnlyDictionary<string, string>>(Enqueued);

        public void Clear() => Enqueued.Clear();
    }
}

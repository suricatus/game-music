using System.Collections.Generic;

namespace Eco.Core.Sessions
{
    public interface ILeadQueue
    {
        void Enqueue(IReadOnlyDictionary<string, string> fields);
        IReadOnlyList<IReadOnlyDictionary<string, string>> PeekAll();
        void Clear();
    }
}

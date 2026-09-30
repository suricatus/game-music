using System;
using System.Collections;

namespace Eco.Core.Sessions
{
    public interface ISessionStore
    {
        IEnumerator Save(MatchSession session, Action onSaved, Action<string> onError);
        IEnumerator TryLoad(string token, Action<MatchSession> onFound, Action onNotFound, Action<string> onError);
    }
}

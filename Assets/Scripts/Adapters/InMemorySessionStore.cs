using System;
using System.Collections;
using System.Collections.Generic;
using Eco.Core.Sessions;

namespace Eco.Adapters
{
    // Local stand-in for a real Session API. Totem and web run as separate
    // processes/devices, so this only works within a single process — swap
    // for HttpSessionStore once a real Session API exists.
    public class InMemorySessionStore : ISessionStore
    {
        private readonly Dictionary<string, MatchSession> _sessions = new();

        public IEnumerator Save(MatchSession session, Action onSaved, Action<string> onError)
        {
            _sessions[session.Token] = session;
            onSaved();
            yield break;
        }

        public IEnumerator TryLoad(string token, Action<MatchSession> onFound, Action onNotFound, Action<string> onError)
        {
            if (_sessions.TryGetValue(token, out var session)) onFound(session);
            else onNotFound();
            yield break;
        }
    }
}

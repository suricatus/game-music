using System;
using System.Collections;
using Eco.Core.Sessions;

namespace Eco.Adapters
{
    public class WebAdapter
    {
        private readonly ISessionStore _sessionStore;

        public WebAdapter(ISessionStore sessionStore)
        {
            _sessionStore = sessionStore;
        }

        public static bool TryReadSessionToken(string url, out string token)
        {
            token = null;
            if (string.IsNullOrEmpty(url)) return false;

            var queryIndex = url.IndexOf('?');
            if (queryIndex < 0) return false;

            foreach (var pair in url[(queryIndex + 1)..].Split('&'))
            {
                var kv = pair.Split('=', 2);
                if (kv.Length != 2 || kv[0] != "match") continue;

                token = Uri.UnescapeDataString(kv[1]);
                return true;
            }

            return false;
        }

        public IEnumerator TryResolveSession(string token, Action<MatchSession> onFound, Action onNotFound, Action<string> onError) =>
            _sessionStore.TryLoad(token, onFound, onNotFound, onError);
    }
}

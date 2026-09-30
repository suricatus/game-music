using System;
using System.Collections;
using System.Collections.Generic;
using Eco.Core.Matching;
using Eco.Core.Models;
using Eco.Core.Sessions;

namespace Eco.Adapters
{
    public class TotemAdapter
    {
        private readonly LeadCaptureConfig _leadCaptureConfig;
        private readonly ILeadQueue _leadQueue;
        private readonly ISessionStore _sessionStore;
        private readonly string _webBaseUrl;
        private readonly Random _random;

        public TotemAdapter(LeadCaptureConfig leadCaptureConfig, ILeadQueue leadQueue, ISessionStore sessionStore,
            string webBaseUrl, Random random = null)
        {
            _leadCaptureConfig = leadCaptureConfig;
            _leadQueue = leadQueue;
            _sessionStore = sessionStore;
            _webBaseUrl = webBaseUrl;
            _random = random;
        }

        public LeadCaptureResult CaptureLead(IReadOnlyDictionary<string, string> fields)
        {
            if (!LeadCaptureValidator.Validate(_leadCaptureConfig, fields, out var missing))
                return LeadCaptureResult.Invalid(missing);

            _leadQueue.Enqueue(fields);
            return LeadCaptureResult.Ok();
        }

        public IEnumerator BeginSession(string campaignId, TrackMatch topMatch, IReadOnlyDictionary<string, string> lead,
            Action<string> onUrlReady, Action<string> onError)
        {
            var token = SessionTokenGenerator.Generate(random: _random);
            var session = new MatchSession(token, campaignId, topMatch.Track.Id, topMatch.DisplayPercent, lead, DateTime.UtcNow);

            yield return _sessionStore.Save(session,
                () => onUrlReady($"{_webBaseUrl}?match={token}"),
                onError);
        }

        public KioskIdleWatcher CreateIdleWatcher(double idleTimeoutSeconds) => new(idleTimeoutSeconds);
    }
}

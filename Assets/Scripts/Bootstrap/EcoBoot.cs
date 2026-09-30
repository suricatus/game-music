using System;
using System.Collections;
using Eco.Adapters;
using Eco.Config;
using Eco.Core.Models;
using Eco.Core.Quiz;
using Eco.Core.Sessions;

namespace Eco.Bootstrap
{
    public enum RuntimeSurface
    {
        Totem,
        Web
    }

    public class EcoBootResult
    {
        public Campaign Campaign;
        public CampaignSource Source;
        public QuizEngine Quiz;
        public TotemAdapter Totem;
        public WebAdapter Web;
        public MatchSession ResolvedSession;
        public LeadSyncService LeadSync;
    }

    public class EcoBoot
    {
        private readonly CampaignConfigService _configService;
        private readonly RuntimeSurface _surface;
        private readonly Func<string, ISessionStore> _sessionStoreFactory;
        private readonly Func<string, ILeadQueue> _leadQueueFactory;
        private readonly Func<string, ILeadUploader> _leadUploaderFactory;
        private readonly string _currentUrl;

        public EcoBoot(CampaignConfigService configService, RuntimeSurface surface,
            Func<string, ISessionStore> sessionStoreFactory,
            Func<string, ILeadQueue> leadQueueFactory = null,
            Func<string, ILeadUploader> leadUploaderFactory = null,
            string currentUrl = null)
        {
            if (surface == RuntimeSurface.Totem && leadQueueFactory == null)
                throw new ArgumentException("A totem surface needs a lead queue factory.", nameof(leadQueueFactory));

            _configService = configService;
            _surface = surface;
            _sessionStoreFactory = sessionStoreFactory;
            _leadQueueFactory = leadQueueFactory;
            _leadUploaderFactory = leadUploaderFactory;
            _currentUrl = currentUrl;
        }

        public IEnumerator Run(Action<EcoBootResult> onReady, Action<string> onFailed)
        {
            EcoBootResult result = null;
            string loadError = null;

            yield return _configService.Load(
                loadResult =>
                {
                    var campaign = loadResult.Campaign;
                    result = new EcoBootResult
                    {
                        Campaign = campaign,
                        Source = loadResult.Source,
                        Quiz = new QuizEngine(campaign)
                    };

                    var sessionStore = _sessionStoreFactory(campaign.SessionApiUrl);

                    if (_surface == RuntimeSurface.Totem)
                    {
                        var leadQueue = _leadQueueFactory(campaign.CampaignId);
                        result.Totem = new TotemAdapter(campaign.LeadCapture, leadQueue, sessionStore, campaign.WebBaseUrl);

                        var uploader = _leadUploaderFactory?.Invoke(campaign.LeadApiUrl);
                        if (uploader != null)
                            result.LeadSync = new LeadSyncService(campaign.CampaignId, leadQueue, uploader);
                    }
                    else
                    {
                        result.Web = new WebAdapter(sessionStore);
                    }
                },
                error => loadError = error);

            if (loadError != null)
            {
                onFailed(loadError);
                yield break;
            }

            if (result.Web != null && WebAdapter.TryReadSessionToken(_currentUrl, out var token))
            {
                yield return result.Web.TryResolveSession(token,
                    found => result.ResolvedSession = found,
                    () => { },
                    error => { });
            }

            onReady(result);
        }
    }
}

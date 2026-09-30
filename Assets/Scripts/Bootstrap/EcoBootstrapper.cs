using System;
using Eco.Adapters;
using Eco.Config;
using Eco.Core.Sessions;
using UnityEngine;

namespace Eco.Bootstrap
{
    public class EcoBootstrapper : MonoBehaviour
    {
        [SerializeField] private string campaignId = "eco-bmth-2026";
        [SerializeField] private string remoteUrl;
        [SerializeField] private float kioskIdleTimeoutSeconds = 60f;

        public event Action<EcoBootResult> OnReady;
        public event Action<string> OnFailed;
        public event Action OnKioskIdleTimeout;
        public event Action<int> OnLeadsSynced;

        private KioskIdleWatcher _idleWatcher;

        private void Start()
        {
            var surface = Application.platform == RuntimePlatform.WebGLPlayer ? RuntimeSurface.Web : RuntimeSurface.Totem;
            var configService = new CampaignConfigService(campaignId, remoteUrl, new PersistentDataCampaignCache());

            var boot = new EcoBoot(configService, surface,
                sessionApiUrl => string.IsNullOrEmpty(sessionApiUrl)
                    ? new InMemorySessionStore()
                    : new HttpSessionStore(sessionApiUrl),
                id => FileLeadQueue.ForCampaign(id),
                leadApiUrl => string.IsNullOrEmpty(leadApiUrl) ? null : new HttpLeadUploader(leadApiUrl),
                Application.absoluteURL);

            StartCoroutine(boot.Run(HandleReady, error => OnFailed?.Invoke(error)));
        }

        private void HandleReady(EcoBootResult result)
        {
            if (result.Totem != null)
            {
                _idleWatcher = result.Totem.CreateIdleWatcher(kioskIdleTimeoutSeconds);
                _idleWatcher.OnIdleTimeout += () => OnKioskIdleTimeout?.Invoke();
            }

            if (result.LeadSync != null)
                StartCoroutine(result.LeadSync.SyncPending(
                    count => OnLeadsSynced?.Invoke(count),
                    _ => { }));

            OnReady?.Invoke(result);
        }

        private void Update()
        {
            _idleWatcher?.Tick(Time.unscaledDeltaTime);
        }

        public void NotifyUserInteraction() => _idleWatcher?.Reset();
    }
}

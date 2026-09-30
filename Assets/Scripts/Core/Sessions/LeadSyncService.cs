using System;
using System.Collections;
using System.Collections.Generic;

namespace Eco.Core.Sessions
{
    public interface ILeadUploader
    {
        IEnumerator Upload(string campaignId, IReadOnlyList<IReadOnlyDictionary<string, string>> leads,
            Action onSuccess, Action<string> onError);
    }

    public class LeadSyncService
    {
        private readonly string _campaignId;
        private readonly ILeadQueue _queue;
        private readonly ILeadUploader _uploader;

        public LeadSyncService(string campaignId, ILeadQueue queue, ILeadUploader uploader)
        {
            _campaignId = campaignId;
            _queue = queue;
            _uploader = uploader;
        }

        public IEnumerator SyncPending(Action<int> onSynced, Action<string> onError)
        {
            var pending = _queue.PeekAll();
            if (pending.Count == 0)
            {
                onSynced(0);
                yield break;
            }

            yield return _uploader.Upload(_campaignId, pending,
                () =>
                {
                    _queue.Clear();
                    onSynced(pending.Count);
                },
                onError);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Eco.Core.Sessions;

namespace Eco.Bootstrap.Tests
{
    public class FakeLeadUploader : ILeadUploader
    {
        public IEnumerator Upload(string campaignId, IReadOnlyList<IReadOnlyDictionary<string, string>> leads,
            Action onSuccess, Action<string> onError)
        {
            onSuccess();
            yield break;
        }
    }
}

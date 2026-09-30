using System;
using System.Collections;
using UnityEngine.Networking;

namespace Eco.Config
{
    public interface ICampaignFetcher
    {
        IEnumerator Fetch(string uri, int timeoutSeconds, Action<string> onSuccess, Action<string> onError);
    }

    public class UnityWebRequestCampaignFetcher : ICampaignFetcher
    {
        public IEnumerator Fetch(string uri, int timeoutSeconds, Action<string> onSuccess, Action<string> onError)
        {
            using var request = UnityWebRequest.Get(uri);
            request.timeout = timeoutSeconds;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                onError(request.error);
            else
                onSuccess(request.downloadHandler.text);
        }
    }
}

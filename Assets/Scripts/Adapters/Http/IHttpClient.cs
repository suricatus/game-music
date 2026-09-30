using System;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

namespace Eco.Adapters.Http
{
    public readonly struct HttpResult
    {
        public readonly bool Success;
        public readonly long StatusCode;
        public readonly string Body;
        public readonly string Error;

        public HttpResult(bool success, long statusCode, string body, string error)
        {
            Success = success;
            StatusCode = statusCode;
            Body = body;
            Error = error;
        }
    }

    public interface IHttpClient
    {
        IEnumerator Get(string url, Action<HttpResult> onComplete);
        IEnumerator Post(string url, string jsonBody, Action<HttpResult> onComplete);
    }

    public class UnityHttpClient : IHttpClient
    {
        public IEnumerator Get(string url, Action<HttpResult> onComplete)
        {
            using var request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            onComplete(ToResult(request));
        }

        public IEnumerator Post(string url, string jsonBody, Action<HttpResult> onComplete)
        {
            using var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            onComplete(ToResult(request));
        }

        private static HttpResult ToResult(UnityWebRequest request)
        {
            var success = request.result == UnityWebRequest.Result.Success;
            return new HttpResult(success, request.responseCode,
                success ? request.downloadHandler.text : null,
                success ? null : request.error);
        }
    }
}

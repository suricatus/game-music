using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Eco.Adapters.Tests
{
    public class HttpLeadUploaderTests
    {
        [UnityTest]
        public IEnumerator Upload_SuccessResponse_CallsOnSuccess()
        {
            var http = FakeHttpClient.ReturningSuccess(200, "");
            var uploader = new HttpLeadUploader("https://api.eco.example/v1", http);
            var leads = new List<IReadOnlyDictionary<string, string>>
            {
                new Dictionary<string, string> { { "nome", "Amanda" } }
            };

            var succeeded = false;
            yield return uploader.Upload("eco-bmth-2026", leads, () => succeeded = true, Assert.Fail);

            Assert.IsTrue(succeeded);
            StringAssert.EndsWith("/leads", http.LastUrl);
            StringAssert.Contains("eco-bmth-2026", http.LastBody);
            StringAssert.Contains("Amanda", http.LastBody);
        }

        [UnityTest]
        public IEnumerator Upload_ServerError_CallsOnError()
        {
            var http = FakeHttpClient.ReturningFailure(500, "server exploded");
            var uploader = new HttpLeadUploader("https://api.eco.example/v1", http);
            var leads = new List<IReadOnlyDictionary<string, string>>
            {
                new Dictionary<string, string> { { "nome", "Amanda" } }
            };

            string error = null;
            yield return uploader.Upload("eco-bmth-2026", leads, () => Assert.Fail("expected failure"), e => error = e);

            Assert.AreEqual("server exploded", error);
        }
    }
}

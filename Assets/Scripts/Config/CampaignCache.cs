using System.IO;
using UnityEngine;

namespace Eco.Config
{
    public interface ICampaignCache
    {
        bool TryRead(string campaignId, out string json);
        void Write(string campaignId, string json);
    }

    public class PersistentDataCampaignCache : ICampaignCache
    {
        public bool TryRead(string campaignId, out string json)
        {
            var path = PathFor(campaignId);
            if (!File.Exists(path))
            {
                json = null;
                return false;
            }

            json = File.ReadAllText(path);
            return true;
        }

        public void Write(string campaignId, string json)
        {
            var path = PathFor(campaignId);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, json);
        }

        private static string PathFor(string campaignId) =>
            Path.Combine(Application.persistentDataPath, "campaigns", campaignId + ".json");
    }
}

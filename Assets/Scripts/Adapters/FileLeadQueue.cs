using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eco.Core.Sessions;
using Newtonsoft.Json;
using UnityEngine;

namespace Eco.Adapters
{
    public class FileLeadQueue : ILeadQueue
    {
        private readonly string _path;

        public FileLeadQueue(string rootDirectory, string campaignId)
        {
            _path = Path.Combine(rootDirectory, "leads", campaignId + ".jsonl");
        }

        public static FileLeadQueue ForCampaign(string campaignId) =>
            new(Application.persistentDataPath, campaignId);

        public void Enqueue(IReadOnlyDictionary<string, string> fields)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            File.AppendAllText(_path, JsonConvert.SerializeObject(fields) + Environment.NewLine);
        }

        public IReadOnlyList<IReadOnlyDictionary<string, string>> PeekAll()
        {
            if (!File.Exists(_path)) return Array.Empty<IReadOnlyDictionary<string, string>>();

            return File.ReadAllLines(_path)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => (IReadOnlyDictionary<string, string>)JsonConvert.DeserializeObject<Dictionary<string, string>>(line))
                .ToList();
        }

        public void Clear()
        {
            if (File.Exists(_path)) File.Delete(_path);
        }
    }
}

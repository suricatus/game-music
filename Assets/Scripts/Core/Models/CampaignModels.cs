using System.Collections.Generic;
using Newtonsoft.Json;

namespace Eco.Core.Models
{
    public class Campaign
    {
        [JsonProperty("campaignId")] public string CampaignId;
        [JsonProperty("status")] public string Status;
        [JsonProperty("schemaVersion")] public int SchemaVersion;
        [JsonProperty("meta")] public CampaignMeta Meta;
        [JsonProperty("theme")] public ThemeSkin Theme;
        [JsonProperty("axes")] public List<AxisDef> Axes;
        [JsonProperty("scoring")] public ScoringConfig Scoring;
        [JsonProperty("questions")] public List<QuestionDef> Questions;
        [JsonProperty("tracks")] public List<TrackDef> Tracks;
        [JsonProperty("leadCapture")] public LeadCaptureConfig LeadCapture;
        [JsonProperty("webBaseUrl")] public string WebBaseUrl;
        [JsonProperty("sessionApiUrl")] public string SessionApiUrl;
        [JsonProperty("leadApiUrl")] public string LeadApiUrl;
    }

    public class CampaignMeta
    {
        [JsonProperty("productName")] public string ProductName;
        [JsonProperty("tagline")] public string Tagline;
        [JsonProperty("artist")] public string Artist;
        [JsonProperty("albums")] public List<string> Albums;
    }

    public class ThemeSkin
    {
        [JsonProperty("colors")] public Dictionary<string, string> Colors;
        [JsonProperty("fonts")] public Dictionary<string, string> Fonts;
        [JsonProperty("logoRef")] public string LogoRef;
        [JsonProperty("characterRef")] public string CharacterRef;
    }

    public class AxisDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("label")] public string Label;
        [JsonProperty("icon")] public string Icon;
        [JsonProperty("description")] public string Description;
    }

    public class ScoringConfig
    {
        [JsonProperty("weights")] public Dictionary<string, double> Weights;
        [JsonProperty("axisFloor")] public int AxisFloor;
        [JsonProperty("minDisplayedMatch")] public int MinDisplayedMatch;
        [JsonProperty("algorithm")] public string Algorithm;
    }

    public class QuestionDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("tag")] public string Tag;
        [JsonProperty("prompt")] public string Prompt;
        [JsonProperty("sourceNote")] public string SourceNote;
        [JsonProperty("options")] public List<OptionDef> Options;
    }

    public class OptionDef
    {
        [JsonProperty("label")] public string Label;
        [JsonProperty("deltas")] public Dictionary<string, int> Deltas;
    }

    public class TrackDef
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("title")] public string Title;
        [JsonProperty("album")] public string Album;
        [JsonProperty("fingerprint")] public Dictionary<string, int> Fingerprint;
        [JsonProperty("resultPhrase")] public string ResultPhrase;
        [JsonProperty("coverRef")] public string CoverRef;
    }

    public class LeadCaptureConfig
    {
        [JsonProperty("fields")] public List<string> Fields;
        [JsonProperty("required")] public List<string> Required;
    }
}

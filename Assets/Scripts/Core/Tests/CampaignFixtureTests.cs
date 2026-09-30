using System.IO;
using System.Linq;
using Eco.Core.Matching;
using Eco.Core.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace Eco.Core.Tests
{
    public class CampaignFixtureTests
    {
        private static Campaign LoadEcoBmth()
        {
            var path = Path.Combine(Application.streamingAssetsPath, "Campaigns", "eco-bmth-2026.json");
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Campaign>(json);
        }

        [Test]
        public void EcoBmthCampaign_HasExpectedShape()
        {
            var campaign = LoadEcoBmth();

            Assert.AreEqual(5, campaign.Axes.Count);
            Assert.AreEqual(8, campaign.Questions.Count);
            Assert.AreEqual(24, campaign.Tracks.Count);
            Assert.AreEqual(4, campaign.Questions[0].Options.Count);
        }

        [Test]
        public void EcoBmthCampaign_EveryOptionDelta_ReferencesADeclaredAxis()
        {
            var campaign = LoadEcoBmth();
            var axisIds = campaign.Axes.Select(a => a.Id).ToHashSet();

            foreach (var question in campaign.Questions)
            foreach (var option in question.Options)
            foreach (var axisId in option.Deltas.Keys)
                Assert.Contains(axisId, axisIds.ToList(),
                    $"Question '{question.Id}' option '{option.Label}' references undeclared axis '{axisId}'.");
        }

        [Test]
        public void EcoBmthCampaign_EveryTrackFingerprint_CoversAllAxes()
        {
            var campaign = LoadEcoBmth();
            var axisIds = campaign.Axes.Select(a => a.Id).ToList();

            foreach (var track in campaign.Tracks)
            foreach (var axisId in axisIds)
                Assert.IsTrue(track.Fingerprint.ContainsKey(axisId),
                    $"Track '{track.Title}' is missing a fingerprint value for axis '{axisId}'.");
        }

        [Test]
        public void ChaosAndBoldnessAnswers_MatchTrackLeansChaoticAndBold()
        {
            var campaign = LoadEcoBmth();
            var profile = new ProfileVector(campaign.Axes, campaign.Scoring.AxisFloor);

            var optionIndexPerQuestion = new[] { 0, 0, 3, 3, 3, 0, 0, 1 };
            for (var i = 0; i < campaign.Questions.Count; i++)
            {
                var question = campaign.Questions[i];
                var option = question.Options[optionIndexPerQuestion[i]];
                profile.ApplyAnswer(question, option, campaign.Scoring);
            }

            var top = MatchCalculator.Rank(profile, campaign.Axes, campaign.Tracks, campaign.Scoring)[0];

            Assert.GreaterOrEqual(top.Track.Fingerprint["ous"], 4);
            Assert.LessOrEqual(top.Track.Fingerprint["rom"], 2);
        }

        [Test]
        public void RomanceAndNostalgiaAnswers_MatchTrackLeansTenderAndReflective()
        {
            var campaign = LoadEcoBmth();
            var profile = new ProfileVector(campaign.Axes, campaign.Scoring.AxisFloor);

            var optionIndexPerQuestion = new[] { 3, 1, 0, 0, 1, 2, 1, 3 };
            for (var i = 0; i < campaign.Questions.Count; i++)
            {
                var question = campaign.Questions[i];
                var option = question.Options[optionIndexPerQuestion[i]];
                profile.ApplyAnswer(question, option, campaign.Scoring);
            }

            var top = MatchCalculator.Rank(profile, campaign.Axes, campaign.Tracks, campaign.Scoring)[0];

            Assert.GreaterOrEqual(top.Track.Fingerprint["rom"], 4);
            Assert.LessOrEqual(top.Track.Fingerprint["cao"], 2);
        }
    }
}

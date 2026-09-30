using System.Collections.Generic;
using Eco.Core.Matching;
using Eco.Core.Models;
using NUnit.Framework;

namespace Eco.Core.Tests
{
    public class MatchCalculatorTests
    {
        private static List<AxisDef> Axes => new()
        {
            new AxisDef { Id = "x" },
            new AxisDef { Id = "y" }
        };

        private static ScoringConfig Scoring => new()
        {
            Weights = new Dictionary<string, double> { { "default", 1.0 } },
            AxisFloor = 0,
            MinDisplayedMatch = 70
        };

        [Test]
        public void Rank_PutsClosestFingerprintFirst()
        {
            var profile = new ProfileVector(Axes);
            profile.Apply(new Dictionary<string, int> { { "x", 5 }, { "y", 1 } });

            var tracks = new List<TrackDef>
            {
                new() { Id = "far", Fingerprint = new Dictionary<string, int> { { "x", 1 }, { "y", 5 } } },
                new() { Id = "close", Fingerprint = new Dictionary<string, int> { { "x", 5 }, { "y", 1 } } }
            };

            var ranked = MatchCalculator.Rank(profile, Axes, tracks, Scoring);

            Assert.AreEqual("close", ranked[0].Track.Id);
            Assert.AreEqual("far", ranked[1].Track.Id);
        }

        [Test]
        public void Rank_DisplayPercent_NeverBelowConfiguredMinimum()
        {
            var profile = new ProfileVector(Axes);
            profile.Apply(new Dictionary<string, int> { { "x", 1 }, { "y", 0 } });

            var tracks = new List<TrackDef>
            {
                new() { Id = "opposite", Fingerprint = new Dictionary<string, int> { { "x", 0 }, { "y", 1 } } }
            };

            var ranked = MatchCalculator.Rank(profile, Axes, tracks, Scoring);

            Assert.GreaterOrEqual(ranked[0].DisplayPercent, Scoring.MinDisplayedMatch);
        }

        [Test]
        public void Rank_PerfectMatch_DisplaysAsHundredPercent()
        {
            var profile = new ProfileVector(Axes);
            profile.Apply(new Dictionary<string, int> { { "x", 4 }, { "y", 2 } });

            var tracks = new List<TrackDef>
            {
                new() { Id = "identical", Fingerprint = new Dictionary<string, int> { { "x", 4 }, { "y", 2 } } }
            };

            var ranked = MatchCalculator.Rank(profile, Axes, tracks, Scoring);

            Assert.AreEqual(100, ranked[0].DisplayPercent);
        }
    }
}

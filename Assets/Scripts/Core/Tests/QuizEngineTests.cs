using System;
using System.Collections.Generic;
using System.IO;
using Eco.Core.Models;
using Eco.Core.Quiz;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace Eco.Core.Tests
{
    public class QuizEngineTests
    {
        private static Campaign TinyCampaign => new()
        {
            Axes = new List<AxisDef> { new() { Id = "a" }, new() { Id = "b" } },
            Scoring = new ScoringConfig { Weights = new Dictionary<string, double> { { "solo", 1.0 } }, AxisFloor = 0, MinDisplayedMatch = 70 },
            Questions = new List<QuestionDef>
            {
                new()
                {
                    Id = "q1", Tag = "solo",
                    Options = new List<OptionDef>
                    {
                        new() { Label = "up-a", Deltas = new Dictionary<string, int> { { "a", 2 } } },
                        new() { Label = "up-b", Deltas = new Dictionary<string, int> { { "b", 2 } } }
                    }
                },
                new()
                {
                    Id = "q2", Tag = "solo",
                    Options = new List<OptionDef>
                    {
                        new() { Label = "up-a", Deltas = new Dictionary<string, int> { { "a", 3 } } },
                        new() { Label = "up-b", Deltas = new Dictionary<string, int> { { "b", 3 } } }
                    }
                }
            },
            Tracks = new List<TrackDef>
            {
                new() { Id = "leans-a", Fingerprint = new Dictionary<string, int> { { "a", 5 }, { "b", 1 } } },
                new() { Id = "leans-b", Fingerprint = new Dictionary<string, int> { { "a", 1 }, { "b", 5 } } }
            }
        };

        [Test]
        public void FreshEngine_StartsAtFirstQuestion()
        {
            var engine = new QuizEngine(TinyCampaign);

            Assert.AreEqual(1, engine.QuestionNumber);
            Assert.AreEqual(2, engine.TotalQuestions);
            Assert.AreEqual("q1", engine.CurrentQuestion.Id);
            Assert.IsFalse(engine.IsComplete);
        }

        [Test]
        public void Answer_AdvancesToNextQuestion()
        {
            var engine = new QuizEngine(TinyCampaign);
            engine.Answer(0);

            Assert.AreEqual(2, engine.QuestionNumber);
            Assert.AreEqual("q2", engine.CurrentQuestion.Id);
            Assert.IsFalse(engine.IsComplete);
        }

        [Test]
        public void Answer_LastQuestion_MarksComplete()
        {
            var engine = new QuizEngine(TinyCampaign);
            engine.Answer(0);
            var outcome = engine.Answer(0);

            Assert.IsTrue(outcome.IsComplete);
            Assert.IsTrue(engine.IsComplete);
            Assert.IsNull(engine.CurrentQuestion);
        }

        [Test]
        public void Answer_ReportsAxisChangesAndRunningVibe()
        {
            var engine = new QuizEngine(TinyCampaign);
            var outcome = engine.Answer(0);

            Assert.AreEqual(2.0, outcome.AxisChanges["a"]);
            Assert.AreEqual(2.0, outcome.CurrentVibe["a"]);
            Assert.AreEqual(0.0, outcome.CurrentVibe["b"]);
        }

        [Test]
        public void Answer_InvalidOptionIndex_Throws()
        {
            var engine = new QuizEngine(TinyCampaign);

            Assert.Throws<ArgumentOutOfRangeException>(() => engine.Answer(99));
        }

        [Test]
        public void Answer_AfterQuizComplete_Throws()
        {
            var engine = new QuizEngine(TinyCampaign);
            engine.Answer(0);
            engine.Answer(0);

            Assert.Throws<InvalidOperationException>(() => engine.Answer(0));
        }

        [Test]
        public void ComputeResult_BeforeComplete_Throws()
        {
            var engine = new QuizEngine(TinyCampaign);

            Assert.Throws<InvalidOperationException>(() => engine.ComputeResult());
        }

        [Test]
        public void ComputeResult_AnsweringTowardA_RanksLeansATrackFirst()
        {
            var engine = new QuizEngine(TinyCampaign);
            engine.Answer(0);
            engine.Answer(0);

            var ranked = engine.ComputeResult();

            Assert.AreEqual("leans-a", ranked[0].Track.Id);
        }

        [Test]
        public void EcoBmthCampaign_FullPlaythrough_ProducesRankedResult()
        {
            var path = Path.Combine(Application.streamingAssetsPath, "Campaigns", "eco-bmth-2026.json");
            var campaign = JsonConvert.DeserializeObject<Campaign>(File.ReadAllText(path));
            var engine = new QuizEngine(campaign);

            while (!engine.IsComplete)
                engine.Answer(0);

            var ranked = engine.ComputeResult();

            Assert.AreEqual(24, ranked.Count);
            Assert.GreaterOrEqual(ranked[0].DisplayPercent, campaign.Scoring.MinDisplayedMatch);
            Assert.GreaterOrEqual(ranked[0].Similarity, ranked[1].Similarity);
        }
    }
}

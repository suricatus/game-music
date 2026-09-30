using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Core.Models;

namespace Eco.Core.Matching
{
    public readonly struct TrackMatch
    {
        public readonly TrackDef Track;
        public readonly double Similarity;
        public readonly int DisplayPercent;

        public TrackMatch(TrackDef track, double similarity, int displayPercent)
        {
            Track = track;
            Similarity = similarity;
            DisplayPercent = displayPercent;
        }
    }

    public static class MatchCalculator
    {
        public static List<TrackMatch> Rank(ProfileVector profile, List<AxisDef> axes, List<TrackDef> tracks, ScoringConfig scoring)
        {
            var axisIds = axes.Select(a => a.Id).ToList();
            var userVector = ToArray(profile.Values, axisIds);

            var matches = tracks.Select(track =>
            {
                var trackVector = ToArray(track.Fingerprint, axisIds);
                var similarity = CosineSimilarity(userVector, trackVector);
                var displayPercent = ScaleToDisplay(similarity, scoring.MinDisplayedMatch);
                return new TrackMatch(track, similarity, displayPercent);
            });

            return matches.OrderByDescending(m => m.Similarity).ToList();
        }

        private static double[] ToArray(IReadOnlyDictionary<string, double> values, List<string> axisIds)
        {
            return axisIds.Select(id => values.TryGetValue(id, out var v) ? v : 0.0).ToArray();
        }

        private static double[] ToArray(IReadOnlyDictionary<string, int> values, List<string> axisIds)
        {
            return axisIds.Select(id => values.TryGetValue(id, out var v) ? (double)v : 0.0).ToArray();
        }

        private static double CosineSimilarity(double[] a, double[] b)
        {
            double dot = 0, normA = 0, normB = 0;
            for (var i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            if (normA == 0 || normB == 0) return 0;
            return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        private static int ScaleToDisplay(double similarity, int minDisplayedMatch)
        {
            var clamped = Math.Clamp(similarity, 0.0, 1.0);
            var scaled = minDisplayedMatch + clamped * (100 - minDisplayedMatch);
            return (int)Math.Round(scaled);
        }
    }
}

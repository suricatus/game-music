using System;
using System.Collections.Generic;
using System.Linq;
using Eco.Core.Models;

namespace Eco.Core.Matching
{
    public class ProfileVector
    {
        private readonly Dictionary<string, double> _values;
        private readonly int _floor;

        public ProfileVector(IEnumerable<AxisDef> axes, int floor = 0)
        {
            _floor = floor;
            _values = axes.ToDictionary(a => a.Id, a => (double)floor);
        }

        public IReadOnlyDictionary<string, double> Values => _values;

        public Dictionary<string, double> Apply(Dictionary<string, int> deltas, double weight = 1.0)
        {
            var actualChanges = new Dictionary<string, double>();

            foreach (var delta in deltas)
            {
                if (!_values.ContainsKey(delta.Key))
                    throw new ArgumentException(
                        $"Delta references axis id '{delta.Key}', which is not declared in this campaign's axes.");

                var before = _values[delta.Key];
                var after = Math.Max(_floor, before + delta.Value * weight);
                _values[delta.Key] = after;
                actualChanges[delta.Key] = after - before;
            }

            return actualChanges;
        }

        public Dictionary<string, double> ApplyAnswer(QuestionDef question, OptionDef chosenOption, ScoringConfig scoring)
        {
            var weight = scoring.Weights != null && scoring.Weights.TryGetValue(question.Tag, out var w) ? w : 1.0;
            return Apply(chosenOption.Deltas, weight);
        }
    }
}

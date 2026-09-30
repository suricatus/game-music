using System;
using System.Collections.Generic;
using Eco.Core.Models;

namespace Eco.Core.Sessions
{
    public readonly struct LeadCaptureResult
    {
        public readonly bool Success;
        public readonly IReadOnlyList<string> MissingFields;

        private LeadCaptureResult(bool success, IReadOnlyList<string> missingFields)
        {
            Success = success;
            MissingFields = missingFields;
        }

        public static LeadCaptureResult Ok() => new(true, Array.Empty<string>());
        public static LeadCaptureResult Invalid(IReadOnlyList<string> missingFields) => new(false, missingFields);
    }

    public static class LeadCaptureValidator
    {
        public static bool Validate(LeadCaptureConfig config, IReadOnlyDictionary<string, string> submitted, out List<string> missing)
        {
            missing = new List<string>();
            if (config?.Required == null) return true;

            foreach (var field in config.Required)
            {
                if (!submitted.TryGetValue(field, out var value) || string.IsNullOrWhiteSpace(value))
                    missing.Add(field);
            }

            return missing.Count == 0;
        }
    }
}

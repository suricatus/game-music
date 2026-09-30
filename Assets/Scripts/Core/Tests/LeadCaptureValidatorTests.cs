using System.Collections.Generic;
using Eco.Core.Models;
using Eco.Core.Sessions;
using NUnit.Framework;

namespace Eco.Core.Tests
{
    public class LeadCaptureValidatorTests
    {
        private static LeadCaptureConfig Config => new()
        {
            Fields = new List<string> { "nome", "email", "empresa" },
            Required = new List<string> { "nome", "email" }
        };

        [Test]
        public void Validate_AllRequiredFieldsPresent_Passes()
        {
            var submitted = new Dictionary<string, string> { { "nome", "Amanda" }, { "email", "a@b.com" } };

            var passed = LeadCaptureValidator.Validate(Config, submitted, out var missing);

            Assert.IsTrue(passed);
            Assert.IsEmpty(missing);
        }

        [Test]
        public void Validate_MissingRequiredField_ListsIt()
        {
            var submitted = new Dictionary<string, string> { { "nome", "Amanda" } };

            var passed = LeadCaptureValidator.Validate(Config, submitted, out var missing);

            Assert.IsFalse(passed);
            CollectionAssert.Contains(missing, "email");
        }

        [Test]
        public void Validate_BlankRequiredField_CountsAsMissing()
        {
            var submitted = new Dictionary<string, string> { { "nome", "Amanda" }, { "email", "   " } };

            var passed = LeadCaptureValidator.Validate(Config, submitted, out var missing);

            Assert.IsFalse(passed);
            CollectionAssert.Contains(missing, "email");
        }

        [Test]
        public void Validate_OptionalFieldMissing_StillPasses()
        {
            var submitted = new Dictionary<string, string> { { "nome", "Amanda" }, { "email", "a@b.com" } };

            var passed = LeadCaptureValidator.Validate(Config, submitted, out _);

            Assert.IsTrue(passed);
        }
    }
}

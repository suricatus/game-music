using System.Collections.Generic;
using Eco.Core.Matching;
using Eco.Core.Models;
using NUnit.Framework;

namespace Eco.Core.Tests
{
    public class ProfileVectorTests
    {
        private static List<AxisDef> TwoAxes => new()
        {
            new AxisDef { Id = "a" },
            new AxisDef { Id = "b" }
        };

        [Test]
        public void Apply_AddsDeltaToNamedAxis()
        {
            var vector = new ProfileVector(TwoAxes);
            vector.Apply(new Dictionary<string, int> { { "a", 3 } });

            Assert.AreEqual(3, vector.Values["a"]);
            Assert.AreEqual(0, vector.Values["b"]);
        }

        [Test]
        public void Apply_ClampsAtFloor_NeverGoesBelowIt()
        {
            var vector = new ProfileVector(TwoAxes, floor: 0);
            vector.Apply(new Dictionary<string, int> { { "a", -5 } });

            Assert.AreEqual(0, vector.Values["a"]);
        }

        [Test]
        public void Apply_WithWeight_ScalesDeltaBeforeAdding()
        {
            var vector = new ProfileVector(TwoAxes);
            vector.Apply(new Dictionary<string, int> { { "a", 2 } }, weight: 0.5);

            Assert.AreEqual(1.0, vector.Values["a"]);
        }

        [Test]
        public void Apply_UnknownAxisId_Throws()
        {
            var vector = new ProfileVector(TwoAxes);

            Assert.Throws<System.ArgumentException>(() =>
                vector.Apply(new Dictionary<string, int> { { "does-not-exist", 1 } }));
        }

        [Test]
        public void Apply_ReturnsActualChangeApplied()
        {
            var vector = new ProfileVector(TwoAxes);
            var changes = vector.Apply(new Dictionary<string, int> { { "a", 2 } }, weight: 0.5);

            Assert.AreEqual(1.0, changes["a"]);
        }

        [Test]
        public void Apply_WhenFloorClamps_ReturnedChangeReflectsWhatActuallyMoved()
        {
            var vector = new ProfileVector(TwoAxes, floor: 0);
            var changes = vector.Apply(new Dictionary<string, int> { { "a", -5 } });

            Assert.AreEqual(0.0, changes["a"]);
        }
    }
}

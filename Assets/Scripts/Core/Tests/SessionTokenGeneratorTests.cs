using Eco.Core.Sessions;
using NUnit.Framework;

namespace Eco.Core.Tests
{
    public class SessionTokenGeneratorTests
    {
        [Test]
        public void Generate_DefaultLength_ReturnsEightCharacters()
        {
            Assert.AreEqual(8, SessionTokenGenerator.Generate().Length);
        }

        [Test]
        public void Generate_NeverContainsAmbiguousCharacters()
        {
            for (var i = 0; i < 200; i++)
            {
                var token = SessionTokenGenerator.Generate();
                Assert.IsFalse(token.Contains('0'));
                Assert.IsFalse(token.Contains('O'));
                Assert.IsFalse(token.Contains('1'));
                Assert.IsFalse(token.Contains('I'));
                Assert.IsFalse(token.Contains('L'));
            }
        }

        [Test]
        public void Generate_SameSeed_ProducesSameToken()
        {
            var a = SessionTokenGenerator.Generate(random: new System.Random(42));
            var b = SessionTokenGenerator.Generate(random: new System.Random(42));

            Assert.AreEqual(a, b);
        }
    }
}

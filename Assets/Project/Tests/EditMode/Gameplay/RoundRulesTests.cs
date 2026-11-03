using NUnit.Framework;
using SelfBulletting.Gameplay.Rules;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class RoundRulesTests
    {
        private RoundRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = new RoundRules(TestSettings.Economy());
        }

        [TestCase(100f)]
        [TestCase(10f)]
        public void Evaluate_ClearPath_WinsAtAnySize(float radiusPercent)
        {
            Assert.AreEqual(RoundOutcome.Win, _rules.Evaluate(true, radiusPercent));
        }

        [Test]
        public void Evaluate_BlockedPathAtCriticalSize_Loses()
        {
            Assert.AreEqual(RoundOutcome.Lose, _rules.Evaluate(false, 10f));
        }

        [TestCase(15f)]
        [TestCase(100f)]
        public void Evaluate_BlockedPathWithRemainder_Continues(float radiusPercent)
        {
            Assert.AreEqual(RoundOutcome.Continue, _rules.Evaluate(false, radiusPercent));
        }
    }
}

using NUnit.Framework;
using SelfBulletting.Gameplay.Modes;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class GameModeTests
    {
        [TestCase(GameMode.Classic, false)]
        [TestCase(GameMode.Slingshot, true)]
        [TestCase(GameMode.Arrow, true)]
        [TestCase(GameMode.Hose, true)]
        public void UsesCurvedPath_EveryModeButClassic(GameMode mode, bool expected)
        {
            Assert.AreEqual(expected, mode.UsesCurvedPath());
        }
    }
}

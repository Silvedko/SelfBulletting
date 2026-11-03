using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Finish;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Presentation.Null;
using SelfBulletting.Infrastructure.Services.Audio;
using UnityEngine;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class WinSequenceTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void HopPath_SplitsPathIntoEqualHopsEndingAtTarget()
        {
            IReadOnlyList<Vector3> hops = HopPath.Build(Vector3.zero, new Vector3(0f, 0f, 4f), 1.5f);

            Assert.AreEqual(3, hops.Count);
            Assert.AreEqual(4f / 3f, hops[0].z, Tolerance);
            Assert.AreEqual(4f, hops[2].z, Tolerance);
        }

        [Test]
        public void HopPath_ZeroDistance_HasNoHops()
        {
            Assert.AreEqual(0, HopPath.Build(Vector3.one, Vector3.one, 1.5f).Count);
        }

        [Test]
        public void PlayAsync_HopsToFinishAndOpensDoorWithinDoorDistance()
        {
            var ball = new RecordingBallView();
            var door = new RecordingDoorView(ball);
            var views = new GameViews(ball, new NullShotView(), new NullPathView(), new NullAimView(), new NullScoreView(), door,
                new NullBallHealthView());
            var sequence = new WinSequence(new FinishSettings(5f, 1.5f, 0.6f, 0.25f), views, new NullAudioService());

            sequence.PlayAsync(Vector3.zero, new Vector3(0f, 0f, 12f), CancellationToken.None).GetAwaiter().GetResult();

            // Hops start at z = 0, 1.5, 3 ... The first start within 5 m of the door is z = 7.5, after 5 hops.
            Assert.AreEqual(8, ball.Hops.Count);
            Assert.AreEqual(12f, ball.Hops[7].z, Tolerance);
            Assert.AreEqual(1, door.OpenCount);
            Assert.AreEqual(5, door.HopsBeforeOpen);
        }

        private sealed class RecordingBallView : IBallView
        {
            public List<Vector3> Hops { get; } = new List<Vector3>();

            public void Place(Vector3 groundPoint, float worldRadius)
            {
            }

            public void SetRadius(float worldRadius)
            {
            }

            public void SetDanger(float level)
            {
            }

            public UniTask HopAsync(Vector3 groundPoint, float height, float durationSeconds, CancellationToken cancellationToken)
            {
                Hops.Add(groundPoint);
                return UniTask.CompletedTask;
            }
        }

        private sealed class RecordingDoorView : IDoorView
        {
            private readonly RecordingBallView _ball;

            public RecordingDoorView(RecordingBallView ball)
            {
                _ball = ball;
            }

            public int OpenCount { get; private set; }
            public int HopsBeforeOpen { get; private set; }

            public void Open()
            {
                OpenCount++;
                HopsBeforeOpen = _ball.Hops.Count;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Gameplay.Hose
{
    /// <summary>
    /// The water of the hose: pouring drains the ball for real and launches the drops that are due along the arc of the
    /// current aim; every frame the drops in the air fly on and report what they hit.
    /// </summary>
    public sealed class HoseStream
    {
        private readonly HoseFlow _flow;
        private readonly HoseDrops _drops;
        private readonly DropSize _size;
        private readonly LobTargeting _targeting;
        private readonly BallModel _ball;

        public HoseStream(HoseFlow flow, HoseDrops drops, DropSize size, LobTargeting targeting, BallModel ball)
        {
            _flow = flow;
            _drops = drops;
            _size = size;
            _targeting = targeting;
            _ball = ball;
        }

        public bool IsFlowing => _flow.IsFlowing;

        /// <summary>No drop is in the air.</summary>
        public bool IsEmpty => _drops.Count == 0;

        public ReadOnlySpan<WaterDrop> InFlight => _drops.InFlight;

        /// <summary>Pours for one frame. The drops of this frame leave the ball at its size before the drain.</summary>
        /// <param name="direction">Flat unit aim direction.</param>
        /// <param name="rangeShare">0 — the shortest range, 1 — the longest.</param>
        /// <param name="dropRadius">Radius of the drops leaving now, meters.</param>
        /// <returns>The arc the drops leave along.</returns>
        public ArcPath Pour(Vector3 direction, float rangeShare, float deltaTime, out float dropRadius)
        {
            _flow.Start();
            dropRadius = _size.GetRadius(_ball.RadiusPercent);
            ArcPath arc = _targeting.Find(direction, rangeShare, _ball.WorldRadius, dropRadius).Arc;

            int dueDrops = _flow.Tick(deltaTime, _ball.Remainder, out float costPercent);
            for (int i = 0; i < dueDrops; i++)
            {
                _drops.TryLaunch(arc, dropRadius);
            }

            if (costPercent > 0f)
            {
                _ball.Spend(costPercent);
            }

            return arc;
        }

        public void Stop()
        {
            _flow.Stop();
        }

        /// <param name="impacts">Filled with the drops that hit a fire or the ground this frame.</param>
        public void Fly(float deltaTime, List<DropImpact> impacts)
        {
            _drops.Tick(deltaTime, impacts);
        }

        public void Clear()
        {
            _flow.Stop();
            _drops.Clear();
        }
    }
}

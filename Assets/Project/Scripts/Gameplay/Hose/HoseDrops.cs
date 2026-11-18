using System;
using System.Collections.Generic;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Scanning;
using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Gameplay.Hose
{
    /// <summary>
    /// The drops in the air, in a fixed pool: nothing is allocated while the water flows. Every frame each drop moves
    /// along its arc and sweeps the piece it covered, so it stops on the first fire it touches; a drop that touches
    /// nothing lands at the end of its arc.
    /// </summary>
    public sealed class HoseDrops
    {
        private const float ShortestFlightSeconds = 0.01f;

        private readonly WaterDrop[] _drops;
        private readonly float _speed;
        private readonly DropSize _size;
        private readonly IObstacleSweep _sweep;

        public HoseDrops(HoseSettings settings, DropSize size, IObstacleSweep sweep)
        {
            _drops = new WaterDrop[settings.MaxDropsInFlight];
            _speed = settings.DropSpeed;
            _size = size;
            _sweep = sweep;
        }

        public int Count { get; private set; }
        public int Capacity => _drops.Length;

        /// <summary>The drops in the air right now, in no particular order.</summary>
        public ReadOnlySpan<WaterDrop> InFlight => new ReadOnlySpan<WaterDrop>(_drops, 0, Count);

        /// <param name="radius">Drop radius, meters.</param>
        /// <returns>False when the pool is full: the drop is not launched.</returns>
        public bool TryLaunch(ArcPath arc, float radius)
        {
            if (Count >= _drops.Length)
            {
                return false;
            }

            float flightSeconds = Mathf.Max(arc.HorizontalLength / _speed, ShortestFlightSeconds);
            _drops[Count++] = new WaterDrop(arc, radius, flightSeconds, 0f);
            return true;
        }

        /// <param name="impacts">Filled with the drops that hit something this frame; they leave the air.</param>
        public void Tick(float deltaTime, List<DropImpact> impacts)
        {
            impacts.Clear();

            // Backwards, so a finished drop can be swapped with the last one, which has already moved.
            for (int i = Count - 1; i >= 0; i--)
            {
                WaterDrop drop = _drops[i];
                WaterDrop moved = drop.Advance(deltaTime);

                if (_sweep.TrySweep(drop.Center, moved.Center, _size.GetTouchRadius(drop.Radius), out Vector3 contact,
                        out Collider obstacle))
                {
                    impacts.Add(DropImpact.OnObstacle(contact, drop.Radius, obstacle));
                    RemoveAt(i);
                }
                else if (moved.HasLanded)
                {
                    impacts.Add(DropImpact.OnGround(moved.Center, drop.Radius));
                    RemoveAt(i);
                }
                else
                {
                    _drops[i] = moved;
                }
            }
        }

        public void Clear()
        {
            Array.Clear(_drops, 0, Count);
            Count = 0;
        }

        private void RemoveAt(int index)
        {
            Count--;
            _drops[index] = _drops[Count];
            _drops[Count] = default;
        }
    }
}

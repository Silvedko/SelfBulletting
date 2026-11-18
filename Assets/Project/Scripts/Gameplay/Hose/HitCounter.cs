using System.Collections.Generic;
using UnityEngine;

namespace SelfBulletting.Gameplay.Hose
{
    /// <summary>
    /// How many hits every fire has taken. A fire goes out after its own number of hits; hits on one fire never count
    /// towards another, so there is no chain.
    /// </summary>
    /// <typeparam name="TFire">The fire; obstacles in the game, anything in tests.</typeparam>
    public sealed class HitCounter<TFire>
    {
        private readonly int _hitsToExtinguish;
        private readonly Dictionary<TFire, int> _hits = new Dictionary<TFire, int>();

        public HitCounter(int hitsToExtinguish)
        {
            _hitsToExtinguish = Mathf.Max(1, hitsToExtinguish);
        }

        /// <returns>How much of the fire is put out after this hit: 1 — it goes out.</returns>
        public float AddHit(TFire fire)
        {
            _hits.TryGetValue(fire, out int hits);
            hits = Mathf.Min(hits + 1, _hitsToExtinguish);
            _hits[fire] = hits;
            return (float)hits / _hitsToExtinguish;
        }
    }
}

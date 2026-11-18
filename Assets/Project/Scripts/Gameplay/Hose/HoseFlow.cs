using SelfBulletting.Configs;
using UnityEngine;

namespace SelfBulletting.Gameplay.Hose
{
    /// <summary>
    /// The tap of the hose: while it is open the ball drains at a steady rate and drops leave at a steady pace, the first
    /// one at once. Closing it stops both; opening it again starts the pace over.
    /// </summary>
    public sealed class HoseFlow
    {
        private const float FirstDropAtOnce = 1f;

        private readonly HoseSettings _settings;

        private float _dueDrops;

        public HoseFlow(HoseSettings settings)
        {
            _settings = settings;
        }

        public bool IsFlowing { get; private set; }

        public void Start()
        {
            if (IsFlowing)
            {
                return;
            }

            IsFlowing = true;
            _dueDrops = FirstDropAtOnce;
        }

        public void Stop()
        {
            IsFlowing = false;
            _dueDrops = 0f;
        }

        /// <param name="remainderPercent">What the ball can still spend, % of R0: the drain never takes more.</param>
        /// <param name="costPercent">What the ball loses this frame, % of R0.</param>
        /// <returns>How many drops leave this frame.</returns>
        public int Tick(float deltaTime, float remainderPercent, out float costPercent)
        {
            if (!IsFlowing)
            {
                costPercent = 0f;
                return 0;
            }

            costPercent = Mathf.Clamp(_settings.DrainPercentPerSecond * deltaTime, 0f, Mathf.Max(0f, remainderPercent));
            _dueDrops += _settings.DropsPerSecond * deltaTime;

            int drops = Mathf.FloorToInt(_dueDrops);
            _dueDrops -= drops;
            return drops;
        }
    }
}

using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    /// <summary>
    /// Hose mode: while the water flows the ball drains by itself and pours out a stream of drops. Every drop puts out
    /// only the fires its splash reaches. Sizes are percents of R0 unless a name says otherwise.
    /// </summary>
    [Serializable]
    public sealed class HoseSettings
    {
        private const float LowestPercent = 1f;
        private const float HighestPercent = 99f;
        private const float SmallestDropPercent = 0.1f;

        [Tooltip("Critical ball size, % of R0: the water stops here and the round is decided as in every mode. Every " +
            "mode loses at the same size, so keep it equal to Shot.MinShootPercent of the shooting modes: the game " +
            "refuses to start otherwise. With the drain below, (100 − this) / drain is how many seconds of water the " +
            "ball holds.")]
        [SerializeField] private float _criticalPercent = 10f;

        [Tooltip("How much the ball loses while the water flows, % of R0 per second.")]
        [SerializeField, Min(0.01f)] private float _drainPercentPerSecond = 2f;

        [Tooltip("Drops per second while the water flows. The first one leaves at once.")]
        [SerializeField, Min(0.1f)] private float _dropsPerSecond = 10f;

        [Tooltip("Horizontal speed of a drop along its arc, m/s.")]
        [SerializeField, Min(0.1f)] private float _dropSpeed = 15f;

        [Tooltip("Drop radius at the full ball, % of R0. The drops shrink with the ball.")]
        [SerializeField, Min(0.1f)] private float _dropRadiusPercent = 8f;

        [Tooltip("The drops never shrink below this radius, % of R0: near the critical size they stay visible and " +
            "still put fires out. Not above the full drop.")]
        [SerializeField, Min(0.1f)] private float _minDropRadiusPercent = 4f;

        [Tooltip("The drop feels the fires with a sphere this many times its radius: a bit bigger than it looks.")]
        [SerializeField, Min(1f)] private float _touchRadiusScale = 1.5f;

        [Tooltip("Where the drop hits, every fire within this many drop radii takes a hit. Not below the touch sphere.")]
        [SerializeField, Min(1f)] private float _splashRadiusScale = 2.5f;

        [Tooltip("Hits a fire takes before it goes out.")]
        [SerializeField, Min(1)] private int _hitsToExtinguish = 1;

        [Tooltip("Most drops in the air at once. Drops and their views come from a pool of this size.")]
        [SerializeField, Min(1)] private int _maxDropsInFlight = 32;

        [Tooltip("The preview checks the arc in this many pieces to find what the next drop hits.")]
        [SerializeField, Min(1)] private int _previewSegments = 12;

        public HoseSettings()
        {
        }

        public HoseSettings(float criticalPercent, float drainPercentPerSecond, float dropsPerSecond, float dropSpeed,
            float dropRadiusPercent, float minDropRadiusPercent, float touchRadiusScale, float splashRadiusScale,
            int hitsToExtinguish, int maxDropsInFlight, int previewSegments)
        {
            _criticalPercent = criticalPercent;
            _drainPercentPerSecond = drainPercentPerSecond;
            _dropsPerSecond = dropsPerSecond;
            _dropSpeed = dropSpeed;
            _dropRadiusPercent = dropRadiusPercent;
            _minDropRadiusPercent = minDropRadiusPercent;
            _touchRadiusScale = touchRadiusScale;
            _splashRadiusScale = splashRadiusScale;
            _hitsToExtinguish = hitsToExtinguish;
            _maxDropsInFlight = maxDropsInFlight;
            _previewSegments = previewSegments;
            Validate();
        }

        public float CriticalPercent => _criticalPercent;
        public float DrainPercentPerSecond => _drainPercentPerSecond;
        public float DropsPerSecond => _dropsPerSecond;
        public float DropSpeed => _dropSpeed;
        public float DropRadiusPercent => _dropRadiusPercent;
        public float MinDropRadiusPercent => _minDropRadiusPercent;
        public float TouchRadiusScale => _touchRadiusScale;
        public float SplashRadiusScale => _splashRadiusScale;
        public int HitsToExtinguish => _hitsToExtinguish;
        public int MaxDropsInFlight => _maxDropsInFlight;
        public int PreviewSegments => _previewSegments;

        public void Validate()
        {
            _criticalPercent = Mathf.Clamp(_criticalPercent, LowestPercent, HighestPercent);
            _minDropRadiusPercent = Mathf.Clamp(_minDropRadiusPercent, SmallestDropPercent, _dropRadiusPercent);
            _splashRadiusScale = Mathf.Max(_splashRadiusScale, _touchRadiusScale);
            _hitsToExtinguish = Mathf.Max(1, _hitsToExtinguish);
            _maxDropsInFlight = Mathf.Max(1, _maxDropsInFlight);
            _previewSegments = Mathf.Max(1, _previewSegments);
        }
    }
}

using UnityEngine;

namespace SelfBulletting.Views.Water
{
    /// <summary>
    /// Water spreading over the ground. When the shot lands, droplets fly out of the impact and a ripple runs out
    /// from it; then every fire the infection wave reaches gets a puddle and a small ripple under it, so the water
    /// visibly spreads outwards in step with the wave. Three looping particle systems with zero emission rate serve
    /// the whole level, so nothing is instantiated or pooled during play.
    /// </summary>
    public sealed class WaterSplashEffect : MonoBehaviour
    {
        [Tooltip("World-space system: droplets that fly out of the impact and fall back.")]
        [SerializeField] private ParticleSystem _droplets;

        [Tooltip("World-space system with horizontal billboards: rings that run out over the ground.")]
        [SerializeField] private ParticleSystem _ripples;

        [Tooltip("World-space system with horizontal billboards: puddles that spread and dry out.")]
        [SerializeField] private ParticleSystem _puddles;

        [Tooltip("Lift above the ground (above the path strip) against z-fighting, meters.")]
        [SerializeField, Min(0f)] private float _groundOffset = 0.02f;

        [SerializeField, Min(1)] private int _dropletsPerImpact = 14;

        [Tooltip("Droplets out of one Hose drop: it hits many times a second, so far fewer than a shot.")]
        [SerializeField, Min(0)] private int _dropletsPerDrop = 3;

        [Tooltip("Diameter of the impact ripple at its widest and of the impact puddle, shares of the shot radius.")]
        [SerializeField, Min(0f)] private float _impactRippleSize = 9f;
        [SerializeField, Min(0f)] private float _impactPuddleSize = 9f;

        [Tooltip("Diameter of the ripple and the puddle under a fire the water reached, meters. The puddle sits under "
            + "the flame, so it has to be well wider than the fire to show, and neighbours merge into one sheet of water.")]
        [SerializeField, Min(0f)] private float _spreadRippleSize = 1.4f;
        [SerializeField, Min(0f)] private float _spreadPuddleSize = 2.4f;

        /// <param name="groundPoint">Where the shot touches the ground when it lands.</param>
        public void PlayImpact(Vector3 groundPoint, float shotRadius)
        {
            Vector3 position = Lift(groundPoint);
            _droplets.Emit(new ParticleSystem.EmitParams { position = position, applyShapeToPosition = true }, _dropletsPerImpact);
            EmitFlat(_ripples, position, shotRadius * _impactRippleSize);
            EmitFlat(_puddles, position, shotRadius * _impactPuddleSize);
        }

        /// <summary>A Hose drop hit a fire or the ground: a few droplets where it hit, a ripple and a puddle below.</summary>
        /// <param name="center">The drop centre when it hit.</param>
        /// <param name="groundPoint">The ground under it.</param>
        public void PlayDrop(Vector3 center, Vector3 groundPoint, float dropRadius)
        {
            if (_dropletsPerDrop > 0)
            {
                _droplets.Emit(new ParticleSystem.EmitParams { position = center, applyShapeToPosition = true }, _dropletsPerDrop);
            }

            Vector3 position = Lift(groundPoint);
            EmitFlat(_ripples, position, dropRadius * _impactRippleSize);
            EmitFlat(_puddles, position, dropRadius * _impactPuddleSize);
        }

        /// <param name="groundPoint">Foot of a fire the water has just reached.</param>
        public void PlaySpread(Vector3 groundPoint)
        {
            Vector3 position = Lift(groundPoint);
            EmitFlat(_ripples, position, _spreadRippleSize);
            EmitFlat(_puddles, position, _spreadPuddleSize);
        }

        private static void EmitFlat(ParticleSystem particles, Vector3 position, float size)
        {
            var flat = new ParticleSystem.EmitParams
            {
                position = position,
                startSize = size,
                rotation = Random.Range(0f, 360f),
            };
            particles.Emit(flat, 1);
        }

        private Vector3 Lift(Vector3 groundPoint) => groundPoint + Vector3.up * _groundOffset;
    }
}

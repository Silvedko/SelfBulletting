using UnityEngine;

namespace SelfBulletting.Views.Obstacles
{
    /// <summary>
    /// Chunks flying out of exploded obstacles. One looping particle system with zero emission rate serves the whole
    /// level: every explosion emits a burst at its own position, so nothing is instantiated or pooled during play.
    /// </summary>
    public sealed class ExplosionEffect : MonoBehaviour
    {
        [Tooltip("World-space system with a sphere shape and no emission of its own.")]
        [SerializeField] private ParticleSystem _particles;

        [SerializeField, Min(1)] private int _chunksPerExplosion = 12;

        public void Play(Vector3 position, Color color)
        {
            var burst = new ParticleSystem.EmitParams
            {
                position = position,
                applyShapeToPosition = true,
                startColor = color,
            };
            _particles.Emit(burst, _chunksPerExplosion);
        }
    }
}

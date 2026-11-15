using UnityEngine;

namespace SelfBulletting.Views.Obstacles
{
    /// <summary>
    /// Steam and the last sparks of fires that go out. Two looping particle systems with zero emission rate serve the
    /// whole level: every fire that goes out emits a burst at its own position, so nothing is instantiated or pooled
    /// during play.
    /// </summary>
    public sealed class SteamPuffEffect : MonoBehaviour
    {
        [Tooltip("World-space system with a sphere shape and no emission of its own: soft puffs that rise and spread.")]
        [SerializeField] private ParticleSystem _steam;

        [Tooltip("World-space system with no emission of its own: a few sparks that fly up and die out. Optional.")]
        [SerializeField] private ParticleSystem _sparks;

        [SerializeField, Min(1)] private int _puffsPerFire = 6;
        [SerializeField, Min(0)] private int _sparksPerFire = 4;

        public void Play(Vector3 position)
        {
            var burst = new ParticleSystem.EmitParams
            {
                position = position,
                applyShapeToPosition = true,
            };
            _steam.Emit(burst, _puffsPerFire);

            if (_sparks != null && _sparksPerFire > 0)
            {
                _sparks.Emit(burst, _sparksPerFire);
            }
        }
    }
}

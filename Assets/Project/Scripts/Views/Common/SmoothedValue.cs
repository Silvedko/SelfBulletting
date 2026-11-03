using UnityEngine;

namespace SelfBulletting.Views.Common
{
    /// <summary>
    /// A value that eases towards its target, so a size that changes at once (a shot without the scale) does not snap.
    /// Frame-rate independent: the gap to the target shrinks by the same share every second.
    /// </summary>
    public sealed class SmoothedValue
    {
        private const float SnapDistance = 0.0005f;

        private readonly float _sharpness;

        public SmoothedValue(float sharpness)
        {
            _sharpness = sharpness;
        }

        public float Current { get; private set; }
        public float Target { get; private set; }

        public void Snap(float value)
        {
            Current = value;
            Target = value;
        }

        public void SetTarget(float value)
        {
            Target = value;
        }

        /// <returns>True if <see cref="Current"/> changed.</returns>
        public bool Tick(float deltaTime)
        {
            if (Current == Target)
            {
                return false;
            }

            float eased = Mathf.Lerp(Current, Target, 1f - Mathf.Exp(-_sharpness * deltaTime));
            Current = Mathf.Abs(Target - eased) < SnapDistance ? Target : eased;
            return true;
        }
    }
}

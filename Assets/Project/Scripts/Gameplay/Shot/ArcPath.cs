using UnityEngine;

namespace SelfBulletting.Gameplay.Shot
{
    /// <summary>
    /// Flight of a lobbed shot: a parabola from the launch point to the landing point that rises <see cref="ApexHeight"/>
    /// above the straight line between them. The preview and the flight both sample it, so they never disagree.
    /// </summary>
    public readonly struct ArcPath
    {
        private const float ParabolaPeak = 4f;

        public ArcPath(Vector3 start, Vector3 end, float apexHeight)
        {
            Start = start;
            End = end;
            ApexHeight = apexHeight;
        }

        public Vector3 Start { get; }
        public Vector3 End { get; }
        public float ApexHeight { get; }

        public float HorizontalLength
        {
            get
            {
                Vector3 offset = End - Start;
                offset.y = 0f;
                return offset.magnitude;
            }
        }

        /// <param name="share">0 — launch, 1 — landing.</param>
        public Vector3 Evaluate(float share)
        {
            float t = Mathf.Clamp01(share);
            return Vector3.Lerp(Start, End, t) + Vector3.up * (ParabolaPeak * ApexHeight * t * (1f - t));
        }

        /// <summary>Unit direction of the flight at that point: up after the launch, level at the apex, down at landing.</summary>
        /// <param name="share">0 — launch, 1 — landing.</param>
        public Vector3 GetDirection(float share)
        {
            float t = Mathf.Clamp01(share);
            Vector3 tangent = End - Start + Vector3.up * (ParabolaPeak * ApexHeight * (1f - 2f * t));
            return tangent.sqrMagnitude > Mathf.Epsilon ? tangent.normalized : Vector3.forward;
        }
    }
}

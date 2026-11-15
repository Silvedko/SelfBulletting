using SelfBulletting.Configs;
using UnityEngine;

namespace SelfBulletting.Gameplay.Shot
{
    /// <summary>
    /// Finds where a lobbed shot lands: along the aim direction from the ball, at a distance between the shortest and
    /// the longest range, never past the field edge and never closer than the launch point.
    /// </summary>
    public sealed class LobTargeting
    {
        private const float MinLandingGap = 0.1f;

        private readonly LobSettings _settings;
        private readonly Vector3 _ballGroundPoint;
        private readonly Rect _field;

        /// <param name="field">The field on the ground: x is the world X, y is the world Z.</param>
        public LobTargeting(LobSettings settings, Vector3 ballGroundPoint, Rect field)
        {
            _settings = settings;
            _ballGroundPoint = ballGroundPoint;
            _field = field;
        }

        /// <param name="rangeShare">0 — the shortest range, 1 — the longest.</param>
        public float GetRange(float rangeShare) => Mathf.Lerp(_settings.MinRange, _settings.MaxRange, Mathf.Clamp01(rangeShare));

        /// <param name="direction">Flat unit aim direction.</param>
        /// <param name="rangeShare">0 — the shortest range, 1 — the longest.</param>
        /// <param name="ballRadius">Player ball radius once the shot has left it, meters.</param>
        /// <param name="shotRadius">Shot ball radius, meters.</param>
        public LobTarget Find(Vector3 direction, float rangeShare, float ballRadius, float shotRadius)
        {
            Vector3 launchCenter = _ballGroundPoint + direction * (ballRadius + shotRadius) + Vector3.up * shotRadius;
            float nearest = ballRadius + shotRadius * 2f + MinLandingGap;
            float range = Mathf.Max(Mathf.Min(GetRange(rangeShare), GetDistanceToFieldEdge(direction)), nearest);

            Vector3 landingCenter = _ballGroundPoint + direction * range + Vector3.up * shotRadius;
            float apex = Mathf.Max(_settings.MinApexHeight, range * _settings.ApexHeightPerMeter);
            return new LobTarget(new ArcPath(launchCenter, landingCenter, apex), landingCenter);
        }

        /// <summary>How far the ball can look along the direction before leaving the field.</summary>
        public float GetDistanceToFieldEdge(Vector3 direction)
        {
            float distance = float.PositiveInfinity;
            distance = Mathf.Min(distance, GetDistanceToBounds(_ballGroundPoint.x, direction.x, _field.xMin, _field.xMax));
            distance = Mathf.Min(distance, GetDistanceToBounds(_ballGroundPoint.z, direction.z, _field.yMin, _field.yMax));
            return Mathf.Max(0f, distance);
        }

        // Distance along one axis to whichever bound the direction heads for.
        private static float GetDistanceToBounds(float origin, float step, float min, float max)
        {
            if (Mathf.Abs(step) <= Mathf.Epsilon)
            {
                return float.PositiveInfinity;
            }

            float bound = step > 0f ? max : min;
            return (bound - origin) / step;
        }
    }
}

using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Gameplay.Hose
{
    /// <summary>
    /// One drop of the hose in the air. It keeps the arc it left along, so the stream bends after the finger.
    /// </summary>
    public readonly struct WaterDrop
    {
        public WaterDrop(ArcPath arc, float radius, float flightSeconds, float elapsedSeconds)
        {
            Arc = arc;
            Radius = radius;
            FlightSeconds = flightSeconds;
            ElapsedSeconds = elapsedSeconds;
        }

        public ArcPath Arc { get; }

        /// <summary>Radius of the drop, meters.</summary>
        public float Radius { get; }

        /// <summary>How long the whole arc takes.</summary>
        public float FlightSeconds { get; }

        public float ElapsedSeconds { get; }

        /// <summary>0 — just left the ball, 1 — on the ground.</summary>
        public float Share => FlightSeconds > 0f ? Mathf.Clamp01(ElapsedSeconds / FlightSeconds) : 1f;

        public Vector3 Center => Arc.Evaluate(Share);

        /// <summary>Unit direction of the flight right now.</summary>
        public Vector3 Direction => Arc.GetDirection(Share);

        public bool HasLanded => ElapsedSeconds >= FlightSeconds;

        public WaterDrop Advance(float deltaTime) => new WaterDrop(Arc, Radius, FlightSeconds, ElapsedSeconds + deltaTime);
    }
}

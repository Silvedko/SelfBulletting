using UnityEngine;

namespace SelfBulletting.Gameplay.Scanning
{
    /// <summary>
    /// Moves a sphere along a straight piece and reports the first obstacle it touches. The Hose drops fly their arcs
    /// piece by piece through it; tests replace the physics with a fake.
    /// </summary>
    public interface IObstacleSweep
    {
        /// <param name="contactCenter">Where the sphere centre is when it touches the obstacle; <paramref name="to"/> when nothing is hit.</param>
        /// <param name="obstacle">The collider touched, or null.</param>
        bool TrySweep(Vector3 from, Vector3 to, float radius, out Vector3 contactCenter, out Collider obstacle);
    }
}

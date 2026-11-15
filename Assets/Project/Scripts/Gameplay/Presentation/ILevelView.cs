using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// Everything in the Game scene that all levels share but that depends on the loaded one: the door at the finish,
    /// the obstacle effects, the camera framing. Called once, right after the level is loaded.
    /// </summary>
    public interface ILevelView
    {
        /// <param name="finishDirection">Flat direction in which the path reaches the finish: the door faces against it.</param>
        void Present(Vector3 startGroundPoint, Vector3 finishGroundPoint, Vector3 finishDirection, Transform obstaclesRoot);
    }
}

using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Views.Framing;
using SelfBulletting.Views.Obstacles;
using SelfBulletting.Views.Water;
using UnityEngine;

namespace SelfBulletting.Views.Levels
{
    /// <summary>
    /// Fits the shared Game scene to the loaded level: moves the finish building to the level finish and turns it
    /// to face the path, hands the steam and water effects to the fire views and frames the camera on the new layout.
    /// </summary>
    public sealed class LevelPresenter : MonoBehaviour, ILevelView
    {
        [Tooltip("Root of the finish building with the door and the pin; its forward axis looks along the path.")]
        [SerializeField] private Transform _finish;

        [SerializeField] private CameraFraming _framing;

        [SerializeField] private SteamPuffEffect _steamPuff;
        [SerializeField] private WaterSplashEffect _waterSplash;

        public void Present(Vector3 startGroundPoint, Vector3 finishGroundPoint, Transform obstaclesRoot)
        {
            Vector3 pathDirection = finishGroundPoint - startGroundPoint;
            pathDirection.y = 0f;
            _finish.SetPositionAndRotation(finishGroundPoint, Quaternion.LookRotation(pathDirection.normalized, Vector3.up));

            foreach (ObstacleView view in obstaclesRoot.GetComponentsInChildren<ObstacleView>(includeInactive: true))
            {
                view.Construct(_steamPuff, _waterSplash);
            }

            _framing.FrameLevel(startGroundPoint);
        }
    }
}

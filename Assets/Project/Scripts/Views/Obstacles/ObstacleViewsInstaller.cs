using UnityEngine;

namespace SelfBulletting.Views.Obstacles
{
    /// <summary>
    /// Hands the shared explosion effect to every obstacle view of the level.
    /// Obstacles are generated in the editor, so they are collected from their root when the scene starts.
    /// </summary>
    public sealed class ObstacleViewsInstaller : MonoBehaviour
    {
        [SerializeField] private Transform _obstaclesRoot;
        [SerializeField] private ExplosionEffect _explosion;

        private void Awake()
        {
            foreach (ObstacleView view in _obstaclesRoot.GetComponentsInChildren<ObstacleView>(includeInactive: true))
            {
                view.Construct(_explosion);
            }
        }
    }
}

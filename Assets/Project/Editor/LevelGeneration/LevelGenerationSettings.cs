using SelfBulletting.Gameplay.Obstacles;
using UnityEngine;

namespace SelfBulletting.Editor.LevelGeneration
{
    /// <summary>
    /// Editor-only settings of the obstacle generator. Change them and run SelfBulletting → Level → Generate Obstacles.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(LevelGenerationSettings), menuName = "SelfBulletting/Level Generation Settings")]
    public sealed class LevelGenerationSettings : ScriptableObject
    {
        [Tooltip("Prefab with an Obstacle component on its root.")]
        [SerializeField] private GameObject _obstaclePrefab;

        [SerializeField] private int _seed = 1;

        [Tooltip("Ground rectangle filled with obstacles: x/width is the world X, y/height is the world Z.")]
        [SerializeField] private Rect _area = new Rect(0f, 0f, 12f, 24f);

        [Tooltip("One obstacle per cell. Smaller cells give a denser forest.")]
        [SerializeField, Min(0.3f)] private float _cellSize = 0.9f;

        [Tooltip("How far an obstacle can move from its cell centre.")]
        [SerializeField, Min(0f)] private float _jitter = 0.3f;

        [SerializeField, Min(0f)] private float _startClearRadius = 2.5f;
        [SerializeField, Min(0f)] private float _finishClearRadius = 3f;

        public GameObject ObstaclePrefab => _obstaclePrefab;

        public bool HasValidPrefab => _obstaclePrefab != null && _obstaclePrefab.GetComponent<Obstacle>() != null;

        public ObstacleLayoutParameters ToLayoutParameters() =>
            new ObstacleLayoutParameters(_seed, _area, _cellSize, _jitter, _startClearRadius, _finishClearRadius);
    }
}

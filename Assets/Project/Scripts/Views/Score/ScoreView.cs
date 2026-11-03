using System;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Presentation;
using UnityEngine;
using UnityEngine.Pool;

namespace SelfBulletting.Views.Score
{
    /// <summary>
    /// Floating score texts over the level: "+N" over every exploded obstacle and the combo label at the impact point.
    /// Labels come from a pool that lives and dies with the Game scene.
    /// </summary>
    public sealed class ScoreView : MonoBehaviour, IScoreView
    {
        [SerializeField] private FloatingText _textPrefab;

        [Tooltip("Labels turn to face this camera.")]
        [SerializeField] private Camera _camera;

        [SerializeField] private FloatingTextStyle _pointsStyle = new FloatingTextStyle();
        [SerializeField] private FloatingTextStyle _comboStyle = new FloatingTextStyle();

        [Tooltip("{0} — points.")]
        [SerializeField] private string _pointsFormat = "+{0}";

        [Tooltip("{0} — multiplier, {1} — bonus points.")]
        [SerializeField] private string _comboFormat = "COMBO x{0:0.#}\n+{1}";

        [Tooltip("Height of the points label above the obstacle base, meters.")]
        [SerializeField, Min(0f)] private float _pointsHeight = 1.8f;

        [Tooltip("Height of the combo label above the impact point, meters.")]
        [SerializeField, Min(0f)] private float _comboHeight = 2.5f;

        [SerializeField, Min(1)] private int _poolCapacity = 32;
        [SerializeField, Min(1)] private int _poolMaxSize = 256;

        private ObjectPool<FloatingText> _pool;
        private Action<FloatingText> _release;
        private int _formattedPoints = -1;
        private string _pointsText;
        private CancellationToken _destroyCancellationToken;

        public void ShowPoints(Vector3 worldPosition, int points)
        {
            Spawn(worldPosition + Vector3.up * _pointsHeight, GetPointsText(points), _pointsStyle);
        }

        public void ShowCombo(Vector3 worldPosition, float multiplier, int bonus)
        {
            string text = string.Format(CultureInfo.InvariantCulture, _comboFormat, multiplier, bonus);
            Spawn(worldPosition + Vector3.up * _comboHeight, text, _comboStyle);
        }

        private void Awake()
        {
            _destroyCancellationToken = gameObject.GetCancellationTokenOnDestroy();
            _release = Release;
            _pool = new ObjectPool<FloatingText>(Create, Activate, Deactivate, DestroyText,
                collectionCheck: false, _poolCapacity, _poolMaxSize);
        }

        private void Spawn(Vector3 position, string text, FloatingTextStyle style)
        {
            _pool.Get().Play(position, _camera.transform.rotation, text, style, _destroyCancellationToken);
        }

        // Every obstacle is worth the same, so the label is formatted once instead of on every explosion.
        private string GetPointsText(int points)
        {
            if (points != _formattedPoints)
            {
                _formattedPoints = points;
                _pointsText = string.Format(CultureInfo.InvariantCulture, _pointsFormat, points);
            }

            return _pointsText;
        }

        private FloatingText Create()
        {
            FloatingText text = Instantiate(_textPrefab, transform);
            text.Initialize(_release);
            return text;
        }

        private void Release(FloatingText text)
        {
            _pool.Release(text);
        }

        private static void Activate(FloatingText text)
        {
            text.gameObject.SetActive(true);
        }

        private static void Deactivate(FloatingText text)
        {
            text.gameObject.SetActive(false);
        }

        private static void DestroyText(FloatingText text)
        {
            if(text == null)
            {
                return;
            }
            Destroy(text.gameObject);
        }
    }
}

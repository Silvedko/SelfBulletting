using System;
using UnityEngine;

namespace SelfBulletting.Views.Score
{
    /// <summary>
    /// Look and motion of one kind of floating text.
    /// </summary>
    [Serializable]
    public sealed class FloatingTextStyle
    {
        [SerializeField] private Color _color = Color.white;
        [SerializeField, Min(0.1f)] private float _fontSize = 8f;

        [Tooltip("How high the text floats up, meters.")]
        [SerializeField] private float _rise = 1.2f;

        [SerializeField, Min(0.05f)] private float _durationSeconds = 0.8f;

        public Color Color => _color;
        public float FontSize => _fontSize;
        public float Rise => _rise;
        public float DurationSeconds => _durationSeconds;
    }
}

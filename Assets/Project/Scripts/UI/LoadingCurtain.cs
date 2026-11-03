using System;
using UnityEngine;

namespace SelfBulletting.UI
{
    /// <summary>
    /// Full-screen splash in the Boot scene. Covers every scene switch and shows load progress.
    /// </summary>
    public sealed class LoadingCurtain : MonoBehaviour, IProgress<float>
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _progressFill;

        public void Show()
        {
            Report(0f);
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        public void Report(float value)
        {
            Vector2 anchorMax = _progressFill.anchorMax;
            anchorMax.x = Mathf.Clamp01(value);
            _progressFill.anchorMax = anchorMax;
        }
    }
}

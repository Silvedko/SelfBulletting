using DG.Tweening;
using UnityEngine;

namespace SelfBulletting.Views.Finish
{
    /// <summary>
    /// Idle up-and-down bob, used by the pin marker over the finish.
    /// </summary>
    public sealed class BobbingMotion : MonoBehaviour
    {
        [Tooltip("How high the object rises above its start, meters.")]
        [SerializeField] private float _height = 0.35f;

        [SerializeField, Min(0.05f)] private float _halfPeriodSeconds = 0.7f;

        private void Start()
        {
            transform.DOLocalMoveY(transform.localPosition.y + _height, _halfPeriodSeconds)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }
    }
}

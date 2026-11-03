using DG.Tweening;
using SelfBulletting.Gameplay.Presentation;
using UnityEngine;

namespace SelfBulletting.Views.Finish
{
    /// <summary>
    /// The finish door: two leaves swing out towards the arriving ball.
    /// Hinges sit on the outer edges of the doorway; positive yaw swings the left leaf outwards.
    /// </summary>
    public sealed class DoorView : MonoBehaviour, IDoorView
    {
        [SerializeField] private Transform _leftHinge;
        [SerializeField] private Transform _rightHinge;
        [SerializeField, Range(0f, 180f)] private float _openAngle = 110f;
        [SerializeField, Min(0f)] private float _openSeconds = 0.45f;

        private bool _isOpen;

        public void Open()
        {
            if (_isOpen)
            {
                return;
            }

            _isOpen = true;
            Swing(_leftHinge, _openAngle);
            Swing(_rightHinge, -_openAngle);
        }

        private void Swing(Transform hinge, float yaw)
        {
            hinge.DOLocalRotate(new Vector3(0f, yaw, 0f), _openSeconds)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject);
        }
    }
}

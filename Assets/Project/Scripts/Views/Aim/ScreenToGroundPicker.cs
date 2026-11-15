using SelfBulletting.Gameplay.Presentation;
using UnityEngine;

namespace SelfBulletting.Views.Aim
{
    /// <summary>
    /// Casts a ray from the game camera through a screen point onto the ground plane.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class ScreenToGroundPicker : MonoBehaviour, IScreenToGround
    {
        [Tooltip("The ground the level lies on; its height is the plane height.")]
        [SerializeField] private Transform _ground;

        private Camera _camera;

        public float ScreenHeight => Screen.height;

        public bool TryGetGroundPoint(Vector2 screenPoint, out Vector3 groundPoint)
        {
            float height = _ground != null ? _ground.position.y : 0f;
            var plane = new Plane(Vector3.up, new Vector3(0f, height, 0f));
            Ray ray = _camera.ScreenPointToRay(screenPoint);
            if (plane.Raycast(ray, out float distance))
            {
                groundPoint = ray.GetPoint(distance);
                return true;
            }

            groundPoint = default;
            return false;
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }
    }
}

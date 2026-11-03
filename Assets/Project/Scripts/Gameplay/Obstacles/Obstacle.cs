using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Presentation.Null;
using UnityEngine;

namespace SelfBulletting.Gameplay.Obstacles
{
    /// <summary>
    /// Gameplay side of an obstacle: its collider blocks the path until it explodes.
    /// Needs a collider on the same object. The look comes from an optional <see cref="IObstacleView"/> on the same object.
    /// </summary>
    public sealed class Obstacle : MonoBehaviour
    {
        private Collider _collider;
        private IObstacleView _view;

        public Collider Collider => _collider;
        public bool IsDestroyed { get; private set; }

        public void Infect()
        {
            _view.ShowInfected();
        }

        public async UniTask ExplodeAsync(CancellationToken cancellationToken)
        {
            IsDestroyed = true;
            _collider.enabled = false;
            await _view.PlayExplosionAsync(cancellationToken);
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _view = TryGetComponent(out IObstacleView view) ? view : new NullObstacleView();
        }
    }
}

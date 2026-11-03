using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    [Serializable]
    public sealed class PhysicsSettings
    {
        [Tooltip("Only obstacles are scanned: the ground, the door and the shot live on other layers.")]
        [SerializeField] private LayerMask _obstacleMask;

        [Tooltip("Size of the preallocated buffers for SphereCastNonAlloc and OverlapSphereNonAlloc.")]
        [SerializeField, Min(16)] private int _queryBufferSize = 256;

        public PhysicsSettings()
        {
        }

        public PhysicsSettings(LayerMask obstacleMask, int queryBufferSize)
        {
            _obstacleMask = obstacleMask;
            _queryBufferSize = queryBufferSize;
        }

        public LayerMask ObstacleMask => _obstacleMask;
        public int QueryBufferSize => _queryBufferSize;
    }
}

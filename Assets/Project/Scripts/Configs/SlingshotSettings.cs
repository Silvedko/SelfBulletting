using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    /// <summary>
    /// Slingshot mode: how far the finger is pulled back, as a share of the screen height.
    /// </summary>
    [Serializable]
    public sealed class SlingshotSettings
    {
        [Tooltip("A pull shorter than this is no aim: releasing there cancels the shot. Share of the screen height.")]
        [SerializeField, Range(0f, 0.5f)] private float _deadZoneScreenShare = 0.05f;

        [Tooltip("A pull this long sends the shot to the longest range. Share of the screen height.")]
        [SerializeField, Range(0.05f, 1f)] private float _fullPullScreenShare = 0.4f;

        public SlingshotSettings()
        {
        }

        public SlingshotSettings(float deadZoneScreenShare, float fullPullScreenShare)
        {
            _deadZoneScreenShare = deadZoneScreenShare;
            _fullPullScreenShare = fullPullScreenShare;
            Validate();
        }

        public float DeadZoneScreenShare => _deadZoneScreenShare;
        public float FullPullScreenShare => _fullPullScreenShare;

        public void Validate()
        {
            _fullPullScreenShare = Mathf.Max(_fullPullScreenShare, _deadZoneScreenShare + 0.01f);
        }
    }
}

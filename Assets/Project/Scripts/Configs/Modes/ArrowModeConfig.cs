using SelfBulletting.Gameplay.Modes;
using UnityEngine;

namespace SelfBulletting.Configs.Modes
{
    /// <summary>
    /// Arrow: the swinging arrow sets the direction, the running landing point the range; the shot cost is fixed.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(ArrowModeConfig), menuName = "SelfBulletting/Modes/Arrow")]
    public sealed class ArrowModeConfig : ShotModeConfig
    {
        [SerializeField] private LobSettings _lob = new LobSettings();
        [SerializeField] private ArrowSettings _arrow = new ArrowSettings();

        public override GameMode Mode => GameMode.Arrow;
        public LobSettings Lob => _lob;
        public ArrowSettings Arrow => _arrow;

        protected override void OnValidate()
        {
            _lob.Validate();
            base.OnValidate();
        }
    }
}

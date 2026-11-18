using SelfBulletting.Gameplay.Modes;
using UnityEngine;

namespace SelfBulletting.Configs.Modes
{
    /// <summary>
    /// Slingshot: the aim scale sets the power, the pull sets the direction and the range of the lobbed shot.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(SlingshotModeConfig), menuName = "SelfBulletting/Modes/Slingshot")]
    public sealed class SlingshotModeConfig : ShotModeConfig
    {
        [SerializeField] private AimSettings _aim = new AimSettings();
        [SerializeField] private LobSettings _lob = new LobSettings();
        [SerializeField] private SlingshotSettings _slingshot = new SlingshotSettings();

        public override GameMode Mode => GameMode.Slingshot;
        public AimSettings Aim => _aim;
        public LobSettings Lob => _lob;
        public SlingshotSettings Slingshot => _slingshot;

        protected override void OnValidate()
        {
            _lob.Validate();
            _slingshot.Validate();
            base.OnValidate();
        }
    }
}

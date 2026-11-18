using SelfBulletting.Gameplay.Modes;
using UnityEngine;

namespace SelfBulletting.Configs.Modes
{
    /// <summary>
    /// Hose: the slingshot pull aims a stream of drops along lobbed arcs; there are no shots, no infection and no combo.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(HoseModeConfig), menuName = "SelfBulletting/Modes/Hose")]
    public sealed class HoseModeConfig : ModeConfig
    {
        [SerializeField] private HoseSettings _hose = new HoseSettings();
        [SerializeField] private LobSettings _lob = new LobSettings();
        [SerializeField] private SlingshotSettings _slingshot = new SlingshotSettings();

        public override GameMode Mode => GameMode.Hose;
        public override float CriticalPercent => _hose.CriticalPercent;
        public HoseSettings Hose => _hose;
        public LobSettings Lob => _lob;
        public SlingshotSettings Slingshot => _slingshot;

        protected override void OnValidate()
        {
            _hose.Validate();
            _lob.Validate();
            _slingshot.Validate();
            base.OnValidate();
        }
    }
}

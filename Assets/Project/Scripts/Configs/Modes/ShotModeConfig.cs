using UnityEngine;

namespace SelfBulletting.Configs.Modes
{
    /// <summary>
    /// The modes that fire one shot at a time: the shot economy, the infection wave it starts and the combo it earns.
    /// The cost of the minimum shot is also the critical ball size.
    /// </summary>
    public abstract class ShotModeConfig : ModeConfig
    {
        [SerializeField] private ShotSettings _shot = new ShotSettings();
        [SerializeField] private InfectionSettings _infection = new InfectionSettings();
        [SerializeField] private ComboSettings _combo = new ComboSettings();

        public ShotSettings Shot => _shot;
        public InfectionSettings Infection => _infection;
        public override ComboSettings Combo => _combo;
        public override float CriticalPercent => _shot.MinShootPercent;

        protected override void OnValidate()
        {
            _shot.Validate();
            _combo.Validate();
            base.OnValidate();
        }
    }
}

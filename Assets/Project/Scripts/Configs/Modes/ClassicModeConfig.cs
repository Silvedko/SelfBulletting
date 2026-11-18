using SelfBulletting.Gameplay.Modes;
using UnityEngine;

namespace SelfBulletting.Configs.Modes
{
    /// <summary>
    /// Classic: the hold drives the ping-pong aim scale, the shot rolls along the straight path.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(ClassicModeConfig), menuName = "SelfBulletting/Modes/Classic")]
    public sealed class ClassicModeConfig : ShotModeConfig
    {
        [SerializeField] private AimSettings _aim = new AimSettings();

        public override GameMode Mode => GameMode.Classic;
        public AimSettings Aim => _aim;
    }
}

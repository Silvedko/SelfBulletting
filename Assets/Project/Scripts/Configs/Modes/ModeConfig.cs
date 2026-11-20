using SelfBulletting.Gameplay.Modes;
using UnityEngine;

namespace SelfBulletting.Configs.Modes
{
    /// <summary>
    /// Everything one game mode tunes for itself. Every mode has its own asset with only the sections it uses;
    /// the values the whole game shares stay in <see cref="SettingsConfig"/>.
    /// </summary>
    public abstract class ModeConfig : ScriptableObject
    {
        [SerializeField] private ScoreSettings _score = new ScoreSettings();
        [SerializeField] private FeedbackSettings _feedback = new FeedbackSettings();

        public abstract GameMode Mode { get; }

        /// <summary>The ball never gets smaller than this, % of R0.</summary>
        public abstract float CriticalPercent { get; }

        public ScoreSettings Score => _score;
        public FeedbackSettings Feedback => _feedback;

        /// <summary>Combo tiers of the mode; none unless the mode shoots.</summary>
        public virtual ComboSettings Combo => ComboSettings.None;

        /// <summary>Bonus for fires put out together; none unless the mode puts fires out one by one (the Hose).</summary>
        public virtual ExtinguishBonusSettings ExtinguishBonus => ExtinguishBonusSettings.None;

        protected virtual void OnValidate()
        {
            _feedback.Validate(CriticalPercent);
        }
    }
}

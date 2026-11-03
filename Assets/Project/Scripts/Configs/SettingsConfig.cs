using UnityEngine;

namespace SelfBulletting.Configs
{
    /// <summary>
    /// All tunable values of the game. Every class receives only the section it needs.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(SettingsConfig), menuName = "SelfBulletting/Settings Config")]
    public sealed class SettingsConfig : ScriptableObject
    {
        [SerializeField] private ApplicationSettings _application = new ApplicationSettings();
        [SerializeField] private LoadingSettings _loading = new LoadingSettings();
        [SerializeField] private BallSettings _ball = new BallSettings();
        [SerializeField] private AimSettings _aim = new AimSettings();
        [SerializeField] private ShotSettings _shot = new ShotSettings();
        [SerializeField] private InfectionSettings _infection = new InfectionSettings();
        [SerializeField] private ScoreSettings _score = new ScoreSettings();
        [SerializeField] private FeedbackSettings _feedback = new FeedbackSettings();
        [SerializeField] private FinishSettings _finish = new FinishSettings();

        public ApplicationSettings Application => _application;
        public LoadingSettings Loading => _loading;
        public BallSettings Ball => _ball;
        public AimSettings Aim => _aim;
        public ShotSettings Shot => _shot;
        public InfectionSettings Infection => _infection;
        public ScoreSettings Score => _score;
        public FeedbackSettings Feedback => _feedback;
        public FinishSettings Finish => _finish;

        private void OnValidate()
        {
            _shot.Validate();
            _score.Validate();
            _feedback.Validate(_shot.MinShootPercent);
        }
    }
}

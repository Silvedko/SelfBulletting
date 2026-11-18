using UnityEngine;

namespace SelfBulletting.Configs
{
    /// <summary>
    /// Values shared by the whole game. Everything a game mode tunes for itself lives in its own mode config
    /// (<see cref="Modes.ModeConfig"/>). Every class receives only the section it needs.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(SettingsConfig), menuName = "SelfBulletting/Settings Config")]
    public sealed class SettingsConfig : ScriptableObject
    {
        [SerializeField] private ApplicationSettings _application = new ApplicationSettings();
        [SerializeField] private LoadingSettings _loading = new LoadingSettings();
        [SerializeField] private BallSettings _ball = new BallSettings();
        [SerializeField] private FinishSettings _finish = new FinishSettings();
        [SerializeField] private PhysicsSettings _physics = new PhysicsSettings();

        public ApplicationSettings Application => _application;
        public LoadingSettings Loading => _loading;
        public BallSettings Ball => _ball;
        public FinishSettings Finish => _finish;
        public PhysicsSettings Physics => _physics;
    }
}

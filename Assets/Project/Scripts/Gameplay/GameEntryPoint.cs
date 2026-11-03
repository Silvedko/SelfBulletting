using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Aim;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Feedback;
using SelfBulletting.Gameplay.Finish;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Gameplay.Obstacles;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Presentation.Null;
using SelfBulletting.Gameplay.Rules;
using SelfBulletting.Gameplay.Scanning;
using SelfBulletting.Gameplay.Score;
using SelfBulletting.Gameplay.Shot;
using SelfBulletting.UI.Final;
using UnityEngine;

namespace SelfBulletting.Gameplay
{
    /// <summary>
    /// Composition root of the Game scene: builds the round from the app services, the level and the views.
    /// </summary>
    public sealed class GameEntryPoint : MonoBehaviour
    {
        [SerializeField] private FinalScreen _finalScreen;
        [SerializeField] private Level _level;

        [Header("Views: optional, each must implement its interface")]
        [SerializeField] private MonoBehaviour _ballView;
        [SerializeField] private MonoBehaviour _shotView;
        [SerializeField] private MonoBehaviour _pathView;
        [SerializeField] private MonoBehaviour _aimView;
        [SerializeField] private MonoBehaviour _scoreView;
        [SerializeField] private MonoBehaviour _doorView;
        [SerializeField] private MonoBehaviour _healthView;

        private BallModel _ball;
        private GameViews _views;
        private GameLoop _loop;

        public FinalScreen FinalScreen => _finalScreen;

        public void Construct(GameDependencies dependencies)
        {
            SettingsConfig settings = dependencies.Settings;

            _finalScreen.Hide();
            _level.Initialize();
            _views = CreateViews();

            var economy = new ShotEconomy(settings.Ball, settings.Shot);
            _ball = new BallModel(economy);

            var danger = new DangerFeedback(settings.Feedback, settings.Shot, dependencies.Vibration, dependencies.Audio);
            var aim = new AimController(dependencies.Input, economy, new AimScale(settings.Aim), _ball, danger, _views, dependencies.Audio);
            var cascade = new ObstacleCascade(new ObstacleScanner(settings.Physics), _level, settings.Infection, dependencies.Audio);
            var winSequence = new WinSequence(settings.Finish, _views, dependencies.Audio);

            _loop = new GameLoop(_level, _ball, economy, new RoundRules(economy), new PathScanner(settings.Physics),
                aim, cascade, new ScoreCounter(settings.Score), winSequence, _views, dependencies.Audio);

            _views.Ball.Place(_level.StartPoint, _ball.WorldRadius);
            _views.Path.SetPath(_level.StartPoint, _level.FinishPoint);
            _views.Path.SetWidth(_ball.WorldRadius);
            _views.Health.Show(_ball.RadiusPercent, economy.GetHealth(_ball.RadiusPercent));
            _ball.RadiusChanged += ShowBallSize;
        }

        public async UniTask<GameResult> PlayRoundAsync(CancellationToken cancellationToken)
        {
            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, destroyCancellationToken))
            {
                return await _loop.RunAsync(linked.Token);
            }
        }

        private void OnDestroy()
        {
            if (_ball != null)
            {
                _ball.RadiusChanged -= ShowBallSize;
            }
        }

        private void OnValidate()
        {
            _ballView = KeepIfImplements<IBallView>(_ballView);
            _shotView = KeepIfImplements<IShotView>(_shotView);
            _pathView = KeepIfImplements<IPathView>(_pathView);
            _aimView = KeepIfImplements<IAimView>(_aimView);
            _scoreView = KeepIfImplements<IScoreView>(_scoreView);
            _doorView = KeepIfImplements<IDoorView>(_doorView);
            _healthView = KeepIfImplements<IBallHealthView>(_healthView);
        }

        private void ShowBallSize()
        {
            _views.Ball.SetRadius(_ball.WorldRadius);
            _views.Path.SetWidth(_ball.WorldRadius);
        }

        private GameViews CreateViews() => new GameViews(
            Resolve<IBallView>(_ballView) ?? new NullBallView(),
            Resolve<IShotView>(_shotView) ?? new NullShotView(),
            Resolve<IPathView>(_pathView) ?? new NullPathView(),
            Resolve<IAimView>(_aimView) ?? new NullAimView(),
            Resolve<IScoreView>(_scoreView) ?? new NullScoreView(),
            Resolve<IDoorView>(_doorView) ?? new NullDoorView(),
            Resolve<IBallHealthView>(_healthView) ?? new NullBallHealthView());

        private static TView Resolve<TView>(MonoBehaviour component) where TView : class =>
            component != null ? component as TView : null;

        private static MonoBehaviour KeepIfImplements<TView>(MonoBehaviour component) where TView : class
        {
            if (component == null || component is TView)
            {
                return component;
            }

            Debug.LogWarning($"{component.GetType().Name} on {component.name} does not implement {typeof(TView).Name}.", component);
            return null;
        }
    }
}

using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Configs;
using SelfBulletting.Configs.Modes;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Feedback;
using SelfBulletting.Gameplay.Finish;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Gameplay.Modes;
using SelfBulletting.Gameplay.Paths;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Presentation.Null;
using SelfBulletting.Gameplay.Rules;
using SelfBulletting.Gameplay.Scanning;
using SelfBulletting.Gameplay.Score;
using SelfBulletting.Gameplay.Strategies;
using SelfBulletting.UI.Final;
using UnityEngine;

namespace SelfBulletting.Gameplay
{
    /// <summary>
    /// Composition root of the Game scene: loads the level prefab and builds the round for the chosen game mode from
    /// the app services, the mode config, the level and the views. The scene itself (camera, ground, UI, views) is shared
    /// by every level and every mode; only the path and the turn differ.
    /// </summary>
    public sealed class GameEntryPoint : MonoBehaviour
    {
        [SerializeField] private FinalScreen _finalScreen;

        [Tooltip("The level prefab is instantiated under this object.")]
        [SerializeField] private Transform _levelRoot;

        [Header("Views: optional, each must implement its interface")]
        [SerializeField] private MonoBehaviour _ballView;
        [SerializeField] private MonoBehaviour _shotView;
        [SerializeField] private MonoBehaviour _pathView;
        [SerializeField] private MonoBehaviour _aimView;
        [SerializeField] private MonoBehaviour _scoreView;
        [SerializeField] private MonoBehaviour _doorView;
        [SerializeField] private MonoBehaviour _healthView;
        [SerializeField] private MonoBehaviour _levelView;
        [SerializeField] private MonoBehaviour _lobAimView;
        [SerializeField] private MonoBehaviour _hoseView;

        [Tooltip("Maps the finger to the ground for the slingshot pull. Must implement IScreenToGround.")]
        [SerializeField] private MonoBehaviour _screenToGround;

        private Level _level;
        private BallModel _ball;
        private GameViews _views;
        private GameLoop _loop;

        public FinalScreen FinalScreen => _finalScreen;

        public void Construct(GameDependencies dependencies, Level levelPrefab, ModeConfig mode)
        {
            SettingsConfig settings = dependencies.Settings;

            _finalScreen.Hide();
            _level = Instantiate(levelPrefab, _levelRoot);
            _level.Initialize();
            _views = CreateViews();

            GroundPath path = mode.Mode.UsesCurvedPath() ? _level.CreateCurvedPath() : _level.CreateStraightPath();
            _views.Level.Present(_level.StartPoint, _level.FinishPoint, path.FinishDirection, _level.ObstaclesRoot);

            var limits = new BallLimits(settings.Ball.StartRadius, mode.CriticalPercent);
            _ball = new BallModel(limits);

            var pathScanner = new PathScanner(settings.Physics);
            var pathCheck = new PathCheck(path, pathScanner, _ball);
            var score = new ScoreCounter(mode.Score, mode.Combo, dependencies.Bonus.HasExtinguishBonus(mode.Mode));
            var danger = new DangerFeedback(mode.Feedback, mode.CriticalPercent, dependencies.Vibration, dependencies.Audio);
            IScreenToGround screen = Resolve<IScreenToGround>(_screenToGround) ?? new NullScreenToGround();
            var strategies = new ShotStrategyFactory(dependencies, screen, _level, _ball, limits, pathScanner,
                new ObstacleScanner(settings.Physics), pathCheck, danger, score, _views);
            var winSequence = new WinSequence(settings.Finish, _views, dependencies.Audio);

            _loop = new GameLoop(path, _ball, new RoundRules(limits), pathCheck, strategies.Create(mode), score,
                winSequence, dependencies.Audio);

            _views.Ball.Place(_level.StartPoint, _ball.WorldRadius);
            _views.Path.SetPath(path.Points);
            _views.Path.SetWidth(_ball.WorldRadius);
            _views.Health.Show(_ball.RadiusPercent, _ball.Health);
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
            _levelView = KeepIfImplements<ILevelView>(_levelView);
            _lobAimView = KeepIfImplements<ILobAimView>(_lobAimView);
            _hoseView = KeepIfImplements<IHoseView>(_hoseView);
            _screenToGround = KeepIfImplements<IScreenToGround>(_screenToGround);
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
            Resolve<IBallHealthView>(_healthView) ?? new NullBallHealthView(),
            Resolve<ILevelView>(_levelView) ?? new NullLevelView(),
            Resolve<ILobAimView>(_lobAimView) ?? new NullLobAimView(),
            Resolve<IHoseView>(_hoseView) ?? new NullHoseView());

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

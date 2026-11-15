using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Aim;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Feedback;
using SelfBulletting.Gameplay.Finish;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Gameplay.Modes;
using SelfBulletting.Gameplay.Obstacles;
using SelfBulletting.Gameplay.Paths;
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
    /// Composition root of the Game scene: loads the level prefab and builds the round for the chosen game mode from
    /// the app services, the level and the views. The scene itself (camera, ground, UI, views) is shared by every level
    /// and every mode; only the path and the aiming differ.
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

        [Tooltip("Maps the finger to the ground for the slingshot. Must implement IScreenToGround.")]
        [SerializeField] private MonoBehaviour _screenToGround;

        private Level _level;
        private BallModel _ball;
        private GameViews _views;
        private GameLoop _loop;

        public FinalScreen FinalScreen => _finalScreen;

        public void Construct(GameDependencies dependencies, Level levelPrefab, GameMode mode)
        {
            SettingsConfig settings = dependencies.Settings;

            _finalScreen.Hide();
            _level = Instantiate(levelPrefab, _levelRoot);
            _level.Initialize();
            _views = CreateViews();

            GroundPath path = mode.IsAimed() ? _level.CreateCurvedPath() : _level.CreateStraightPath();
            _views.Level.Present(_level.StartPoint, _level.FinishPoint, path.FinishDirection, _level.ObstaclesRoot);

            var economy = new ShotEconomy(settings.Ball, settings.Shot);
            _ball = new BallModel(economy);

            var pathScanner = new PathScanner(settings.Physics);
            var obstacleScanner = new ObstacleScanner(settings.Physics);
            var danger = new DangerFeedback(settings.Feedback, settings.Shot, dependencies.Vibration, dependencies.Audio);
            var charge = new ChargePreview(economy, _ball, danger, _views);
            IShotAiming aim = CreateAiming(mode, settings, dependencies, economy, charge, pathScanner, obstacleScanner);
            var cascade = new ObstacleCascade(obstacleScanner, _level, settings.Infection, dependencies.Audio);
            var winSequence = new WinSequence(settings.Finish, _views, dependencies.Audio);

            _loop = new GameLoop(_level, path, _ball, economy, new RoundRules(economy), pathScanner,
                aim, cascade, new ScoreCounter(settings.Score), winSequence, _views, dependencies.Audio);

            _views.Ball.Place(_level.StartPoint, _ball.WorldRadius);
            _views.Path.SetPath(path.Points);
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

        private IShotAiming CreateAiming(GameMode mode, SettingsConfig settings, GameDependencies dependencies,
            ShotEconomy economy, ChargePreview charge, PathScanner pathScanner, ObstacleScanner obstacleScanner)
        {
            var scale = new AimScale(settings.Aim);
            if (!mode.IsAimed())
            {
                var targeting = new ShotTargeting(pathScanner, _level.StartPoint, _level.FinishPoint);
                return new ClassicAimController(dependencies.Input, economy, scale, targeting, _ball, charge, _views,
                    dependencies.Audio);
            }

            var lobTargeting = new LobTargeting(settings.Lob, _level.StartPoint, _level.Field);
            var preview = new LobPreview(economy, lobTargeting, charge, new TargetHighlight(obstacleScanner, _level), _views);
            if (mode == GameMode.Slingshot)
            {
                IScreenToGround screen = Resolve<IScreenToGround>(_screenToGround) ?? new NullScreenToGround();
                return new SlingshotAimController(dependencies.Input, screen, new SlingshotPull(settings.Slingshot), economy,
                    scale, _ball, preview, _views, dependencies.Audio);
            }

            return new ArrowAimController(dependencies.Input, new ArrowSwing(settings.Arrow), economy, _ball, preview,
                _level.StartPoint, _views, dependencies.Audio);
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
            Resolve<ILobAimView>(_lobAimView) ?? new NullLobAimView());

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

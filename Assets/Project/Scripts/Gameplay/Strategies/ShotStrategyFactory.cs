using System;
using SelfBulletting.Configs;
using SelfBulletting.Configs.Modes;
using SelfBulletting.Gameplay.Aim;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Feedback;
using SelfBulletting.Gameplay.Hose;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Gameplay.Obstacles;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Scanning;
using SelfBulletting.Gameplay.Score;
using SelfBulletting.Gameplay.Shot;
using SelfBulletting.Infrastructure.Services.Audio;
using SelfBulletting.Infrastructure.Services.Bonus;
using SelfBulletting.Infrastructure.Services.Input;

namespace SelfBulletting.Gameplay.Strategies
{
    /// <summary>
    /// Builds the turn of a game mode from its config and the pieces every mode shares. Part of the Game scene's
    /// composition root: every mode-specific object is created here and nowhere else.
    /// </summary>
    public sealed class ShotStrategyFactory
    {
        private readonly IInputService _input;
        private readonly IAudioService _audio;
        private readonly IBonusService _bonus;
        private readonly IScreenToGround _screen;
        private readonly Level _level;
        private readonly BallModel _ball;
        private readonly BallLimits _limits;
        private readonly PathScanner _pathScanner;
        private readonly ObstacleScanner _obstacleScanner;
        private readonly PathCheck _pathCheck;
        private readonly DangerFeedback _danger;
        private readonly ScoreCounter _score;
        private readonly GameViews _views;

        public ShotStrategyFactory(GameDependencies dependencies, IScreenToGround screen, Level level, BallModel ball,
            BallLimits limits, PathScanner pathScanner, ObstacleScanner obstacleScanner, PathCheck pathCheck,
            DangerFeedback danger, ScoreCounter score, GameViews views)
        {
            _input = dependencies.Input;
            _audio = dependencies.Audio;
            _bonus = dependencies.Bonus;
            _screen = screen;
            _level = level;
            _ball = ball;
            _limits = limits;
            _pathScanner = pathScanner;
            _obstacleScanner = obstacleScanner;
            _pathCheck = pathCheck;
            _danger = danger;
            _score = score;
            _views = views;
        }

        public IShotStrategy Create(ModeConfig config)
        {
            switch (config)
            {
                case ClassicModeConfig classic:
                    return CreateClassic(classic);
                case SlingshotModeConfig slingshot:
                    return CreateSlingshot(slingshot);
                case ArrowModeConfig arrow:
                    return CreateArrow(arrow);
                case HoseModeConfig hose:
                    return CreateHose(hose);
                default:
                    throw new ArgumentOutOfRangeException(nameof(config), config, "No turn for this mode config.");
            }
        }

        private IShotStrategy CreateClassic(ClassicModeConfig config)
        {
            ShotEconomy economy = CreateEconomy(config);
            var targeting = new ShotTargeting(_pathScanner, _level.StartPoint, _level.FinishPoint);
            var aim = new ClassicAimController(_input, economy, new AimScale(config.Aim), targeting, _ball,
                CreateCharge(economy), _views, _audio);
            return CreateReleasedShot(config, economy, aim);
        }

        private IShotStrategy CreateSlingshot(SlingshotModeConfig config)
        {
            ShotEconomy economy = CreateEconomy(config);
            var aim = new SlingshotAimController(_input, _screen, new SlingshotPull(config.Slingshot), economy,
                new AimScale(config.Aim), _ball, CreateLobPreview(economy, config.Lob), _views, _audio);
            return CreateReleasedShot(config, economy, aim);
        }

        private IShotStrategy CreateArrow(ArrowModeConfig config)
        {
            ShotEconomy economy = CreateEconomy(config);
            var aim = new ArrowAimController(_input, new ArrowSwing(config.Arrow), config.Arrow.ShotCostPercent,
                config.Arrow.DamageRadiusPercent, economy, _ball, CreateLobPreview(economy, config.Lob), _level.StartPoint,
                _views, _audio);
            return CreateReleasedShot(config, economy, aim);
        }

        private IShotStrategy CreateHose(HoseModeConfig config)
        {
            HoseSettings hose = config.Hose;
            var size = new DropSize(hose, _limits);
            var stream = new HoseStream(new HoseFlow(hose), new HoseDrops(hose, size, _pathScanner), size,
                CreateLobTargeting(config.Lob), _ball);
            var preview = new HosePreview(_pathScanner, size, CreateHighlight(), _views.LobAim, hose.PreviewSegments);
            var splash = new DropSplash(_obstacleScanner, _level, size, new HitCounter<Obstacle>(hose.HitsToExtinguish));
            var aim = new HoseAim(_input, _screen, new SlingshotPull(config.Slingshot));
            var burst = new ExtinguishBurst(_bonus, config.Mode);

            _views.Hose.Prepare(hose.MaxDropsInFlight);
            return new HoseStrategy(aim, stream, preview, splash, burst, _pathCheck, _ball, _danger, _score, _views,
                _audio, _level.StartPoint.y);
        }

        private IShotStrategy CreateReleasedShot(ShotModeConfig config, ShotEconomy economy, IShotAiming aim)
        {
            var cascade = new ObstacleCascade(_obstacleScanner, _level, config.Infection, _audio);
            return new ReleasedShotStrategy(_level, _ball, economy, aim, cascade, _score, _views, _audio);
        }

        private ShotEconomy CreateEconomy(ShotModeConfig config) => new ShotEconomy(_limits, config.Shot);

        private ChargePreview CreateCharge(ShotEconomy economy) => new ChargePreview(economy, _ball, _danger, _views);

        private LobPreview CreateLobPreview(ShotEconomy economy, LobSettings lob) =>
            new LobPreview(economy, CreateLobTargeting(lob), CreateCharge(economy), CreateHighlight(), _views);

        private LobTargeting CreateLobTargeting(LobSettings lob) => new LobTargeting(lob, _level.StartPoint, _level.Field);

        private TargetHighlight CreateHighlight() => new TargetHighlight(_obstacleScanner, _level);
    }
}

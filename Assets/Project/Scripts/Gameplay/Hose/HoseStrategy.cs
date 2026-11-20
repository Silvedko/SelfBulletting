using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Feedback;
using SelfBulletting.Gameplay.Obstacles;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Scanning;
using SelfBulletting.Gameplay.Score;
using SelfBulletting.Gameplay.Shot;
using SelfBulletting.Gameplay.Strategies;
using SelfBulletting.Infrastructure.Services.Audio;
using UnityEngine;

namespace SelfBulletting.Gameplay.Hose
{
    /// <summary>
    /// Hose mode: the whole round is one turn. While the finger pulls past the dead zone the ball pours itself out as a
    /// stream of drops; lifting the finger stops the water and the next press opens it again at once. The path is checked
    /// after every fire that goes out and whenever the water stops. The turn ends when the way is open or the water has
    /// run out, once the last drops have landed and the last fires have sunk; the round then decides the win.
    /// Fires put out together within the bonus window earn a bonus on top of their own points.
    /// </summary>
    public sealed class HoseStrategy : IShotStrategy
    {
        private const int NoHealthShown = -1;

        private readonly HoseAim _aim;
        private readonly HoseStream _stream;
        private readonly HosePreview _preview;
        private readonly DropSplash _splash;
        private readonly ExtinguishBurst _burst;
        private readonly PathCheck _pathCheck;
        private readonly BallModel _ball;
        private readonly DangerFeedback _danger;
        private readonly ScoreCounter _score;
        private readonly GameViews _views;
        private readonly IAudioService _audio;
        private readonly float _groundHeight;
        private readonly List<DropImpact> _impacts = new List<DropImpact>();
        private readonly Action<Obstacle> _onExtinguished;

        private CancellationToken _cancellationToken;
        private int _sinkingFires;
        private bool _isPathClear;
        private bool _hasFireGoneOut;
        private int _shownHealthPercent = NoHealthShown;
        private float _elapsedSeconds;

        /// <param name="groundHeight">World height of the ground: the splashes spread over it.</param>
        public HoseStrategy(HoseAim aim, HoseStream stream, HosePreview preview, DropSplash splash, ExtinguishBurst burst,
            PathCheck pathCheck, BallModel ball, DangerFeedback danger, ScoreCounter score, GameViews views,
            IAudioService audio, float groundHeight)
        {
            _aim = aim;
            _stream = stream;
            _preview = preview;
            _splash = splash;
            _burst = burst;
            _pathCheck = pathCheck;
            _ball = ball;
            _danger = danger;
            _score = score;
            _views = views;
            _audio = audio;
            _groundHeight = groundHeight;
            _onExtinguished = Extinguish;
        }

        private bool CanPour => !_isPathClear && _ball.HasRemainder;
        private bool IsTurnOver => !CanPour && _stream.IsEmpty && _sinkingFires == 0;

        public async UniTask PlayAsync(RaycastHit blocker, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _isPathClear = false;

            try
            {
                while (!IsTurnOver)
                {
                    Tick(Time.deltaTime);
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                // The last fires may still be inside the window: they count before the round is decided.
                if (_burst.TryCloseNow(out ExtinguishBurstResult lastBurst))
                {
                    AwardBurst(lastBurst);
                }
            }
            finally
            {
                _audio.StopLoop(SoundId.HoseStream);
                _stream.Clear();
                _preview.Hide();
                _views.Hose.Hide();
            }
        }

        private void Tick(float deltaTime)
        {
            // Closed before the drops fly, so a fire that goes out this frame never joins a burst whose window passed.
            _elapsedSeconds += deltaTime;
            if (_burst.TryClose(_elapsedSeconds, out ExtinguishBurstResult burst))
            {
                AwardBurst(burst);
            }

            bool isAimed = _aim.TryAim(out Vector3 direction, out float rangeShare);
            if (isAimed && CanPour)
            {
                Pour(direction, rangeShare, deltaTime);
            }
            else
            {
                StopWater();
            }

            FlyDrops(deltaTime);

            if (_hasFireGoneOut)
            {
                _hasFireGoneOut = false;
                CheckPath();
            }
        }

        private void Pour(Vector3 direction, float rangeShare, float deltaTime)
        {
            if (!_stream.IsFlowing)
            {
                _audio.StartLoop(SoundId.HoseStream);
            }

            ArcPath arc = _stream.Pour(direction, rangeShare, deltaTime, out float dropRadius);
            _preview.Show(arc, dropRadius);
            ShowDanger(_danger.Tick(_ball.RadiusPercent, deltaTime));
            ShowHealth(isForced: false);

            if (!_ball.HasRemainder)
            {
                StopWater();
            }
        }

        private void StopWater()
        {
            if (!_stream.IsFlowing)
            {
                return;
            }

            _stream.Stop();
            _audio.StopLoop(SoundId.HoseStream);
            _preview.Hide();
            _danger.Reset();
            ShowDanger(0f);
            ShowHealth(isForced: true);
            CheckPath();
        }

        // The ball shrinks while it pours, so the corridor it needs narrows as well.
        private void CheckPath()
        {
            if (_isPathClear || !_pathCheck.IsClear())
            {
                return;
            }

            _isPathClear = true;
            StopWater();
        }

        private void FlyDrops(float deltaTime)
        {
            _stream.Fly(deltaTime, _impacts);
            foreach (DropImpact impact in _impacts)
            {
                Vector3 center = impact.Center;
                _views.Hose.PlaySplash(center, new Vector3(center.x, _groundHeight, center.z), impact.DropRadius);
                _audio.Play(SoundId.HoseSplash);
                _splash.Apply(impact, _onExtinguished);
            }

            _views.Hose.ShowDrops(_stream.InFlight);
        }

        private void Extinguish(Obstacle fire)
        {
            _hasFireGoneOut = true;
            _audio.Play(SoundId.Explode);
            Vector3 position = fire.transform.position;
            _views.Score.ShowPoints(position, _score.AddObstacle());
            _burst.Add(_elapsedSeconds, position);
            SinkAsync(fire).Forget();
        }

        private void AwardBurst(ExtinguishBurstResult burst)
        {
            float multiplier = burst.Tier.Multiplier;
            int bonus = _score.AddExtinguishBonus(burst.Fires, multiplier);
            _views.Score.ShowCombo(burst.Center, multiplier, bonus);
            _audio.Play(ComboSounds.ForLevel(burst.Tier.Level));
        }

        // Starts at once, so the fire is out before any other drop of this frame can hit it.
        private async UniTaskVoid SinkAsync(Obstacle fire)
        {
            _sinkingFires++;
            try
            {
                await fire.ExplodeAsync(_cancellationToken);
            }
            finally
            {
                _sinkingFires--;
            }
        }

        private void ShowDanger(float danger)
        {
            _views.Ball.SetDanger(danger);
            _views.Path.SetDanger(danger);
        }

        // The mass is really spent while the water flows, so the HUD follows it, a whole percent at a time.
        private void ShowHealth(bool isForced)
        {
            int percent = Mathf.CeilToInt(_ball.RadiusPercent);
            if (!isForced && percent == _shownHealthPercent)
            {
                return;
            }

            _shownHealthPercent = percent;
            _views.Health.Show(_ball.RadiusPercent, _ball.Health);
        }
    }
}

using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Feedback;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Gameplay.Aim
{
    /// <summary>
    /// What the charge does to the player ball while aiming, in every mode: the ball shrinks to what is left after
    /// the shot, the path narrows with it, and both pulse when the preview gets close to the critical size.
    /// The mass is spent only when the shot is fired.
    /// </summary>
    public sealed class ChargePreview
    {
        private readonly ShotEconomy _economy;
        private readonly BallModel _ball;
        private readonly DangerFeedback _danger;
        private readonly GameViews _views;

        public ChargePreview(ShotEconomy economy, BallModel ball, DangerFeedback danger, GameViews views)
        {
            _economy = economy;
            _ball = ball;
            _danger = danger;
            _views = views;
        }

        /// <returns>Player ball radius once the shot has left it, meters.</returns>
        public float Show(ShotRequest request)
        {
            float previewPercent = _ball.RadiusPercent - request.CostPercent;
            float previewWorldRadius = _economy.ToWorldRadius(previewPercent);
            float danger = _danger.Tick(previewPercent, Time.deltaTime);

            _views.Ball.SetRadius(previewWorldRadius);
            _views.Ball.SetDanger(danger);
            _views.Path.SetWidth(previewWorldRadius);
            _views.Path.SetDanger(danger);
            return previewWorldRadius;
        }

        /// <summary>Back to the real ball size, calm. A fired shot then shrinks the ball for real.</summary>
        public void Reset()
        {
            _danger.Reset();
            _views.Ball.SetDanger(0f);
            _views.Path.SetDanger(0f);
            _views.Ball.SetRadius(_ball.WorldRadius);
            _views.Path.SetWidth(_ball.WorldRadius);
        }
    }
}

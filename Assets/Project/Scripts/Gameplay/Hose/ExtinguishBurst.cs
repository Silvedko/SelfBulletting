using SelfBulletting.Gameplay.Modes;
using SelfBulletting.Infrastructure.Services.Bonus;
using UnityEngine;

namespace SelfBulletting.Gameplay.Hose
{
    /// <summary>
    /// Groups the fires the Hose puts out into bursts: the first fire opens a burst, and every fire that goes out
    /// within the bonus window of that first one joins it. Once the window has passed the burst closes, and if it reached
    /// a bonus tier it is handed out once. A mode without the bonus never opens a burst.
    /// </summary>
    public sealed class ExtinguishBurst
    {
        private readonly IBonusService _bonus;
        private readonly GameMode _mode;
        private readonly bool _isEnabled;
        private readonly float _windowSeconds;

        private bool _isOpen;
        private float _openedAt;
        private int _fires;
        private Vector3 _positionSum;

        public ExtinguishBurst(IBonusService bonus, GameMode mode)
        {
            _bonus = bonus;
            _mode = mode;
            _isEnabled = bonus.HasExtinguishBonus(mode);
            _windowSeconds = _isEnabled ? bonus.GetExtinguishWindowSeconds(mode) : 0f;
        }

        /// <summary>
        /// A fire went out at <paramref name="time"/>. Call <see cref="TryClose"/> with the same or a later time first,
        /// so a fire past the window never joins the burst it missed.
        /// </summary>
        public void Add(float time, Vector3 position)
        {
            if (!_isEnabled)
            {
                return;
            }

            if (!_isOpen)
            {
                _isOpen = true;
                _openedAt = time;
                _fires = 0;
                _positionSum = Vector3.zero;
            }

            _fires++;
            _positionSum += position;
        }

        /// <summary>Closes the open burst once its window has passed.</summary>
        /// <returns>True when the closed burst reached a bonus tier.</returns>
        public bool TryClose(float time, out ExtinguishBurstResult result)
        {
            if (_isOpen && time - _openedAt >= _windowSeconds)
            {
                return Close(out result);
            }

            result = default;
            return false;
        }

        /// <summary>Closes the open burst at once, window or not: the turn is over.</summary>
        /// <returns>True when the closed burst reached a bonus tier.</returns>
        public bool TryCloseNow(out ExtinguishBurstResult result)
        {
            if (_isOpen)
            {
                return Close(out result);
            }

            result = default;
            return false;
        }

        private bool Close(out ExtinguishBurstResult result)
        {
            _isOpen = false;
            BonusTier tier = _bonus.GetExtinguishBonus(_mode, _fires);
            result = new ExtinguishBurstResult(_fires, _positionSum / _fires, tier);
            return tier.IsReached;
        }
    }

    /// <summary>A closed burst: how many fires went out together, their middle and the bonus tier they reached.</summary>
    public readonly struct ExtinguishBurstResult
    {
        public ExtinguishBurstResult(int fires, Vector3 center, BonusTier tier)
        {
            Fires = fires;
            Center = center;
            Tier = tier;
        }

        public int Fires { get; }
        public Vector3 Center { get; }
        public BonusTier Tier { get; }
    }
}

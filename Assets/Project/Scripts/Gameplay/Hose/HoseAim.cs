using SelfBulletting.Gameplay.Aim;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Infrastructure.Services.Input;
using UnityEngine;

namespace SelfBulletting.Gameplay.Hose
{
    /// <summary>
    /// The hose is aimed like the slingshot: the finger goes down anywhere and is pulled back, the water flies the
    /// opposite way and the pull length sets the range. There is water only past the dead zone. Only a press made during
    /// the turn counts, so a finger still down from before does not open the tap.
    /// </summary>
    public sealed class HoseAim
    {
        private readonly IInputService _input;
        private readonly IScreenToGround _screen;
        private readonly SlingshotPull _pull;

        private Vector2 _pressPoint;
        private bool _isHeld;
        private Vector3 _direction = Vector3.forward;

        public HoseAim(IInputService input, IScreenToGround screen, SlingshotPull pull)
        {
            _input = input;
            _screen = screen;
            _pull = pull;
        }

        /// <summary>Reads the finger for this frame; call it every frame of the turn.</summary>
        /// <param name="direction">Flat unit direction of the water.</param>
        /// <param name="rangeShare">0 — the shortest range, 1 — the longest.</param>
        /// <returns>False while the finger is up or inside the dead zone: no water then.</returns>
        public bool TryAim(out Vector3 direction, out float rangeShare)
        {
            if (_input.WasPressedThisFrame)
            {
                _pressPoint = _input.Position;
                _isHeld = true;
            }

            if (!_input.IsPressed)
            {
                _isHeld = false;
            }

            rangeShare = 0f;
            bool isAimed = _isHeld && _pull.TryAim(_screen, _pressPoint, _input.Position, ref _direction, out rangeShare);
            direction = _direction;
            return isAimed;
        }
    }
}

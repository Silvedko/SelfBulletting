using UnityEngine.InputSystem;

namespace SelfBulletting.Infrastructure.Services.Input
{
    /// <summary>
    /// Polls the current pointer instead of subscribing to callbacks, so there is nothing to unsubscribe.
    /// </summary>
    public sealed class PointerInputService : IInputService
    {
        public bool IsPressed => Pointer.current != null && Pointer.current.press.isPressed;
        public bool WasPressedThisFrame => Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
        public bool WasReleasedThisFrame => Pointer.current != null && Pointer.current.press.wasReleasedThisFrame;
    }
}

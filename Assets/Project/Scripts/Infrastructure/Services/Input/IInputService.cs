namespace SelfBulletting.Infrastructure.Services.Input
{
    /// <summary>
    /// One-finger input: a touch on the device, the mouse in the editor.
    /// </summary>
    public interface IInputService
    {
        bool IsPressed { get; }
        bool WasPressedThisFrame { get; }
        bool WasReleasedThisFrame { get; }
    }
}

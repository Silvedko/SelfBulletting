namespace SelfBulletting.Infrastructure.Services.Vibration
{
    /// <summary>
    /// Used in the editor and on platforms without vibration.
    /// </summary>
    public sealed class NullVibrationService : IVibrationService
    {
        public void Pulse(int milliseconds)
        {
        }
    }
}

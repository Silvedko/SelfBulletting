namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// The door at the finish. Gameplay opens it when the ball gets within <c>FinishSettings.DoorOpenDistance</c>.
    /// </summary>
    public interface IDoorView
    {
        void Open();
    }
}

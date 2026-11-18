namespace SelfBulletting.Infrastructure.Services.Audio
{
    public enum SoundId
    {
        UiClick,
        AimCharge,
        ShotFire,
        Infect,
        Explode,
        Combo,
        Warning,
        LastShot,
        DoorOpen,
        Hop,
        Win,
        Lose,

        // Appended, not inserted: SoundLibrary stores ids as numbers. Combo is the first tier, these are the higher ones.
        ComboBig,
        ComboHuge,

        // Hose mode: the stream loops while the water flows, every drop splashes where it hits.
        HoseStream,
        HoseSplash
    }
}

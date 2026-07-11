namespace CitySim.Data;

public static class Globals
{
    public const int TileSize = 32;

    public const float EnergyDecayRate = 0.000015f;
    public const float EnergyRecoverySleepRate = 0.00003f;
    public const float SatietyDecayRate = 0.000045f;
    public const float SocialDecayRate = 0.000025f;
    public const float SleepMetabolismFactor = 0.35f;
    public const float MinSocialNeed = 0.3f;
    public const float MinSatietyNeed = 0.3f;
    public const float MinEnergyNeed = 0.3f;
    public const float NeedScheduleCooldownHours = 1f;
    public const float MemoryLifespanPerUnit = 2000f; // +-5 satisfaction memory takes around a week to decay
    public const float CommuterDistancePenaltyPerUnit = 4f;
}

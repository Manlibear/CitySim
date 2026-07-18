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

    public const float MinJobPerformance = -20f;
    public const decimal ComfortableBalance = 1000m;

    public const int InteractionTileRange = 3;
    public const float SocialScoreSensitivity = 50f;

    // this will be effectively doubled as both social components in the pair will fire
    public const float SocialRelationshipPerSecond = .005f;
    public const float MoodNeedsModifier = .6f;
    public const float MoodMemoryMultiplier = .01f;
    public const float MoodInterviewModifier = .4f;
    public const float MoodSocialModifier = 30f;
    public const float MoodJobPerformanceModifier = .4f;

    // one sample every 15 sim-minutes, capped at 288 samples (~72 hours) of history
    public const float MoodSampleIntervalMinutes = 15f;
    public const int MoodHistoryMaxSamples = 288;
}

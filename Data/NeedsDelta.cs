namespace CitySim.Data;

public record NeedsDelta
{
    public float? SatietyDelta { get; set; }
    public float? EnergyDelta { get; set; }
    public float? SocialDelta { get; set; }
    public required float Duration { get; set; }
}

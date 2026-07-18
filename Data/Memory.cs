namespace CitySim.Data;

using System;

public record IMemory
{
    public float Satisfaction
    {
        // Ease-out toward 0 as Age approaches Lifespan — bigger OriginalSatisfaction means a
        // longer Lifespan, so severe memories fade slower than trivial ones.
        get
        {
            if (IsPermanent) return OriginalSatisfaction;

            var progress = Lifespan > 0f ? Math.Clamp(Age / Lifespan, 0f, 1f) : 1f;
            return OriginalSatisfaction * MathF.Pow(1f - progress, 2f);
        }
        internal set
        {
            Lifespan = Globals.MemoryLifespanPerUnit * Math.Abs(value);
            OriginalSatisfaction = value;

        }
    }

    public bool IsPermanent { get; set; }
    public float Age { get; set; }
    private float Lifespan { get; set; }
    private float OriginalSatisfaction { get; set; }

}

public record IShopMemory : IMemory
{

    public Guid EntityID { get; set; }
}

public record ShopQueryMemory : IShopMemory
{
    public Item? Item { get; set; }
    public string? Tag { get; set; }
    public ItemType? ItemType { get; set; }
    public bool Available { get; set; }
}

public record ConfidenceMemory : IMemory
{
    public ActivityType Type { get; set; }
}

public record FinancialMemory : IMemory { }

public record JobMemory : IMemory
{
    public required string Employer { get; set; }
}

public record ItemCostMemory : IShopMemory
{
    public Item Item { get; set; }
}

public record SocialInteractionMemory : IMemory
{
    public Guid OtherPersonID { get; set; }
}

public record HousingMemory : IMemory { }

public record NeedCrisisMemory : IMemory
{
    public NeedType Need { get; set; }
}

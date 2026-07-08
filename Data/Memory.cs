using System;
using CitySim.Data;

public record IMemory
{
    public float Satisfaction
    {
        // Ease-out toward 0 as Age approaches Lifespan — bigger OriginalSatisfaction means a
        // longer Lifespan (set elsewhere), so severe memories fade slower than trivial ones.
        get
        {
            var progress = Lifespan > 0f ? Math.Clamp(Age / Lifespan, 0f, 1f) : 1f;
            return OriginalSatisfaction * MathF.Pow(1f - progress, 2f);
        }
        internal set
        {
            Lifespan = Globals.MemoryLifespanPerUnit * Math.Abs(value);
            OriginalSatisfaction = value;

        }
    }

    public float Age { get; set; }
    private float Lifespan { get; set; }
    private float OriginalSatisfaction { get; set; }
}

public record ShopQueryMemory : IMemory
{
    public Guid EntityID { get; set; }
    public Item? Item { get; set; }
    public string? Tag { get; set; }
    public ItemType? ItemType { get; set; }
    public bool Available { get; set; }
}
